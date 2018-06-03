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
        /// List of handlers to process terminal input requests keyed by request type
        /// </summary>
        private Dictionary<string, List<IIORequestHandler>> IORequestHandlersForTypes = new Dictionary<string, List<IIORequestHandler>>();
        /// <summary>
        /// Constructor
        /// </summary>
        public IOManager()
        {
            IORequestHandlers = BuildRequestHandlers();
            IORequestHandlersForTypes = BuildRequestHandlersForTypes(IORequestHandlers);
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
        /// Allow request handlers to register for specific request types
        /// </summary>
        /// <param name="requestHandlers">List of handlers to process terminal input requests</param>
        /// <returns></returns>
        private Dictionary<string, List<IIORequestHandler>> BuildRequestHandlersForTypes(
            List<IIORequestHandler> requestHandlers)
        {
            var requestHandlersForTypes = new Dictionary<string, List<IIORequestHandler>>();
            requestHandlers.ForEach(delegate (IIORequestHandler requestHandler)
            {
                var requestTypes = requestHandler.RegisterForIORequests();
                requestTypes.ForEach(delegate (string requestType)
                {
                    var requestHandlersForType = requestHandlersForTypes.GetValueOrDefault(requestType);
                    if (requestHandlersForType == null)
                    {
                        requestHandlersForType = new List<IIORequestHandler>()
                        {
                            requestHandler
                        };
                    } else
                    {
                        requestHandlersForType.Add(requestHandler);
                    }
                    requestHandlersForTypes[requestType] = requestHandlersForType;
                });
            });
            return requestHandlersForTypes;
        }
        /// <summary>
        /// Forward terminal IO request to IO request handlers registered for it's type
        /// </summary>
        /// <param name="api"></param>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        /// <param name="terminalRequest">A request from a terminal</param>
        public void HandleRequest(IApi api, Terminal terminal, IIORequest terminalRequest)
        {
            var requestType = terminalRequest.GetType().ToString();
            var requestHandlersForType = IORequestHandlersForTypes
                .Where(x => x.Key == requestType)
                .Select(x => x.Value)
                .FirstOrDefault();
            requestHandlersForType.ForEach(delegate (IIORequestHandler requestHandler)
            {
                requestHandler.HandleIORequest(api, terminal, terminalRequest);
            });
        }
    }
}
