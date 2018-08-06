using BeforeOurTime.Business.Apis.Accounts;
using BeforeOurTime.Business.Apis.Items;
using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Business.Apis.Messages;
using BeforeOurTime.Business.Apis.Scripts;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models.Items;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeforeOurTime.Business.Apis
{
    /// <summary>
    /// Interface to the core environment
    /// </summary>
    public interface IApi
    {
        ILogger GetLogger();
        IMessageManager GetMessageManager();
        IAccountManager GetAccountManager();
        IScriptManager GetScriptManager();
        IItemManager GetItemManager();
        ITerminalManager GetTerminalManager();
        /// <summary>
        /// Get item detail manager based on detail manager type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetAttributeManager<T>() where T : IAttributeManager;
        /// <summary>
        /// Get item attribute manager based on class name of attribute type
        /// </summary>
        /// <param name="attributeTypeName"></param>
        /// <returns></returns>
        IAttributeManager GetAttributeManagerOfType(string attributeTypeName);
        /// <summary>
        /// Get all attribute managers for an item
        /// </summary>
        /// <param name="item">Item to determine managers for</param>
        /// <returns></returns>
        List<IAttributeManager> GetAttributeManagers(Item item);
        /// <summary>
        /// (awaitable) Execute all item scripts that desire a regular periodic event
        /// </summary>
        /// <param name="delayMs">Interval between ticks</param>
        /// <param name="ct">Cancelation token for ticks</param>
        Task TickAsync(int delayMs, CancellationToken ct);
        /// <summary>
        /// Deliver messages to their recipient items and execute each item script
        /// </summary>
        /// <param name="delayMs">Interval between ticks</param>
        /// <param name="ct">Cancelation token for ticks</param>
        Task DeliverMessagesAsync(
            int delayMs,
            CancellationToken ct,
            IConfigurationRoot config,
            IServiceProvider serviceProvider);
    }
}
