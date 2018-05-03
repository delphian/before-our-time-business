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
        void ItemMove(Item Source, Item newParent, Item child);
        /// <summary>
        /// Truncate all tables in database
        /// </summary>
        IApi DataReset();
        /// <summary>
        /// Install initial accounts and database objects
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory</param>
        IApi DataInstall(string path);
        /// <summary>
        /// Permenantly delete an item and remove from data store
        /// </summary>
        /// <remarks>
        /// All children will be re-homed to the item parent unless otherwise specified
        /// </remarks>
        /// <param name="item">Item to delete</param>
        /// <param name="deleteChildren">Also delete all children</param>
        void ItemDelete(Item item, bool? deleteChildren = false);
    }
}
