using BeforeOurTime.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Dbs
{
    /// <summary>
    /// CRUD Data store operations
    /// </summary>
    public interface ICrudDataRepository
    {
    }
    /// <summary>
    /// CRUD Data store operations for a specified data type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICrudDataRepository<T> : IDataRepository where T : Model
    {
        /// <summary>
        /// Create multiple models
        /// </summary>
        /// <remarks>
        /// All other forms of create should call this one
        /// </remarks>
        /// <param name="models">List of models to create</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>List of models created</returns>
        List<T> Create(List<T> models, TransactionOptions options = null);
        /// <summary>
        /// Create single model
        /// </summary>
        /// <param name="model">Model to create</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>Model created</returns>
        T Create(T model, TransactionOptions options = null);
        /// <summary>
        /// Read multiple models
        /// </summary>
        /// <remarks>
        /// All other forms of read should call this one
        /// </remarks>
        /// <param name="ids">List of unique model identifiers</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>List of models</returns>
        List<T> Read(List<Guid> ids, TransactionOptions options = null);
        /// <summary>
        /// Read single model
        /// </summary>
        /// <param name="id">Unique model identifier</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>Single model</returns>
        T Read(Guid id, TransactionOptions options = null);
        /// <summary>
        /// Read all models, or specify an offset and limit
        /// </summary>
        /// <param name="offset">Number of model records to skip</param>
        /// <param name="limit">Maximum number of model records to return</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>List of models</returns>
        List<T> Read(int? offset = null, int? limit = null, TransactionOptions options = null);
        /// <summary>
        /// Update multiple models
        /// </summary>
        /// <remarks>
        /// All other forms of update should call this one
        /// </remarks>
        /// <param name="models">List of models to update</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>List of models updated</returns>
        List<T> Update(List<T> models, TransactionOptions options = null);
        /// <summary>
        /// Update single model
        /// </summary>
        /// <param name="model">Model to update</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns>Updated model</returns>
        T Update(T model, TransactionOptions options = null);
        /// <summary>
        /// Delete multiple models
        /// </summary>
        /// <remarks>
        /// All other forms of delete should call this one
        /// </remarks>
        /// <param name="models">List of models to delete</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        void Delete(List<T> models, TransactionOptions options = null);
        /// <summary>
        /// Delete single model
        /// </summary>
        /// <param name="model">Model to delete</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        void Delete(T model, TransactionOptions options = null);
        /// <summary>
        /// Delete all models
        /// </summary>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        void Delete(TransactionOptions options = null);
    }
}
