using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Requests.Models
{
    public class IOGoRequest : IORequest, IIORequest
    {
        /// <summary>
        /// Unique exit attribute identifier
        /// </summary>
        public Guid ExitId { set; get; }
        /// <summary>
        /// Unique terminal request identifier
        /// </summary>
        /// <returns></returns>
        public Guid GetId()
        {
            return new Guid("cee49fc3-a342-4720-8887-db7a71f14fa8");
        }
        /// <summary>
        /// Human friendly name describing request type
        /// </summary>
        /// <remarks>
        /// Such as "Look Command", or "Ping"
        /// </remarks>
        /// <returns></returns>
        public string GetName()
        {
            return "Go Command";
        }
    }
}
