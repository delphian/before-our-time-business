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
        Unknown = 0,
        Arriving = 1,
        Departing = 2,
        Speaking = 3,
        Emoting = 4,
    }
}
