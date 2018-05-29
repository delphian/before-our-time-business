using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Updates.Models
{
    public class IOLocationUpdate : IOUpdate, IIOUpdate
    {
        /// <summary>
        /// Unique Location Item Detail Identifier
        /// </summary>
        public Guid DetailLocationId { set; get; }
        /// <summary>
        /// Short name of location
        /// </summary>
        public string Name { set; get; }
        /// <summary>
        /// Longer paragraph description of location
        /// </summary>
        public string Description { set; get; }
        /// <summary>
        /// Background image for location
        /// </summary>
        public string Image { set; get; }
        /// <summary>
        /// Unique terminal update identifier
        /// </summary>
        /// <returns></returns>
        public Guid GetId()
        {
            return new Guid("57161c60-2754-4132-a274-e3cc6e19cb13");
        }
        /// <summary>
        /// Human friendly name describing update type
        /// </summary>
        /// <remarks>
        /// Such as "Look Command Results", or "Ping Result"
        /// </remarks>
        /// <returns></returns>
        public string GetName()
        {
            return "Location Update";
        }
    }
}
