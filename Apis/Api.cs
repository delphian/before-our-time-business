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
using BeforeOurTime.Repository.Models.Accounts;
using BeforeOurTime.Repository.Models.Accounts.Authorization;
using BeforeOurTime.Repository.Models.Accounts.Authentication.Providers;
using BeforeOurTime.Business.JsEvents;

namespace BeforeOurTime.Business.Apis
{
    /// <summary>
    /// Interface to the core environment
    /// </summary>
    public partial class Api : IApi
    {
        private IAccountRepo AccountRepo { set; get; }
        private IAuthorGroupRepo AuthorGroupRepo { set; get; }
        private IMessageRepo MessageRepo { set; get; }
        private IRepository<AuthorizationRole> AuthorRoleRepo { set; get; }
        private IRepository<AuthorizationGroupRole> AuthorGroupRoleRepo { set; get; }
        private IRepository<AuthorizationAccountGroup> AuthorAccountGroupRepo { set; get; }
        private IRepository<AuthenticationBotMeta> AuthenBotMetaRepo { set; get; }
        private IItemRepo<Item> ItemRepo { set; get; }
        protected IJsEventManager JsEventManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemRepo"></param>
        /// <param name="messageRepo"></param>
        public Api(
            IAccountRepo accountRepo,
            IRepository<AuthorizationRole> authorRoleRepo,
            IAuthorGroupRepo authorGroupRepo,
            IRepository<AuthorizationGroupRole> authorGroupRoleRepo,
            IRepository<AuthorizationAccountGroup> authorAccountGroupRepo,
            IRepository<AuthenticationBotMeta> authenBotMetaRepo,
            IMessageRepo messageRepo, 
            IItemRepo<Item> itemRepo,
            IJsEventManager jsEventManager)
        {
            AccountRepo = accountRepo;
            AuthorRoleRepo = authorRoleRepo;
            AuthorGroupRepo = authorGroupRepo;
            AuthorGroupRoleRepo = authorGroupRoleRepo;
            AuthorAccountGroupRepo = authorAccountGroupRepo;
            AuthenBotMetaRepo = authenBotMetaRepo;
            ItemRepo = itemRepo;
            MessageRepo = messageRepo;
            JsEventManager = jsEventManager;
        }
        /// <summary>
        /// Send a message to multiple recipient items
        /// </summary>
        /// <param name="message"></param>
        /// <param name="recipients"></param>
        public void SendMessage(Message message, List<Item> recipients)
        {
            if (message.From != null)
            {
                var messages = new List<Message>();
                recipients.ForEach(delegate (Item recipient)
                {
                    var messageCopy = (Message)message.Clone();
                    messageCopy.ToId = recipient.Id;
                    messages.Add(messageCopy);
                });
                MessageRepo.Create(messages);
            } else
            {
                Console.WriteLine("Refusing to send anonymous item message " + message.Type.ToString());
            }
        }
    }
}
