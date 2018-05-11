using BeforeOurTime.Business.Apis.Accounts;
using BeforeOurTime.Repository.Models.Accounts;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Messages;
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
        /// <summary>
        /// Create a new item
        /// </summary>
        /// <remarks>
        /// Any item, or model derived from an item, may be created
        /// </remarks>
        /// <param name="Source">Item responsible for doing the creating</param>
        /// <param name="newParent">Item which will become the parent</param>
        /// <param name="child">Item which is the new child being created</param>
        bool ItemCreate<T>(Item source, Item parent, T child) where T : Item;
        /// <summary>
        /// Create a new account and local authentication credentials
        /// </summary>
        /// <param name="name">Friendly account name</param>
        /// <param name="email">Login email address for account</param>
        /// <param name="password">Password for account</param>
        Account AccountCreate(string name, string email, string password);
        /// <summary>
        /// Create a new character
        /// </summary>
        /// <param name="name">Public name of the character</param>
        /// <param name="accountId">Account to which this character belongs</param>
        /// <param name="parentId">Location of new item</param>
        Character ItemCreateCharacter(
            string name,
            Guid accountId,
            Guid parentId);
        IAccountManager GetAccountManager();
    }
}
