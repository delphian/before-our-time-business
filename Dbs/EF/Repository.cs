using BeforeOurTime.Models;
using BeforeOutTime.Business.Dbs.EF;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Dbs.EF
{
    public class Repository<T> : IRepository<T> where T : Model, IModel
    {
        protected BaseContext Db { set; get; }
        /// <summary>
        /// Single data set (table)
        /// </summary>
        protected DbSet<T> Set { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">Entity framework database context</param>
        public Repository(BaseContext db, DbSet<T> dbSet)
        {
            Db = db;
            Set = dbSet;
        }
        /// <summary>
        /// Create multiple models
        /// </summary>
        /// <remarks>
        /// All other forms of create should call this one
        /// </remarks>
        /// <param name="models">List of models to create</param>
        /// <returns>List of models created</returns>
        public virtual List<T> Create(List<T> models)
        {
            Set.AddRange(models);
            Db.SaveChanges();
            return models;
        }
        /// <summary>
        /// Create single model
        /// </summary>
        /// <param name="model">Model to create</param>
        /// <returns>Model created</returns>
        public T Create(T model)
        {
            return Create(new List<T>() { model }).FirstOrDefault();
        }
        /// <summary>
        /// Read multiple models
        /// </summary>
        /// <remarks>
        /// All other forms of read should call this one
        /// </remarks>
        /// <param name="ids">List of unique model identifiers</param>
        /// <returns>List of models</returns>
        public virtual List<T> Read(List<Guid> ids)
        {
            var resultSet = Set
                .Where(x => ids.Contains(x.Id)).AsNoTracking();
            return resultSet.ToList();
        }
        /// <summary>
        /// Read a single model
        /// </summary>
        /// <param name="id">Unique model identifier</param>
        /// <returns>Single model</returns>
        public T Read(Guid id)
        {
            return Read(new List<Guid>() { id }).FirstOrDefault();
        }
        /// <summary>
        /// Read all model records, or specify an offset and limit
        /// </summary>
        /// <param name="offset">Number of model records to skip</param>
        /// <param name="limit">Maximum number of model records to return</param>
        /// <returns>List of models</returns>
        public List<T> Read(int? offset = null, int? limit = null)
        {
            IQueryable<T> modelQuery = Set;
            if (offset != null)
            {
                modelQuery = modelQuery.Skip(offset.Value);
            }
            if (limit != null)
            {
                modelQuery = modelQuery.Take(limit.Value);
            }
            List<Guid> modelIds = modelQuery.Select(x => x.Id).ToList();
            List<T> models = Read(modelIds);
            return models;
        }
        /// <summary>
        /// Update a list of models
        /// </summary>
        /// <remarks>
        /// All other forms of update should call this one
        /// </remarks>
        /// <param name="models">List of models to update</param>
        /// <returns>List of models updated</returns>
        public virtual List<T> Update(List<T> models)
        {
            models.ForEach((model) =>
            {
                var trackedModel = Set.Where(x => x.Id == model.Id).First();
                Db.Entry(trackedModel).CurrentValues.SetValues(model);
                Db.SaveChanges();
            });
            return models;
        }
        /// <summary>
        /// Update single model
        /// </summary>
        /// <param name="model">Model to update</param>
        /// <returns>Model updated</returns>
        public T Update(T model)
        {
            return Update(new List<T>() { model }).FirstOrDefault();
        }
        /// <summary>
        /// Delete multiple models
        /// </summary>
        /// <remarks>
        /// All other forms of delete should call this one
        /// </remarks>
        /// <param name="models">List of models to delete</param>
        public virtual void Delete(List<T> models)
        {
            models.ForEach((model) =>
            {
                Set.Remove(model);
            });
            Db.SaveChanges();
        }
        /// <summary>
        /// Delete single model
        /// </summary>
        /// <param name="model">Model to delete</param>
        public void Delete(T model)
        {
            Delete(new List<T>() { model });
        }
        /// <summary>
        /// Delete all models
        /// </summary>
        public void Delete() 
        {
            Set.RemoveRange(Read());
            Db.SaveChanges();
        }
    }
}
