using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Updates.Models
{
    public class IOLookUpdate : IOUpdate, IIOUpdate
    {
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
        /// Such as "Look Command Results", or "Ping Results"
        /// </remarks>
        /// <returns></returns>
        public string GetName()
        {
            return "Look Command Update";
        }
    }
}
