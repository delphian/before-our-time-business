using BeforeOurTime.Repository.Models.Scripts.Callbacks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Scripts.Delegates.onTick
{
    /// <summary>
    /// A void, or empty, return value
    /// </summary>
    /// <remarks>
    /// This object will be return from the onTick script delegate
    /// </remarks>
    public class OnTickReturn : ICallbackReturn
    {
    }
}
