using BeforeOurTime.Repository.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Characters
{
    public interface IItemCharacterManager : IItemManager
    {
        /// <summary>
        /// Create a new character
        /// </summary>
        /// <param name="name">Public name of the character</param>
        /// <param name="accountId">Account to which this character belongs</param>
        /// <param name="parentId">Location of new item</param>
        ItemCharacter Create(string name, Guid accountId, Guid parentId);
    }
}
