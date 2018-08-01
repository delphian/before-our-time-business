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
using System.Threading.Tasks;
using System.Threading;
using BeforeOurTime.Models.Messages.Events.Ticks;
using BeforeOurTime.Repository.Models.Messages.Data;
using BeforeOurTime.Business.Apis.Scripts.Libraries;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Business.Apis.Items.Attributes.Locations;

namespace BeforeOurTime.Business.Apis
{
    /// <summary>
    /// Interface to the core environment
    /// </summary>
    public partial class Api : IApi
    {
        private object lockObject;
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
            IPlayerAttributeManager attributePlayerManager,
            ICharacterAttributeManager characterAttributeManager,
            ILocationAttributeManager attributeLocationManager,
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
            AttributeManagerList.Add(typeof(IPlayerAttributeManager), attributePlayerManager);
            AttributeManagerList.Add(typeof(ICharacterAttributeManager), characterAttributeManager);
            AttributeManagerList.Add(typeof(ILocationAttributeManager), attributeLocationManager);
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
            return (T)AttributeManagerList.Where(x => x.Key == typeof(T)).Select(x => x.Value).FirstOrDefault();
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
        /// <summary>
        /// (awaitable) Execute all item scripts that desire a regular periodic event
        /// </summary>
        /// <param name="delayMs">Interval between ticks</param>
        /// <param name="ct">Cancelation token for ticks</param>
        public async Task TickAsync(int delayMs, CancellationToken ct)
        {
            var game = GetAttributeManager<IAttributeGameManager>().GetDefaultGame();
            var onTickDelegate = ScriptManager.GetDelegateDefinition("onTick");
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(delayMs);
                lock (lockObject)
                {
                    var tickEvent = new TickEvent() { };
                    var itemRecipientIds = ItemManager.GetDelegateImplementerIds(onTickDelegate);
                    var itemRecipients = ItemManager.Read(itemRecipientIds);
                    MessageManager.SendMessage(tickEvent, itemRecipients, game.Id);
                }
            }
        }
        /// <summary>
        /// Deliver messages to their recipient items and execute each item script
        /// </summary>
        /// <param name="delayMs">Interval between ticks</param>
        /// <param name="ct">Cancelation token for ticks</param>
        public async Task DeliverMessagesAsync(int delayMs, CancellationToken ct, IConfigurationRoot config, IServiceProvider serviceProvider)
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(delayMs);
                lock (lockObject)
                {
                    // Create script global functions
                    var jsFunctionManager = new JsFunctionManager(config, serviceProvider);
                    // Get messages
                    List<SavedMessage> messages = MessageManager.CullAllMessages();
                    // Deliver message to each recipient
                    foreach (SavedMessage message in messages)
                    {
                        try
                        {
                            var item = ItemManager.Read(message.RecipientId);
                            List<IAttributeManager> attributeManagers = GetAttributeManagers(item);
                            // Hand off message deliver to each item's manager code
                            attributeManagers.ForEach(delegate (IAttributeManager attributeManager)
                            {
                                // TODO : This should probably just add items to jsFunctionManager
                                // and then execute the script once instead of each manager executing the script
                                attributeManager.DeliverMessage(message, item, jsFunctionManager);
                            });
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("script failed: " + ex.Message);
                        }
                    }
                }
            }
        }
    }
}
