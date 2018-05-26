using BeforeOurTime.Business.Apis.IO.Requests.Models;
using BeforeOurTime.Business.Apis.Items.Details;
using BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput;
using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Repository.Models.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Requests.Handlers
{
    public class IODefaultRequestHandler : IIORequestHandler
    {
        public IODefaultRequestHandler()
        {
        }
        public void HandleRequest(IApi api, Terminal terminal, IIORequest terminalInput)
        {
            var from = terminal.Character.Item;
            var scriptDelegate = api.GetScriptManager().GetDelegateDefinition("onTerminalInput");
            var clientMessage = new Message()
            {
                DelegateId = scriptDelegate.GetId(),
                Sender = from,
                Package = JsonConvert.SerializeObject(new OnTerminalInputArgument()
                {
                    Terminal = terminal,
                    Raw = terminalInput.GetId().ToString()
                })
            };
            api.GetMessageManager().SendMessage(clientMessage, api.GetItemManager().Read());
        }
    }
}
