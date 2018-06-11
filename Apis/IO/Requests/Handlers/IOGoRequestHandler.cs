using BeforeOurTime.Business.Apis.Items.Attributes.Interfaces;
using BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput;
using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Requests;
using BeforeOurTime.Repository.Models.Messages.Requests.Look;
using BeforeOurTime.Repository.Models.Messages.Responses.Enumerate;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Requests.Handlers
{
    public class IOGoRequestHandler : IIORequestHandler
    {
        public IOGoRequestHandler()
        {
        }
        /// <summary>
        /// Register to handle a specific set of IO requests
        /// </summary>
        /// <returns></returns>
        public List<string> RegisterForIORequests()
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
        public void HandleIORequest(IApi api, Terminal terminal, IRequest terminalInput)
        {
            if (terminalInput.GetType() == typeof(GoRequest))
            {
                var ioGoRequest = (GoRequest) Convert.ChangeType(terminalInput, typeof(GoRequest));
                var locationExit = api.GetAttributeManager<IAttributeExitManager>().Read(ioGoRequest.ExitId);
                var locationAttribute = api.GetAttributeManager<IAttributeLocationManager>().Read(locationExit.DestinationLocationId);
                var playerAttribute = api.GetAttributeManager<IAttributePlayerManager>().Read(terminal.PlayerId);
                var newLocation = api.GetItemManager().Read(locationAttribute.ItemId);
                api.GetItemManager().Move(playerAttribute.Item, newLocation, locationExit.Item);

                var lookRequestHandler = new IOLookRequestHandler();
                lookRequestHandler.HandleIORequest(api, terminal, new LookRequest()
                {

                });
            }
        }
    }
}
