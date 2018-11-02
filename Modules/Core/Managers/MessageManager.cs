using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Dbs;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.Core.Models.Data;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Terminal.Models.Data;

namespace BeforeOurTime.Business.Apis.Items
{
    /// <summary>
    /// Pass messages between items and terminals
    /// </summary>
    public class MessageManager : ModelManager<MessageData>, IMessageManager
    {
        /// <summary>
        /// Manage all modules
        /// </summary>
        private IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Repository for manager
        /// </summary>
        private IMessageDataRepo MessageDataRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemRepo"></param>
        /// <param name="messageManager"></param>
        public MessageManager(
            IModuleManager moduleManager,
            IMessageDataRepo messageDataRepo)
        {
            ModuleManager = moduleManager;
            MessageDataRepo = messageDataRepo;
        }
        /// <summary>
        /// Get all repositories declared by manager
        /// </summary>
        /// <returns></returns>
        public List<ICrudModelRepository> GetRepositories()
        {
            return new List<ICrudModelRepository>() { MessageDataRepo };
        }
        /// <summary>
        /// Get repository as interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetRepository<T>() where T : ICrudModelRepository
        {
            return GetRepositories().Where(x => x is T).Select(x => (T)x).FirstOrDefault();
        }
        /// <summary>
        /// Send one or more messages to one or more items
        /// </summary>
        /// <param name="messages">List of messages to send</param>
        /// <param name="items">List of items to send messages to</param>
        /// <param name="origin">Origin of the messages</param>
        public void SendMessage(List<IMessage> messages, List<Item> items, Item origin = null)
        {
            items.ForEach(item =>
            {
                // If a terminal is attached to the item then forward message to terminal
                var terminal = item.GetData<TerminalData>()?.Terminal;
                if (terminal != null)
                {
                    messages.ForEach(message =>
                    {
                        terminal.SendToClient(message);
                    });
                }
                else
                {
                    // Deliver to item javascript?
                }
            });
        }
    }
}