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
using BeforeOurTime.Business.Apis.Items.Attributes.Interfaces;
using BeforeOurTime.Business.Apis.IO;
using BeforeOurTime.Business.Apis.Terminals;
using Microsoft.Extensions.Logging;
using BeforeOurTime.Models.Items;

namespace BeforeOurTime.Business.Apis
{
    /// <summary>
    /// Interface to the core environment
    /// </summary>
    public partial class Api : IApi
    {
        private Dictionary<Type, IAttributeManager> AttributeManagerList = new Dictionary<Type, IAttributeManager>();
        private ILogger Logger { set; get; }
        private IMessageManager MessageManager { set; get; }
        private IAccountManager AccountManager { set; get; }
        private IScriptManager ScriptManager { set; get; }
        private IIOManager IOManager { set; get; }
        private IItemManager ItemManager { set; get; }
        private ITerminalManager TerminalManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemRepo"></param>
        /// <param name="messageRepo"></param>
        public Api(
            ILogger logger,
            IMessageManager messageManager,
            IAccountManager accountManager,
            IScriptManager scriptManager,
            IIOManager ioManager,
            IItemManager itemManager,
            ITerminalManager terminalManager,
            IAttributeGameManager attributeGameManager,
            IAttributePlayerManager attributePlayerManager,
            IAttributeLocationManager attributeLocationManager,
            IAttributePhysicalManager attributePhysicalManager,
            IAttributeExitManager attributeExitManager)
        {
            Logger = logger;
            MessageManager = messageManager;
            AccountManager = accountManager;
            ScriptManager = scriptManager;
            IOManager = ioManager;
            ItemManager = itemManager;
            TerminalManager = terminalManager;
            AttributeManagerList.Add(typeof(IAttributeGameManager), attributeGameManager);
            AttributeManagerList.Add(typeof(IAttributePlayerManager), attributePlayerManager);
            AttributeManagerList.Add(typeof(IAttributeLocationManager), attributeLocationManager);
            AttributeManagerList.Add(typeof(IAttributePhysicalManager), attributePhysicalManager);
            AttributeManagerList.Add(typeof(IAttributeExitManager), attributeExitManager);
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
        public IIOManager GetIOManager()
        {
            return IOManager;
        }
        public IItemManager GetItemManager()
        {
            return ItemManager;
        }
        public ITerminalManager GetTerminalManager()
        {
            return TerminalManager;
        }
        public ILogger GetLogger()
        { 
            return Logger;
        }
        /// <summary>
        /// Get item detail manager based on detail manager type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetAttributeManager<T>() where T : IAttributeManager
        {
            return (T)AttributeManagerList.Where(x => x.Key == typeof(T)).Select(x => x.Value).First();
        }
        /// <summary>
        /// Get all attribute managers for an item
        /// </summary>
        /// <param name="item">Item to determine managers for</param>
        /// <returns></returns>
        public List<IAttributeManager> GetAttributeManagers(Item item)
        {
            return AttributeManagerList
                .Where(x => x.Value.IsManaging(item))
                .Select(x => x.Value)
                .ToList();
        }
    }
}
