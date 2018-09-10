using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.ItemAttributes;
using BeforeOurTime.Models.ItemAttributes.Physicals;
using BeforeOurTime.Models.Primitives.Images;
using System;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Models.ItemAttributes.Visibles;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Physicals
{
    public interface IVisibleAttributeManager : IAttributeManager<VisibleAttribute>, IAttributeManager
    {
    }
}
