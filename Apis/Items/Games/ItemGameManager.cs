using BeforeOurTime.Business.Apis.Messages;
using BeforeOurTime.Business.Apis.Scripts;
using BeforeOurTime.Repository.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Games
{
    class ItemGameManager : ItemManager, IItemGameManager
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemRepo"></param>
        /// <param name="messageManager"></param>
        /// <param name="scriptManager"></param>
        public ItemGameManager(
            IItemRepo<Item> itemRepo,
            IMessageManager messageManager,
            IScriptManager scriptManager) : base(itemRepo, messageManager, scriptManager)
        {
        }
    }
}
