﻿using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BeforeOurTime.Business.Apis.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using BeforeOurTime.Models.Exceptions;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Logs;
using BeforeOurTime.Models.Modules.Core.Managers;

namespace BeforeOurTime.Business.Apis
{
    /// <summary>
    /// Interface to the core environment
    /// </summary>
    public partial class Api : IApi
    {
        private IBotLogger Logger { set; get; }
        private IConfiguration Configuration { set; get; }
        private IMessageManager MessageManager { set; get; }
        private IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemRepo"></param>
        /// <param name="messageRepo"></param>
        public Api(
            IBotLogger logger,
            IConfiguration configuration,
            IMessageManager messageManager,
            IModuleManager moduleManager)
        {
            Logger = logger;
            Configuration = configuration;
            MessageManager = messageManager;
            ModuleManager = moduleManager;
        }
        public IMessageManager GetMessageManager()
        {
            return MessageManager;
        }
        public IItemManager GetItemManager()
        {
            return ModuleManager.GetManager<IItemManager>();
        }
        public IBotLogger GetLogger()
        { 
            return Logger;
        }
        public IConfiguration GetConfiguration()
        {
            return Configuration;
        }
        public IModuleManager GetModuleManager()
        {
            return ModuleManager;
        }
        /// <summary>
        /// (awaitable) Execute all item scripts that desire a regular periodic event
        /// </summary>
        /// <param name="delayMs">Interval between ticks</param>
        /// <param name="ct">Cancelation token for ticks</param>
        //public async Task TickAsync(int delayMs, CancellationToken ct)
        //{
        //    var game = GetAttributeManager<IGameAttributeManager>().GetDefaultGame();
        //    var onTickDelegate = ScriptManager.GetDelegateDefinition("onTick");
        //    while (!ct.IsCancellationRequested)
        //    {
        //        await Task.Delay(delayMs);
        //        lock (lockObject)
        //        {
        //            var tickEvent = new TickEvent() { };
        //            //var itemRecipientIds = ItemManager.GetDelegateImplementerIds(onTickDelegate);
        //            //var itemRecipients = ItemManager.Read(itemRecipientIds);
        //            //MessageManager.SendMessage(tickEvent, itemRecipients, game.Id);
        //        }
        //    }
        //}
        /// <summary>
        /// Deliver messages to their recipient items and execute each item script
        /// </summary>
        /// <param name="delayMs">Interval between ticks</param>
        /// <param name="ct">Cancelation token for ticks</param>
        //public async Task DeliverMessagesAsync(int delayMs, CancellationToken ct, IConfiguration config, IServiceProvider serviceProvider)
        //{
        //    while (!ct.IsCancellationRequested)
        //    {
        //        await Task.Delay(delayMs);
        //        lock (lockObject)
        //        {
        //            // Create script global functions
        //            var jsFunctionManager = new JsFunctionManager(config, serviceProvider);
        //            // Get messages
        //            List<SavedMessage> messages = MessageManager.CullAllMessages();
        //            // Deliver message to each recipient
        //            foreach (SavedMessage message in messages)
        //            {
        //                try
        //                {
        //                    var item = ItemManager.Read(message.RecipientId);
        //                    List<IAttributeManager> attributeManagers = GetAttributeManagers(item);
        //                    // Hand off message deliver to each item's manager code
        //                    attributeManagers.ForEach(delegate (IAttributeManager attributeManager)
        //                    {
        //                        // TODO : This should probably just add items to jsFunctionManager
        //                        // and then execute the script once instead of each manager executing the script
        //                        attributeManager.DeliverMessage(message, item, jsFunctionManager);
        //                    });
        //                }
        //                catch (Exception ex)
        //                {
        //                    Logger.LogError("script failed: " + ex.Message);
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
