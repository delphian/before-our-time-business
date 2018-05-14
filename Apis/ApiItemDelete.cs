using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Messages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Repository.Models;

namespace BeforeOurTime.Business.Apis
{
    /// <summary>
    /// Interface into the game
    /// </summary>
    public partial class Api
    {
        /// <summary>
        /// Permenantly delete an item and remove from data store
        /// </summary>
        /// <remarks>
        /// All children will be re-homed to the item parent unless otherwise specified
        /// </remarks>
        /// <param name="item">Item to delete</param>
        /// <param name="deleteChildren">Also delete all children</param>
        public void ItemDelete(Item item, bool? deleteChildren = false)
        {
            // Move the item
            var oldParent = item.Parent;
            item.Children.ForEach(delegate (Item child)
            {
                GetItemManager().Move(child, oldParent, item);
            });
            ItemRepo.Delete(new List<Item>() { item });
        }
    }
}
