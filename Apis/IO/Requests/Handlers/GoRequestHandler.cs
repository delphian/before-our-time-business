using BeforeOurTime.Business.Apis.Items.Attributes.Interfaces;
using BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput;
using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Attributes.Exits;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Requests;
using BeforeOurTime.Repository.Models.Messages.Requests.Look;
using BeforeOurTime.Repository.Models.Messages.Responses;
using BeforeOurTime.Repository.Models.Messages.Responses.Enumerate;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Requests.Handlers
{
    public class GoRequestHandler : IRequestHandler
    {
        public GoRequestHandler()
        {
        }
        /// <summary>
        /// Register to handle a specific set of IO requests
        /// </summary>
        /// <returns></returns>
        public List<string> RegisterForRequests()
        {
            return new List<string>()
            {
                typeof(GoRequest).ToString()
            };
        }
        /// <summary>
        /// Handle terminal request
        /// </summary>
        /// <param name="api"></param>
        /// <param name="terminal"></param>
        /// <param name="terminalRequest"></param>
        public IResponse HandleRequest(IApi api, Terminal terminal, IRequest request, IResponse response)
        {
            if (request.GetType() == typeof(GoRequest))
            {
                var goRequest = request.GetMessageAsType<GoRequest>();
                var exit = api.GetItemManager().Read(goRequest.ItemId);
                var player = api.GetItemManager().Read(terminal.PlayerId);
                var locationAttribute = api.GetAttributeManager<IAttributeLocationManager>()
                    .Read(exit.GetAttribute<AttributeExit>().DestinationLocationId);
                var location = api.GetItemManager().Read(locationAttribute.ItemId);
                api.GetItemManager().Move(player, location, exit);
                var lookRequestHandler = new LookRequestHandler();
                response = lookRequestHandler.HandleRequest(api, terminal, new LookRequest()
                {

                }, response);
            }
            return response;
        }
    }
}
