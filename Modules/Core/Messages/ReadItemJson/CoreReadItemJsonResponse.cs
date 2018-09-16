using BeforeOurTime.Models.Json;
using BeforeOurTime.Models.Messages.Events.Created;
using BeforeOurTime.Models.Messages.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Modules.Core.Messages
{
    public class CoreReadItemJsonResponse : Response, IResponse
    {
        /// <summary>
        /// Unique message identifier
        /// </summary>
        [JsonIgnore]
        [JsonConverter(typeof(GuidJsonConverter))]
        public static Guid _Id = new Guid("4dc3881e-d0a3-4f44-a7dc-86ce3696c310");
        /// <summary>
        /// Event message for item read
        /// </summary>
        [JsonProperty(PropertyName = "coreReadItemJsonEvent", Order = 1000)]
        public CoreReadItemJsonEvent CoreReadItemJsonEvent { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public CoreReadItemJsonResponse()
        {
            this.MessageId = _Id;
            this.MessageName = "Read Item JSON Response";
        }
    }
}
