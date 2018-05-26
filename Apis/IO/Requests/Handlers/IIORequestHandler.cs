﻿using BeforeOurTime.Business.Apis.IO.Requests.Models;
using BeforeOurTime.Business.Terminals;
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
        /// Handle terminal request
        /// </summary>
        /// <param name="api"></param>
        /// <param name="terminal"></param>
        /// <param name="terminalRequest"></param>
        void HandleRequest(IApi api, Terminal terminal, IIORequest terminalRequest);
    }
}
