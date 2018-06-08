using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Updates.Models
{
    /// <summary>
    /// A response or update to a terminal
    /// </summary>
    /// <remarks>
    /// All communication to terminals must be in the form of IIOUpdate
    /// </remarks>
    public interface IIOUpdate
    {
        /// <summary>
        /// Unique update identifier
        /// </summary>
        /// <returns></returns>
        Guid GetId();
        /// <summary>
        /// Human friendly name describing update type
        /// </summary>
        /// <remarks>
        /// Such as "Look Command Results", or "Ping Result"
        /// </remarks>
        /// <returns></returns>
        string GetName();
    }
}
