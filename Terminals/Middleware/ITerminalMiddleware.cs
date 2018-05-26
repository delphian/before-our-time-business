using BeforeOurTime.Business.Apis.IO.Requests.Models;
using BeforeOurTime.Business.Apis.IO.Updates.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Terminals.Middleware
{
    /// <summary>
    /// Features that may insert themselves between terminal and api or terminal and server
    /// </summary>
    public interface ITerminalMiddleware
    {
        /// <summary>
        /// Opportunity to alter a raw message heading toward the API from a client
        /// </summary>
        /// <param name="terminalRequest">A request from a terminal</param>
        /// <param name="next">Next middleware</param>
        /// <returns></returns>
        IIORequest ToApi(IIORequest terminalRequest, Func<IIORequest, IIORequest> next);
        /// <summary>
        /// Opportunity to alter a raw message heading toward the client from the API
        /// </summary>
        /// <param name="environmentUpdate">A response or update to a terminal</param>
        /// <param name="next">Next middleware</param>
        /// <returns></returns>
        IIOUpdate ToClient(IIOUpdate environmentUpdate, Func<IIOUpdate, IIOUpdate> next);
    }
}
