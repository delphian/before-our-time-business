using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Requests.Models
{
    public class IOGoRequest : IORequest
    {
        /// <summary>
        /// Unique exit attribute identifier
        /// </summary>
        public Guid ExitId { set; get; }
    }
}
