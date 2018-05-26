using BeforeOurTime.Business.Apis.IO.Requests.Handlers;
using BeforeOurTime.Business.Apis.IO.Requests.Models;
using BeforeOurTime.Business.Terminals;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO
{
    /// <summary>
    /// Manage all terminal input request handlers
    /// </summary>
    public class IOManager : IIOManager
    {
        /// <summary>
        /// List of handlers to process terminal input requests
        /// </summary>
        private List<IIORequestHandler> IORequestHandlers = new List<IIORequestHandler>();
        /// <summary>
        /// Constructor
        /// </summary>
        public IOManager()
        {
            IORequestHandlers = BuildRequestHandlers();
        }
        /// <summary>
        /// Use reflection to register all classes which will handle a terminal request
        /// </summary>
        /// <returns></returns>
        private List<IIORequestHandler> BuildRequestHandlers()
        {
            var requestHandlers = new List<IIORequestHandler>();
            var interfaceType = typeof(IIORequestHandler);
            requestHandlers = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => (IIORequestHandler) Activator.CreateInstance(x))
                .ToList();
            return requestHandlers;
        }
        /// <summary>
        /// Distribute a terminal request to all registered terminal request handlers
        /// </summary>
        /// <param name="api"></param>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        /// <param name="terminalRequest">A request from a terminal</param>
        public void HandleRequest(IApi api, Terminal terminal, IIORequest terminalRequest)
        {
            IORequestHandlers.ForEach(delegate (IIORequestHandler requestHandler)
            {
                requestHandler.HandleRequest(api, terminal, terminalRequest);
            });
        }
    }
}
