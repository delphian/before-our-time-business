using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Requests.Models
{
    /// <summary>
    /// A request from a terminal
    /// </summary>
    /// <remarks>
    /// All communication from terminals must be in the form of IIORequest
    /// </remarks>
    public interface IIORequest
    {
        /// <summary>
        /// Unique terminal request identifier
        /// </summary>
        /// <returns></returns>
        Guid GetId();
        /// <summary>
        /// Human friendly name describing request type
        /// </summary>
        /// <remarks>
        /// Such as "Look Command", or "Ping"
        /// </remarks>
        /// <returns></returns>
        string GetName();
    }
}
