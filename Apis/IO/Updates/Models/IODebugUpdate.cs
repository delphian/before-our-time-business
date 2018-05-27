using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Updates.Models
{
    /// <summary>
    /// Description of error, runtime condition, or other detailed execution data
    /// </summary>
    public class IODebugUpdate : IOUpdate, IIOUpdate
    {
        /// <summary>
        /// Raw textual description
        /// </summary>
        public string Description { set; get; }
        /// <summary>
        /// Unique terminal update identifier
        /// </summary>
        /// <returns></returns>
        public Guid GetId()
        {
            return new Guid("61013c1e-84f3-4c2e-87fa-2f00e20b4411");
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
            return "Debug Update";
        }
    }
}
