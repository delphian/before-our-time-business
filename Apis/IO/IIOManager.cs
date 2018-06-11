using BeforeOurTime.Business.Apis.IO.Requests.Handlers;
using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Repository.Models.Messages.Requests;
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
        /// <param name="terminalRequest">A request from a terminal</param>
        void HandleRequest(IApi api, Terminal terminal, IRequest terminalRequest);
    }
}
