using BeforeOurTime.Business.Apis.IO.Requests.Handlers;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Responses;
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
        private List<IRequestHandler> RequestHandlers = new List<IRequestHandler>();
        /// <summary>
        /// List of handlers to process terminal input requests keyed by request type
        /// </summary>
        private Dictionary<Guid, List<IRequestHandler>> RequestHandlersForTypes = new Dictionary<Guid, List<IRequestHandler>>();
        /// <summary>
        /// Constructor
        /// </summary>
        public IOManager()
        {
            RequestHandlers = BuildRequestHandlers();
            RequestHandlersForTypes = BuildRequestHandlersForTypes(RequestHandlers);
        }
        /// <summary>
        /// Use reflection to register all classes which will handle a terminal request
        /// </summary>
        /// <returns></returns>
        private List<IRequestHandler> BuildRequestHandlers()
        {
            var requestHandlers = new List<IRequestHandler>();
            var interfaceType = typeof(IRequestHandler);
            requestHandlers = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => (IRequestHandler) Activator.CreateInstance(x))
                .ToList();
            return requestHandlers;
        }
        /// <summary>
        /// Allow request handlers to register for specific request types
        /// </summary>
        /// <param name="requestHandlers">List of handlers to process terminal input requests</param>
        /// <returns></returns>
        private Dictionary<Guid, List<IRequestHandler>> BuildRequestHandlersForTypes(
            List<IRequestHandler> requestHandlers)
        {
            var requestHandlersForTypes = new Dictionary<Guid, List<IRequestHandler>>();
            requestHandlers.ForEach(delegate (IRequestHandler requestHandler)
            {
                var requestGuids = requestHandler.RegisterForRequests();
                requestGuids.ForEach(delegate (Guid requestGuid)
                {
                    var requestHandlersForType = requestHandlersForTypes?.GetValueOrDefault(requestGuid);
                    if (requestHandlersForType == null)
                    {
                        requestHandlersForType = new List<IRequestHandler>()
                        {
                            requestHandler
                        };
                    } else
                    {
                        requestHandlersForType.Add(requestHandler);
                    }
                    requestHandlersForTypes[requestGuid] = requestHandlersForType;
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
        public IResponse HandleRequest(IApi api, Terminal terminal, IRequest request)
        {
            IResponse response = new Response() { ResponseSuccess = false };
            var requestGuid = request.GetMessageId();
            var requestHandlersForType = RequestHandlersForTypes
                .Where(x => x.Key == requestGuid)
                .Select(x => x.Value)
                .FirstOrDefault();
            requestHandlersForType.ForEach(delegate (IRequestHandler requestHandler)
            {
                response = requestHandler.HandleRequest(api, terminal, request, response);
            });
            return response;
        }
    }
}
