using BeforeOurTime.Business.Apis.Accounts;
using BeforeOurTime.Business.Apis.IO;
using BeforeOurTime.Business.Apis.Items;
using BeforeOurTime.Business.Apis.Items.Attributes.Interfaces;
using BeforeOurTime.Business.Apis.Messages;
using BeforeOurTime.Business.Apis.Scripts;
using BeforeOurTime.Business.Apis.Terminals;
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
        IIOManager GetIOManager();
        IItemManager GetItemManager();
        ITerminalManager GetTerminalManager();
        /// <summary>
        /// Get item detail manager based on detail manager type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetAttributeManager<T>() where T : IAttributeManager;
        /// <summary>
        /// Get all attribute managers for an item
        /// </summary>
        /// <param name="item">Item to determine managers for</param>
        /// <returns></returns>
        List<IAttributeManager> GetAttributeManagers(Item item);
    }
}
