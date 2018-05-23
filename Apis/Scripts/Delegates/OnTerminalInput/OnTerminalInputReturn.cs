using BeforeOurTime.Repository.Models.Scripts.Delegates;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput
{
    /// <summary>
    /// A void, or empty, return value
    /// </summary>
    /// <remarks>
    /// This object will be return from the onTerminalInput script delegate
    /// </remarks>
    public class OnTerminalInputReturn : IDelegateReturn
    {
    }
}
