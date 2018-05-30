using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Interfaces
{
    public interface IAttributeCharacterManager : IAttributeManager<AttributeCharacter>, IAttributeManager
    {
        /// <summary>
        /// Create a new character
        /// </summary>
        /// <param name="name">Public name of the character</param>
        /// <param name="accountId">Account to which this character belongs</param>
        /// <param name="initialLocation">Location of new character</param>
        AttributeCharacter Create(string name, Guid accountId, AttributeLocation initialLocation);
    }
}
