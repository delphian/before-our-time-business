using BeforeOurTime.Business.Apis.Items.Attributes.Interfaces;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Items.Attributes;
using BeforeOurTime.Repository.Dbs.EF;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes
{
    /// <summary>
    /// Manage details of an item's extended attributes
    /// </summary>
    public class AttributeManager<T> : IAttributeManager<T> where T : ItemAttribute
    {
        protected IAttributeRepository<T> AttributeRepo { set; get; }
        public AttributeManager(IAttributeRepository<T> attributeRepo)
        {
            AttributeRepo = attributeRepo;
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
            return AttributeRepo.Read(ids);
        }

        public T Read(Guid id)
        {
            return Read(new List<Guid>() { id }).FirstOrDefault();
        }

        public List<T> Read(int? offset = null, int? limit = null)
        {
            return AttributeRepo.Read(offset, limit);
        }

        public T Read(Item item)
        {
            return AttributeRepo.Read(item);
        }

        public List<T> Update(List<T> models)
        {
            return AttributeRepo.Update(models);
        }

        public T Update(T model)
        {
            return Update(new List<T>() { model }).FirstOrDefault();
        }

        public void Delete(List<T> models)
        {
            AttributeRepo.Delete(models);
        }

        public void Delete(T model)
        {
            Delete(new List<T>() { model });
        }

        public void Delete()
        {
            AttributeRepo.Delete();
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
