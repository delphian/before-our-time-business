using BeforeOurTime.Repository.Models.Items;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Business.Apis.Scripts;
using BeforeOurTime.Repository.Models.Scripts.Callbacks;

namespace BeforeOurTime.Business.Apis.Items
{
    public class ItemManager : IItemManager
    {
        protected IScriptManager ScriptManager { set; get; }
        protected IItemRepo<Item> ItemRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scriptManager"></param>
        /// <param name="itemRepo"></param>
        public ItemManager(
            IScriptManager scriptManager,
            IItemRepo<Item> itemRepo)
        {
            ItemRepo = itemRepo;
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
            var created = false;
            var invalidCallbacks = ScriptManager.GetInvalidCallbacks(item.Script);
            if (invalidCallbacks.Count == 0)
            {
                item.FunctionLinks = new List<ScriptCallbackItemLink>();
                ScriptManager.GetCallbacks(item.Script).ForEach(delegate (ScriptCallback callback)
                {
                    item.FunctionLinks.Add(new ScriptCallbackItemLink()
                    {
                        Item = item,
                        Callback = callback
                    });
                });
                ItemRepo.Create<T>(new List<T>() { item });
                // TODO : Construct item manifest message
                created = true;
            } else
            {
                var handlersStr = "";
                invalidCallbacks.ForEach(delegate (ScriptCallback invalidCallback)
                {
                    handlersStr += (handlersStr.Length == 0) ? invalidCallback.FunctionName :
                                                               ", " + invalidCallback.FunctionName;
                });
                Console.WriteLine(item.Id + " has declared invalid function callbacks: " + handlersStr);
            }
            return created;
        }
    }
}
