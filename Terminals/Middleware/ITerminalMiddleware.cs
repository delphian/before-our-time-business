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
        /// <param name="message">Raw message headed toward the API</param>
        /// <param name="next">Next middleware</param>
        /// <returns></returns>
        string ToApi(string message, Func<string, string> next);
        /// <summary>
        /// Opportunity to alter a raw message heading toward the client from the API
        /// </summary>
        /// <param name="message">Raw message headed toward the client</param>
        /// <param name="next">Next middleware</param>
        /// <returns></returns>
        string ToClient(string message, Func<string, string> next);
    }
}
