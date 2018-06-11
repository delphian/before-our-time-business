using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Repository.Models.Messages.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Requests.Handlers
{
    /// <summary>
    /// Handle one or more terminal requests
    /// </summary>
    public interface IIORequestHandler
    {
        /// <summary>
        /// Register to handle a specific set of IO requests
        /// </summary>
        /// <returns></returns>
        List<string> RegisterForIORequests();
        /// <summary>
        /// Handle terminal request
        /// </summary>
        /// <param name="api"></param>
        /// <param name="terminal"></param>
        /// <param name="terminalRequest"></param>
        void HandleIORequest(IApi api, Terminal terminal, IRequest terminalRequest);
    }
}
