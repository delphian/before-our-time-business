using BeforeOurTime.Models;
using BeforeOurTime.Repository.Models;
using BeforeOutTime.Repository.Dbs.EF;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Repository.Dbs.EF
{
    public class Repository<T> : IRepository<T> where T : Model, IModel
    {
        internal BaseContext Db { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">Entity framework database context</param>
        public Repository(BaseContext db)
        {
            Db = db;
        }
        /// <summary>
        /// Create multiple models
        /// </summary>
        /// <remarks>
        /// All other forms of create should call this one
        /// </remarks>
        /// <param name="models">List of models to create</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>List of models created</returns>
        public virtual List<T> Create(List<T> models, TransactionOptions options = null)
        {
            options = options ?? new TransactionOptions()
            {
                NoTracking = true
            };
            Db.GetDbSet<T>().AddRange(models);
            Db.SaveChanges();
            if (options.NoTracking == true)
            {
                models.ForEach((model) =>
                {
                    Db.Entry(model).State = EntityState.Detached;
                });
            }
            return models;
        }
        /// <summary>
        /// Create single model
        /// </summary>
        /// <param name="model">Model to create</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>Model created</returns>
        public T Create(T model, TransactionOptions options = null)
        {
            return Create(new List<T>() { model }, options).FirstOrDefault();
        }
        /// <summary>
        /// Read multiple models
        /// </summary>
        /// <remarks>
        /// All other forms of read should call this one
        /// </remarks>
        /// <param name="ids">List of unique model identifiers</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>List of models</returns>
        public virtual List<T> Read(List<Guid> ids, TransactionOptions options = null)
        {
            options = options ?? new TransactionOptions()
            {
                NoTracking = true
            };
            var resultSet = Db.GetDbSet<T>()
                .Where(x => ids.Contains(x.Id));
            resultSet = (options?.NoTracking == true) ? resultSet.AsNoTracking() : resultSet;
            return resultSet.ToList();
        }
        /// <summary>
        /// Read a single model
        /// </summary>
        /// <param name="id">Unique model identifier</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>Single model</returns>
        public T Read(Guid id, TransactionOptions options = null)
        {
            return Read(new List<Guid>() { id }, options).FirstOrDefault();
        }
        /// <summary>
        /// Read all model records, or specify an offset and limit
        /// </summary>
        /// <param name="offset">Number of model records to skip</param>
        /// <param name="limit">Maximum number of model records to return</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>List of models</returns>
        public List<T> Read(int? offset = null, int? limit = null, TransactionOptions options = null)
        {
            IQueryable<T> modelQuery = Db.GetDbSet<T>();
            if (offset != null)
            {
                modelQuery = modelQuery.Skip(offset.Value);
            }
            if (limit != null)
            {
                modelQuery = modelQuery.Take(limit.Value);
            }
            List<Guid> modelIds = modelQuery.Select(x => x.Id).ToList();
            List<T> models = Read(modelIds, options);
            return models;
        }
        /// <summary>
        /// Update a list of models
        /// </summary>
        /// <remarks>
        /// All other forms of update should call this one
        /// </remarks>
        /// <param name="models">List of models to update</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>List of models updated</returns>
        public virtual List<T> Update(List<T> models, TransactionOptions options = null)
        {
            options = options ?? new TransactionOptions()
            {
                NoTracking = true
            };
            models.ForEach((model) =>
            {
                var trackedModel = Read(model.Id, new TransactionOptions() { NoTracking = false });
                if (trackedModel == null)
                {
                    throw new Exception("No such model exists " + typeof(T).ToString() + " " + model?.Id);
                }
                Db.Entry(trackedModel).CurrentValues.SetValues(model);
                Db.SaveChanges();
            });
            return models;
        }
        /// <summary>
        /// Update single model
        /// </summary>
        /// <param name="model">Model to update</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>Model updated</returns>
        public T Update(T model, TransactionOptions options = null)
        {
            return Update(new List<T>() { model }, options).FirstOrDefault();
        }
        /// <summary>
        /// Delete multiple models
        /// </summary>
        /// <remarks>
        /// All other forms of delete should call this one
        /// </remarks>
        /// <param name="models">List of models to delete</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public virtual void Delete(List<T> models, TransactionOptions options = null)
        {
            models.ForEach((model) =>
            {
                var trackedModel = Read(model.Id, new TransactionOptions() { NoTracking = false });
                if (trackedModel == null)
                {
                    throw new Exception("Attempting to delete untracked model");
                }
                Db.GetDbSet<T>().RemoveRange(trackedModel);
            });
            Db.SaveChanges();
        }
        /// <summary>
        /// Delete single model
        /// </summary>
        /// <param name="model">Model to delete</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void Delete(T model, TransactionOptions options = null)
        {
            Delete(new List<T>() { model }, options);
        }
        /// <summary>
        /// Delete all models
        /// </summary>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void Delete(TransactionOptions options = null) 
        {
            Db.GetDbSet<T>().RemoveRange(Read());
            Db.SaveChanges();
        }
    }
}
