using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Updates.Models
{
    /// <summary>
    /// Type of physical activity attributes are invoking
    /// </summary>
    public enum IOPhysicalUpdateType : int
    {
        /// <summary>
        /// Unknown or not available. Generally this is a bad thing
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Item with physical attributes has arrived at location
        /// </summary>
        Arriving = 1,
        /// <summary>
        /// Item with physical attributes has departed location
        /// </summary>
        Departing = 2,
        /// <summary>
        /// Item with physical attributes is speaking
        /// </summary>
        Speaking = 3,
        /// <summary>
        /// Item with physical attributes is emoting
        /// </summary>
        Emoting = 4,
    }
}
