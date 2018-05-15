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
    public class ItemManager : IItemManager
    {
        private IItemRepo<Item> ItemRepo { set; get; }
        private IItemLocationRepo ItemLocationRepo { set; get; }
        private IMessageManager MessageManager { set; get; }
        private IScriptManager ScriptManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scriptManager"></param>
        /// <param name="itemRepo"></param>
        /// <param name="api"></param>
        public ItemManager(
            IItemRepo<Item> itemRepo,
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
        /// <remarks>
        /// Any item, or model derived from an item, may be created
        /// </remarks>
        /// <param name="source">Item responsible for doing the creating</param>
        /// <param name="item">Item which is new and being created</param>
        public T Create<T>(Item source, T item) where T : Item
        {
            item.FunctionLinks = UpdateScriptCallbackLinks(item);
            ItemRepo.Create<T>(new List<T>() { item });
            return item;
        }
        /// <summary>
        /// Create a new character
        /// </summary>
        /// <param name="name">Public name of the character</param>
        /// <param name="accountId">Account to which this character belongs</param>
        /// <param name="parentId">Location of new item</param>
        public Character CreateCharacter(
            string name,
            Guid accountId,
            Guid parentId)
        {
            var parent = Read<Item>(parentId);
            var game = Read<Item>(new Guid("487a7282-0cad-4081-be92-83b14671fc23"));
            var character = new Character()
            {
                Name = name,
                AccountId = accountId,
                Type = ItemType.Character,
                UuidType = Guid.NewGuid(),
                ParentId = parentId,
                Data = "{}",
                Script = "{ function onTick(e) {}; function onTerminalOutput(e) { terminalMessage(e.terminal.id, e.raw); }; function onItemMove(e) { }; }"
            };
            Create<Character>(game, character);
            return character;
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
        public T Update<T>(T item) where T : Item
        {
            item.FunctionLinks = UpdateScriptCallbackLinks(item);
            return ItemRepo.Update<T>(new List<T>() { item }).FirstOrDefault();
        }
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
            MessageManager.SendMessage(message, recipients);
            return item;
        }
    }
}
