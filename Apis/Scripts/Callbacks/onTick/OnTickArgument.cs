using BeforeOurTime.Repository.Models.Scripts.Callbacks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Scripts.Callbacks.onTick
{
    /// <summary>
    /// A void, or empty, argument
    /// </summary>
    /// <remarks>
    /// This object will be recieved as the argument to the onTick script
    /// function callback.
    /// </remarks>
    public class OnTickArgument : ICallbackArgument
    {
    }
}
