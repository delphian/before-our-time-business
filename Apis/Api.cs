using BeforeOurTime.Business.JsMessageBody;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Messages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Repository.Models;
using System.Linq;

namespace BeforeOurTime.Business.Apis
{
    /// <summary>
    /// Interface into the game
    /// </summary>
    public partial class Api : IApi
    {
        private IItemRepo<Item> ItemRepo { set; get; }
        private IMessageRepo MessageRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemRepo"></param>
        /// <param name="messageRepo"></param>
        public Api(IMessageRepo messageRepo, IItemRepo<Item> itemRepo)
        {
            MessageRepo = messageRepo;
            ItemRepo = itemRepo;
        }
        /// <summary>
        /// Send a message to multiple recipient items
        /// </summary>
        /// <param name="message"></param>
        /// <param name="recipients"></param>
        public void SendMessage(Message message, List<Item> recipients)
        {
            var messages = new List<Message>();
            recipients.ForEach(delegate (Item recipient)
            {
                var messageCopy = (Message)message.Clone();
                messageCopy.To = recipient;
                messages.Add(messageCopy);
            });
            MessageRepo.Create(messages);
        }
    }
}
