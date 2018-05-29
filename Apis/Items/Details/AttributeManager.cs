using BeforeOurTime.Repository.Dbs.EF;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Details;
using BeforeOurTime.Repository.Models.Items.Details.Repos;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Details
{
    /// <summary>
    /// Manage details of an item's extended attributes
    /// </summary>
    public class AttributeManager<T> : IDetailManager<T> where T : Detail
    {
        private IAttributeRepository<T> AttributeRepo { set; get; }
        public AttributeManager(IAttributeRepository<T> attributeRepo)
        {
            AttributeRepo = attributeRepo;
        }

        public T Read(Item item)
        {
            throw new NotImplementedException();
        }

        public List<T> Create(List<T> models)
        {
            return AttributeRepo.Create(models);
        }

        public T Create(T model)
        {
            return AttributeRepo.Create(model);
        }

        public List<T> Read(List<Guid> ids)
        {
            throw new NotImplementedException();
        }

        public T Read(Guid id)
        {
            throw new NotImplementedException();
        }

        public List<T> Read(int? offset = null, int? limit = null)
        {
            throw new NotImplementedException();
        }

        public List<T> Update(List<T> models)
        {
            throw new NotImplementedException();
        }

        public T Update(T model)
        {
            throw new NotImplementedException();
        }

        public void Delete(List<T> models)
        {
            throw new NotImplementedException();
        }

        public void Delete(T model)
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Attach new attributes to an existing item
        /// </summary>
        /// <param name="attributes">Unsaved new attributes</param>
        /// <param name="item">Existing item that has already been saved</param>
        /// <returns></returns>
        public T Attach(T attributes, Item item)
        {
            attributes.Item = item;
            attributes = AttributeRepo.Create(attributes);
            return attributes;
        }
    }
}
