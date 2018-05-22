using BeforeOurTime.Business.Apis.Accounts;
using BeforeOurTime.Business.Apis.Items;
using BeforeOurTime.Business.Apis.Items.Details;
using BeforeOurTime.Business.Apis.Messages;
using BeforeOurTime.Business.Apis.Scripts;
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
        T GetDetailManager<T>() where T : IDetailManager;
    }
}
