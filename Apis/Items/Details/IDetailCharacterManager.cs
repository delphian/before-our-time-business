using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Details;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Details
{
    public interface IDetailCharacterManager : IDetailManager<DetailCharacter>
    {
        /// <summary>
        /// Create a new character
        /// </summary>
        /// <param name="name">Public name of the character</param>
        /// <param name="accountId">Account to which this character belongs</param>
        /// <param name="initialLocation">Location of new character</param>
        DetailCharacter Create(string name, Guid accountId, DetailLocation initialLocation);
    }
}
