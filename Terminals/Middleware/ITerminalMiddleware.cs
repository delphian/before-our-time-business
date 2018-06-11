using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Requests;
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
        IRequest ToApi(IRequest terminalRequest, Func<IRequest, IRequest> next);
        /// <summary>
        /// Opportunity to alter a raw message heading toward the client from the API
        /// </summary>
        /// <param name="environmentUpdate">A response or update to a terminal</param>
        /// <param name="next">Next middleware</param>
        /// <returns></returns>
        IMessage ToClient(IMessage environmentUpdate, Func<IMessage, IMessage> next);
    }
}
