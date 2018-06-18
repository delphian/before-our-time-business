using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO
{
    /// <summary>
    /// Manage all terminal input request handlers
    /// </summary>
    public interface IIOManager
    {
        /// <summary>
        /// Distribute a terminal request to all registered terminal request handlers
        /// </summary>
        /// <param name="api"></param>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        /// <param name="request">A request from a terminal</param>
        /// <param name="response">A response to the terminal</param>
        IResponse HandleRequest(IApi api, Terminal terminal, IRequest request);
    }
}
