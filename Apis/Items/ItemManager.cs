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
using BeforeOurTime.Business.Apis.Messages;

namespace BeforeOurTime.Business.Apis.Items
{
    /// <summary>
    /// Manages security, environment messages, CRUD, and run time considerations for items
    /// </summary>
    public class ItemManager : IItemManager
    {
        private IItemRepo ItemRepo { set; get; }
        private IMessageManager MessageManager { set; get; }
        private IScriptManager ScriptManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemRepo"></param>
        /// <param name="scriptManager"></param>
        /// <param name="messageManager"></param>
        public ItemManager(
            IItemRepo itemRepo,
            IMessageManager messageManager,
            IScriptManager scriptManager)
        {
            ItemRepo = itemRepo;
            MessageManager = messageManager;
            ScriptManager = scriptManager;
        }
        /// <summary>
        /// Create a new item
        /// </summary>
        /// <param name="item">Item which is new and being created</param>
        public Item Create(Item item)
        {
            item.FunctionLinks = UpdateScriptCallbackLinks(item);
            ItemRepo.Create(new List<Item>() { item });
            return item;
        }
        /// <summary>
        /// Read single model of a type derived from Item
        /// </summary>
        /// <param name="itemIds">Unique item identifier</param>
        /// <returns></returns>
        public Item Read(Guid itemId)
        {
            return Read(new List<Guid>() { itemId }).FirstOrDefault();
        }
        /// <summary>
        /// Read multiple models derived from Item
        /// </summary>
        /// <param name="itemIds">List of unique item identifiers</param>
        /// <returns></returns>
        public List<Item> Read(List <Guid> itemIds)
        {
            return ItemRepo.Read(itemIds);
        }
        /// <summary>
        /// Read all models derived from Item, or specify an offset and limit
        /// </summary>
        /// <param name="offset">Number of model records to skip</param>
        /// <param name="limit">Maximum number of model records to return</param>
        /// <returns></returns>
        public List<Item> Read(int? offset = null, int? limit = null)
        {
            return ItemRepo.Read(offset, limit);
        }
        /// <summary>
        /// Update any model that is derived from Item
        /// </summary>
        /// <param name="item">Item to be updated</param>
        /// <returns></returns>
        public Item Update(Item item)
        {
            item.FunctionLinks = UpdateScriptCallbackLinks(item);
            return ItemRepo.Update(new List<Item>() { item }).FirstOrDefault();
        }
        /// <summary>
        /// Permenantly delete an item and remove from data store
        /// </summary>
        /// <remarks>
        /// All children will be re-homed to the item parent unless otherwise specified
        /// </remarks>
        /// <param name="item">Item to delete</param>
        /// <param name="deleteChildren">Also delete all children</param>
        public void Delete(Item item, bool? deleteChildren = false)
        {
            // Move the item
            var oldParent = item.Parent;
            item.Children.ForEach(delegate (Item child)
            {
                Move(child, oldParent, item);
            });
            ItemRepo.Delete(new List<Item>() { item });
        }
        /// <summary>
        /// Generate callback links by parsing an item's script callback function definitions
        /// </summary>
        /// <param name="item">Item to generate callback links for</param>
        /// <returns></returns>
        protected List<ScriptCallbackItemLink> UpdateScriptCallbackLinks(Item item)
        {
            var callbackLinks = new List<ScriptCallbackItemLink>();
            var invalidCallbacks = ScriptManager.GetScriptInvalidDelegates(item.Script);
            if (invalidCallbacks.Count > 0)
            {
                var callbackStrs = "";
                invalidCallbacks.ForEach(delegate (ICallback invalidCallback)
                {
                    callbackStrs += (callbackStrs.Length == 0) ? invalidCallback.GetFunctionName() :
                                                               ", " + invalidCallback.GetFunctionName();
                });
                throw new Exception("Improperly declared script callback functions: " + callbackStrs);
            }
            ScriptManager.GetScriptValidDelegates(item.Script).ForEach(delegate (ICallback callback)
            {
                callbackLinks.Add(new ScriptCallbackItemLink()
                {
                    Item = item,
                    CallbackId = callback.GetId()
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
                CallbackId = ScriptManager.GetDelegateDefinition("onItemMove").GetId(),
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
            MessageManager.SendMessage(message, recipients);
            return item;
        }
    }
}
