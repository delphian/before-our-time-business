﻿using BeforeOurTime.Business.Apis;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Business.Dbs;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Modules
{
    /// <summary>
    /// Register through reflection and manage all modules
    /// </summary>
    public class ModuleManager : IModuleManager
    {
        /// <summary>
        /// System configuration
        /// </summary>
        private IConfiguration Configuration { set; get; }
        /// <summary>
        /// Access to items in the data store
        /// </summary>
        private IItemRepo ItemRepo { set; get; }
        /// <summary>
        /// List of all registered Api Modules
        /// </summary>
        private List<IModule> Modules { set; get; } = new List<IModule>();
        /// <summary>
        /// List of all registered CRUD repositories
        /// </summary>
        private List<ICrudDataRepository> Repositories { set; get; } = new List<ICrudDataRepository>();
        /// <summary>
        /// Record which modules have registered for specific messages
        /// </summary>
        private Dictionary<Guid, List<IModule>> MessageHandlers { set; get; } = new Dictionary<Guid, List<IModule>>();
        /// <summary>
        /// Constructor
        /// </summary>
        public ModuleManager(
            IConfiguration configuration, 
            IItemRepo itemRepo)
        {
            Configuration = configuration;
            ItemRepo = itemRepo;
            RegisterModules();
        }
        /// <summary>
        /// Register all API modules that implement IApiModule
        /// </summary>
        private void RegisterModules()
        {
            var interfaceType = typeof(IModule);
            Modules = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => (IModule)Activator.CreateInstance(x, new object[] { Configuration, ItemRepo }))
                .ToList();
            Modules.ForEach((module) =>
            {
                Repositories.AddRange(module.GetRepositories());
                module.RegisterForMessages().ForEach(messageId =>
                {
                    if (MessageHandlers.ContainsKey(messageId))
                    {
                        MessageHandlers[messageId].Add(module);
                    }
                    else
                    {
                        MessageHandlers.Add(messageId, new List<IModule>() { module });
                    }
                });
            });
            Modules.ForEach((module) =>
            {
                module.Initialize(Repositories);
            });
        }
        /// <summary>
        /// Get API module that implements interface
        /// </summary>
        /// <typeparam name="T">Interface that API module must implement</typeparam>
        /// <returns>API module if found, otherwise null</returns>
        public T GetModule<T>() where T : IModule
        {
            var module = Modules.Where(x => x is T).Select(x => x).FirstOrDefault();
            return (T)module;
        }
        /// <summary>
        /// Get repository that implements interface
        /// </summary>
        /// <typeparam name="T">Interface that repository must implement</typeparam>
        /// <returns></returns>
        public T GetRepository<T>() where T : ICrudDataRepository
        {
            var repository = Repositories.Where(x => x is T).Select(x => x).FirstOrDefault();
            return (T)repository;
        }
        /// <summary>
        /// Get all modules that have registered handle a message
        /// </summary>
        /// <param name="messageId">Unique message identifier</param>
        /// <returns></returns>
        public List<IModule> GetModulesForMessage(Guid messageId)
        {
            var modules = new List<IModule>();
            if (MessageHandlers.ContainsKey(messageId))
            {
                modules = MessageHandlers[messageId];
            }
            return modules;
        }
        /// <summary>
        /// Handle a message
        /// </summary>
        /// <param name="api"></param>
        /// <param name="message"></param>
        /// <param name="terminal"></param>
        /// <param name="response"></param>
        public void HandleMessage(IMessage message, IApi api, Terminal terminal, IResponse response)
        {
            var modules = GetModulesForMessage(message.GetMessageId());
            modules.ForEach(module =>
            {
                module.HandleMessage(message, api, terminal, response);
            });
        }
    }
}
