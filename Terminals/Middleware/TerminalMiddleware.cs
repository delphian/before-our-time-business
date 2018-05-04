using BeforeOurTime.Business.Apis;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Terminals.Middleware
{
    /// <summary>
    /// Base class for terminal middleware
    /// </summary>
    public class TerminalMiddleware
    {
        /// <summary>
        /// Interface to the core environment
        /// </summary>
        protected IApi Api { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="api">Interface to the core environment</param>
        public TerminalMiddleware(IApi api)
        {
            Api = api;
        }
    }
}
