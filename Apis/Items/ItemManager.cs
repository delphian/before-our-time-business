using BeforeOurTime.Repository.Models.Items;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Business.Apis.Scripts;
using BeforeOurTime.Repository.Models.Scripts.Callbacks;
using System.Linq;
using BeforeOurTime.Repository.Models.Messages;

namespace BeforeOurTime.Business.Apis.Items
{
    public class ItemManager : IItemManager
    {
        private IItemRepo<Item> ItemRepo { set; get; }
        private IScriptManager ScriptManager { set; get; }
        private IApi Api { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scriptManager"></param>
        /// <param name="itemRepo"></param>
        /// <param name="api"></param>
        public ItemManager(
            IScriptManager scriptManager,
            IItemRepo<Item> itemRepo,
            IApi api)
        {
            ScriptManager = scriptManager;
            ItemRepo = itemRepo;
            Api = api;
        }
        /// <summary>
        /// Create a new item
        /// </summary>
        /// <remarks>
        /// Any item, or model derived from an item, may be created
        /// </remarks>
        /// <param name="source">Item responsible for doing the creating</param>
        /// <param name="item">Item which is new and being created</param>
        public bool Create<T>(Item source, T item) where T : Item
        {
            item.FunctionLinks = UpdateScriptCallbackLinks(item);
            ItemRepo.Create<T>(new List<T>() { item });
            return true;
        }
        /// <summary>
        /// Read single model of a type derived from Item
        /// </summary>
        /// <typeparam name="T">Subtype of item</typeparam>
        /// <param name="itemIds">Unique item identifier</param>
        /// <returns></returns>
        public T Read<T>(Guid itemId) where T : Item
        {
            return Read<T>(new List<Guid>() { itemId }).FirstOrDefault();
        }
        /// <summary>
        /// Read multiple items
        /// </summary>
        /// <typeparam name="T">Subtype of item</typeparam>
        /// <param name="itemIds">List of unique item identifiers</param>
        /// <returns></returns>
        public List<T> Read<T>(List <Guid> itemIds) where T : Item
        {
            return ItemRepo.Read<T>(itemIds);
        }
        /// <summary>
        /// Read all models of a type derived from Item, or specify an offset and limit
        /// </summary>
        /// <remarks>
        /// If repository was instanted with type Item (T) then this method may be used to 
        /// read any model (TDerived) that is derived from type T (Item)
        /// </remarks>
        /// <typeparam name="T">Subtype of item</typeparam>
        /// <param name="offset">Number of model records to skip</param>
        /// <param name="limit">Maximum number of model records to return</param>
        /// <returns></returns>
        public List<T> Read<T>(int? offset = null, int? limit = null) where T : Item
        {
            return ItemRepo.Read<T>(offset, limit);
        }
        /// <summary>
        /// Update any model that is derived from type Item
        /// </summary>
        /// <typeparam name="TDerived">Type derived from item</typeparam>
        /// <param name="item">Item to be updated</param>
        /// <returns></returns>
        public Item Update<T>(T item) where T : Item
        {
            item.FunctionLinks = UpdateScriptCallbackLinks(item);
            return ItemRepo.Update<T>(new List<T>() { item }).FirstOrDefault();
        }
        /// <summary>
        /// Generate callback links by parsing an item's script callback function definitions
        /// </summary>
        /// <param name="item">Item to generate callback links for</param>
        /// <returns></returns>
        protected List<ScriptCallbackItemLink> UpdateScriptCallbackLinks(Item item)
        {
            var callbackLinks = new List<ScriptCallbackItemLink>();
            var invalidCallbacks = ScriptManager.GetInvalidCallbacks(item.Script);
            if (invalidCallbacks.Count > 0)
            {
                var callbackStrs = "";
                invalidCallbacks.ForEach(delegate (ScriptCallback invalidCallback)
                {
                    callbackStrs += (callbackStrs.Length == 0) ? invalidCallback.FunctionName :
                                                               ", " + invalidCallback.FunctionName;
                });
                throw new Exception("Improperly declared script callback functions: " + callbackStrs);
            }
            ScriptManager.GetCallbacks(item.Script).ForEach(delegate (ScriptCallback callback)
            {
                callbackLinks.Add(new ScriptCallbackItemLink()
                {
                    Item = item,
                    Callback = callback
                });
            });
           return callbackLinks;
        }
        /// <summary>
        /// Relocate an item
        /// </summary>
        /// <param name="item">Item to move</param>
        /// <param name="newParent">Item which will become the parent</param>
        /// <param name="source">Item responsible for doing the moving</param>
        public Item Move(Item item, Item newParent, Item source = null)
        {
            // Construct the message
            var message = new Message()
            {
                Sender = source,
                Callback = ScriptManager.GetCallbackDefinition("onItemMove"),
                Package = "",
                //JsonConvert.SerializeObject(new OnItemMove()
                //{
                //    Type = MessageType.EventItemMove,
                //    From = child.Parent,
                //    To = newParent,
                //    Item = child
                //})
            };
            // Move the item
            var oldParent = item.Parent;
            oldParent?.Children.Remove(item);
            newParent.Children.Add(item);
            item.Parent = newParent;
            var updateItems = new List<Item>() { newParent, item };
            if (oldParent != null)
            {
                updateItems.Add(oldParent);
            }
            ItemRepo.Update(updateItems);
            // Distribute message
            var recipients = new List<Item>() { item, newParent };
            if (oldParent != null)
            {
                recipients.Add(oldParent);
            }
            Api.SendMessage(message, recipients);
            return item;
        }
    }
}
