using BeforeOurTime.Business.Apis;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Terminals.Middleware.Telnet
{
    class TelnetTerminalMiddleware : TerminalMiddleware, ITerminalMiddleware
    {
        public TelnetTerminalMiddleware(IApi api) : base(api) { }
        /// <summary>
        /// Opportunity to alter a raw message heading toward the API from a client
        /// </summary>
        /// <param name="terminalRequest"></param>
        /// <param name="next">Next middleware</param>
        /// <returns></returns>
        public IRequest ToApi(IRequest terminalRequest, Func<IRequest, IRequest> next) {
            return (next != null) ? next(terminalRequest) : terminalRequest;
        }
        /// <summary>
        /// Opportunity to alter a raw message heading toward the client from the API
        /// </summary>
        /// <param name="environmentUpdate"></param>
        /// <param name="next">Next middleware</param>
        /// <returns></returns>
        public IMessage ToClient(IMessage environmentUpdate, Func<IMessage, IMessage> next)
        {
            return (next != null) ? next(environmentUpdate) : environmentUpdate;
        }
    }
}
