using BeforeOurTime.Business.Dbs;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Items;
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
    }
}
