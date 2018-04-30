using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis
{
    public interface IApi
    {
        /// <summary>
        /// Send a message to multiple recipient items
        /// </summary>
        /// <param name="message"></param>
        /// <param name="recipients"></param>
        void SendMessage(Message message, List<Item> recipients);
        /// <summary>
        /// Relocate an item
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="newParent"></param>
        /// <param name="child"></param>
        void MoveItem(Item Source, Item newParent, Item child);
    }
}
