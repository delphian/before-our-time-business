using BeforeOurTime.Repository.Models.Items.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Updates.Models
{
    /// <summary>
    /// A physical (visible) item has done something
    /// </summary>
    public class IOPhysicalUpdate : IOUpdate, IIOUpdate
    {
        /// <summary>
        /// Physical attributes of item
        /// </summary>
        public AttributePhysical PhysicalAttributes { set; get; }
        /// <summary>
        /// Type of physical activity attributes are invoking
        /// </summary>
        public IOPhysicalUpdateType Type { set; get; }
        /// <summary>
        /// Unique update identifier
        /// </summary>
        /// <returns></returns>
        public Guid GetId()
        {
            return new Guid("2d6bb89d-08d3-4d3f-acff-2e0dc0481a56");
        }
        /// <summary>
        /// Human friendly name describing update type
        /// </summary>
        public string GetName()
        {
            return "Physical (visible) item has arrived at location";
        }
    }
}
