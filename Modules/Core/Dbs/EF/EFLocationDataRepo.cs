using BeforeOurTime.Models;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Modules.Core.Dbs;
using BeforeOurTime.Models.Modules.Core.Models.Data;
using BeforeOutTime.Repository.Dbs.EF;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Modules.Core.Dbs.EF
{
    /// <summary>
    /// Central data repository for all location items
    /// </summary>
    public class EFLocationDataRepo : ILocationDataRepo
    {
        /// <summary>
        /// Date store context
        /// </summary>
        private EFCoreModuleContext Db { set; get; }
        /// <summary>
        /// Single data set (table)
        /// </summary>
        private DbSet<LocationData> Set { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">Entity framework database context</param>
        public EFLocationDataRepo(
            EFCoreModuleContext db,
            IItemRepo itemRepo)
        {
            Db = db;
            Set = Db.GetDbSet<LocationData>();
        }
        /// <summary>
        /// Read associated Location attributes of item
        /// </summary>
        /// <param name="item">Item that may have associated attributes</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns></returns>
        public LocationData Read(Item item, TransactionOptions options = null)
        {
            var dataId = Set.Where(x => x.DataItemId == item.Id).Select(x => x.Id).FirstOrDefault();
            return Read(dataId, options);
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
        public virtual List<LocationData> Create(List<LocationData> models, TransactionOptions options = null)
        {
            options = options ?? new TransactionOptions()
            {
                NoTracking = true
            };
            Set.AddRange(models);
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
        public LocationData Create(LocationData model, TransactionOptions options = null)
        {
            return Create(new List<LocationData>() { model }, options).FirstOrDefault();
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
        public virtual List<LocationData> Read(List<Guid> ids, TransactionOptions options = null)
        {
            options = options ?? new TransactionOptions()
            {
                NoTracking = true
            };
            var resultSet = Db.GetDbSet<LocationData>()
                .Where(x => ids.Contains(x.Id));
            resultSet = (options?.NoTracking == true) ? resultSet.AsNoTracking() : resultSet.AsTracking();
            return resultSet.ToList();
        }
        /// <summary>
        /// Read a single model
        /// </summary>
        /// <param name="id">Unique model identifier</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>Single model</returns>
        public LocationData Read(Guid id, TransactionOptions options = null)
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
        public List<LocationData> Read(int? offset = null, int? limit = null, TransactionOptions options = null)
        {
            IQueryable<LocationData> modelQuery = Db.GetDbSet<LocationData>();
            if (offset != null)
            {
                modelQuery = modelQuery.Skip(offset.Value);
            }
            if (limit != null)
            {
                modelQuery = modelQuery.Take(limit.Value);
            }
            List<Guid> modelIds = modelQuery.Select(x => x.Id).ToList();
            List<LocationData> models = Read(modelIds, options);
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
        public virtual List<LocationData> Update(List<LocationData> models, TransactionOptions options = null)
        {
            models.ForEach((model) =>
            {
                var trackedModel = Read(model.Id, new TransactionOptions() { NoTracking = false });
                if (trackedModel == null)
                {
                    throw new Exception("No such model exists " + typeof(LocationData).ToString() + " " + model?.Id);
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
        public LocationData Update(LocationData model, TransactionOptions options = null)
        {
            return Update(new List<LocationData>() { model }, options).FirstOrDefault();
        }
        /// <summary>
        /// Delete multiple models
        /// </summary>
        /// <remarks>
        /// All other forms of delete should call this one
        /// </remarks>
        /// <param name="models">List of models to delete</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public virtual void Delete(List<LocationData> models, TransactionOptions options = null)
        {
            models.ForEach((model) =>
            {
                var trackedModel = Read(model.Id, new TransactionOptions() { NoTracking = false });
                if (trackedModel == null)
                {
                    throw new Exception("Attempting to delete untracked model");
                }
                Db.GetDbSet<LocationData>().RemoveRange(trackedModel);
            });
            Db.SaveChanges();
        }
        /// <summary>
        /// Delete single model
        /// </summary>
        /// <param name="model">Model to delete</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void Delete(LocationData model, TransactionOptions options = null)
        {
            Delete(new List<LocationData>() { model }, options);
        }
        /// <summary>
        /// Delete all models
        /// </summary>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void Delete(TransactionOptions options = null)
        {
            Db.GetDbSet<LocationData>().RemoveRange(Read());
            Db.SaveChanges();
        }
        /// <summary>
        /// Get all unique item identifiers of managed items
        /// </summary>
        /// <returns></returns>
        public List<Guid> GetItemIds()
        {
            return Set.Select(x => x.DataItemId).ToList();
        }
    }
}
