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
using BeforeOurTime.Business.Apis.Items.Details;
using BeforeOurTime.Repository.Models.Items.Details;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Business.Apis.IO;

namespace BeforeOurTime.Business.Apis
{
    /// <summary>
    /// Interface to the core environment
    /// </summary>
    public partial class Api : IApi
    {
        private Dictionary<Type, IDetailManager> DetailManagerList = new Dictionary<Type, IDetailManager>();
        private IMessageManager MessageManager { set; get; }
        private IAccountManager AccountManager { set; get; }
        private IScriptManager ScriptManager { set; get; }
        private IIOManager IOManager { set; get; }
        private IItemManager ItemManager { set; get; }
        private IDetailGameManager DetailGameManager { set; get; }
        private IDetailCharacterManager DetailCharacterManager { set; get; }
        private IDetailLocationManager DetailLocationManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemRepo"></param>
        /// <param name="messageRepo"></param>
        public Api(
            IMessageManager messageManager,
            IAccountManager accountManager,
            IScriptManager scriptManager,
            IIOManager ioManager,
            IItemManager itemManager,
            IDetailGameManager detailGameManager,
            IDetailCharacterManager detailCharacterManager,
            IDetailLocationManager detailLocationManager)
        {
            MessageManager = messageManager;
            AccountManager = accountManager;
            ScriptManager = scriptManager;
            IOManager = ioManager;
            ItemManager = itemManager;
            DetailManagerList.Add(typeof(IDetailGameManager), detailGameManager);
            DetailManagerList.Add(typeof(IDetailCharacterManager), detailCharacterManager);
            DetailManagerList.Add(typeof(IDetailLocationManager), detailLocationManager);
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
        /// <summary>
        /// Get item detail manager based on detail manager type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetDetailManager<T>() where T : IDetailManager
        {
            return (T)DetailManagerList.Where(x => x.Key == typeof(T)).Select(x => x.Value).First();
        }
        /// <summary>
        /// Get all attribute managers for an item
        /// </summary>
        /// <param name="item">Item to determine managers for</param>
        /// <returns></returns>
        public List<IDetailManager> GetAttributeManagers(Item item)
        {
            return DetailManagerList
                .Where(x => x.Value.IsManaging(item))
                .Select(x => x.Value)
                .ToList();
        }
    }
}
