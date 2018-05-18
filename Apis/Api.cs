using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BeforeOurTime.Business.Apis.Accounts;
using BeforeOurTime.Business.Apis.Scripts;
using BeforeOurTime.Business.Apis.Items;
using BeforeOurTime.Business.Apis.Messages;
using BeforeOurTime.Business.Apis.Items.Games;
using BeforeOurTime.Business.Apis.Items.Characters;
using BeforeOurTime.Business.Apis.Items.Locations;

namespace BeforeOurTime.Business.Apis
{
    /// <summary>
    /// Interface to the core environment
    /// </summary>
    public partial class Api : IApi
    {
        private Dictionary<Type, IItemManager> ItemManagerList = new Dictionary<Type, IItemManager>();
        private IMessageManager MessageManager { set; get; }
        private IAccountManager AccountManager { set; get; }
        private IScriptManager ScriptManager { set; get; }
        private IItemManager ItemManager { set; get; }
        private IItemGameManager ItemGameManager { set; get; }
        private IItemCharacterManager ItemCharacterManager { set; get; }
        private IItemLocationManager ItemLocationManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemRepo"></param>
        /// <param name="messageRepo"></param>
        public Api(
            IMessageManager messageManager,
            IAccountManager accountManager,
            IScriptManager scriptManager,
            IItemManager itemManager,
            IItemGameManager itemGameManager,
            IItemCharacterManager itemCharacterManager,
            IItemLocationManager itemLocationManager)
        {
            MessageManager = messageManager;
            AccountManager = accountManager;
            ScriptManager = scriptManager;
            ItemManager = itemManager;
            ItemManagerList.Add(typeof(IItemGameManager), itemGameManager);
            ItemManagerList.Add(typeof(IItemCharacterManager), itemCharacterManager);
            ItemManagerList.Add(typeof(IItemLocationManager), itemLocationManager);
        }
        public IMessageManager GetMessageManager()
        {
            return MessageManager;
        }
        public IAccountManager GetAccountManager()
        {
            return AccountManager;
        }
        public IScriptManager GetScriptManager()
        {
            return ScriptManager;
        }
        public IItemManager GetItemManager()
        {
            return ItemManager;
        }
        public T GetItemManager<T>() where T : IItemManager
        {
            return (T)ItemManagerList.Where(x => x.Key == typeof(T)).Select(x => x.Value).First();
        }
    }
}
