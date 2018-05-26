using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Requests.Models
{
    public class IOLookRequest : IORequest, IIORequest
    {
        /// <summary>
        /// Unique terminal request identifier
        /// </summary>
        /// <returns></returns>
        public Guid GetId()
        {
            return new Guid("a50d4898-22f6-46c3-b23b-69b866593b13");
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
            return "Look Command";
        }
    }
}
