using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Items.Attributes;
using BeforeOurTime.Models.Items.Attributes.Characters;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Interfaces
{
    public interface ICharacterAttributeManager : IAttributeManager<CharacterAttribute>, IAttributeManager
    {
    }
}
