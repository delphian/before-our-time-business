using BeforeOurTime.Business.Apis.Accounts;
using BeforeOurTime.Business.Apis.Items;
using BeforeOurTime.Business.Apis.Items.Details;
using BeforeOurTime.Business.Apis.Messages;
using BeforeOurTime.Business.Apis.Scripts;
using BeforeOurTime.Repository.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis
{
    /// <summary>
    /// Interface to the core environment
    /// </summary>
    public interface IApi
    {
        IMessageManager GetMessageManager();
        IAccountManager GetAccountManager();
        IScriptManager GetScriptManager();
        IItemManager GetItemManager();
        /// <summary>
        /// Get item detail manager based on detail manager type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetDetailManager<T>() where T : IDetailManager;
        /// <summary>
        /// Get item detail manager based on item type
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        IDetailManager GetDetailManager(ItemType itemType);
    }
}
