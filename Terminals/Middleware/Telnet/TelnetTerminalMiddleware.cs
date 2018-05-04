using BeforeOurTime.Business.Apis;
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
        /// <param name="message">Raw message headed toward the API</param>
        /// <param name="next">Next middleware</param>
        /// <returns></returns>
        public string ToApi(string message, Func<string, string> next) {
            return (next != null) ? next(message) : message;
        }
        /// <summary>
        /// Opportunity to alter a raw message heading toward the client from the API
        /// </summary>
        /// <param name="message">Raw message headed toward the client</param>
        /// <param name="next">Next middleware</param>
        /// <returns></returns>
        public string ToClient(string message, Func<string, string> next)
        {
            return (next != null) ? next(message) : message;
        }
    }
}
