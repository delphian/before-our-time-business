using BeforeOurTime.Business.Apis.Scripts;
using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Attributes;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.Messages
{
    /// <summary>
    /// Central environment interface for all things message related
    /// </summary>
    public class MessageManager : IMessageManager
    {
        private IMessageRepo MessageRepo { set; get; }
        private IScriptManager ScriptManager { set; get; }
        private ITerminalManager TerminalManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageRepo"></param>
        public MessageManager(
            IMessageRepo messageRepo,
            IScriptManager scriptManager,
            ITerminalManager terminalManager)
        {
            MessageRepo = messageRepo;
            ScriptManager = scriptManager;
            TerminalManager = terminalManager;
        }
        /// <summary>
        /// Send a message to multiple recipient items
        /// </summary>
        /// <param name="message"></param>
        /// <param name="recipients"></param>
        /// <param name="senderId"></param>
        public void SendMessage(IMessage message, List<Item> recipients, Guid senderId)
        {
            var savedMessage = new SavedMessage()
            {
                SenderId = senderId,
                DelegateId = ScriptManager.GetDelegateDefinition("onArrival").GetId(),
                Package = JsonConvert.SerializeObject(message)
            };
            recipients.ForEach(delegate (Item recipient)
            {
                var playerAttribute = recipient.GetAttribute<AttributePlayer>();
                if (playerAttribute != null)
                {
                    var terminalId = TerminalManager.GetTerminals()
                        .Where(x => x.PlayerId == playerAttribute.Id)
                        .Select(x => x.Id)
                        .FirstOrDefault();
                    TerminalManager.SendToTerminalId(terminalId, message);
                } else
                {
                    var messageCopy = (SavedMessage)savedMessage.Clone();
                    messageCopy.RecipientId = recipient.Id;
                    MessageRepo.Create(messageCopy);
                }
            });
        }
    }
}
