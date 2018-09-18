using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.ItemAttributes;
using BeforeOurTime.Models.ItemAttributes.Physicals;
using BeforeOurTime.Models.Primitives.Images;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Models.ItemAttributes.Visibles;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Physicals
{
    public class VisibleAttributeManager : AttributeManager<VisibleAttribute>, IVisibleAttributeManager
    {
        private IVisibleAttributeRepo VisibleAttributeRepo { set; get; }
        private IItemManager ItemManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public VisibleAttributeManager(
            IItemRepo itemRepo,
            IVisibleAttributeRepo visibleAttributeRepo,
            IItemManager itemManager) : base(itemRepo, visibleAttributeRepo)
        {
            VisibleAttributeRepo = visibleAttributeRepo;
            ItemManager = itemManager;
        }
        /// <summary>
        /// Determine if an item has attributes that may be managed
        /// </summary>
        /// <param name="item">Item that may posses attributes</param>
        public bool IsManaging(Item item)
        {
            var managed = false;
            if (VisibleAttributeRepo.Read(item) != null)
            {
                managed = true;
            }
            return managed;
        }
    }
}
