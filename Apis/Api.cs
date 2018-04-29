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

namespace BeforeOurTime.Business.Apis
{
    /// <summary>
    /// Interface into the game
    /// </summary>
    public partial class Api : IApi
    {
        private IConfigurationRoot Configuration { set; get; }
        private IServiceProvider ServiceProvider { set; get; }
        private IItemRepo<Item> ItemRepo { set; get; }
        private IMessageRepo MessageRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="serviceProvider"></param>
        public Api(IConfigurationRoot configuration, IServiceProvider serviceProvider)
        {
            Configuration = configuration;
            ServiceProvider = serviceProvider;
            ItemRepo = ServiceProvider.GetService<IItemRepo<Item>>();
            MessageRepo = ServiceProvider.GetService<IMessageRepo>();
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
                message.To = recipient;
                messages.Add((Message)message.Clone());
            });
            MessageRepo.Create(messages);
        }
    }
}
