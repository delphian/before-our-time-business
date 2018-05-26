using BeforeOurTime.Business.Apis.IO.Requests.Models;
using BeforeOurTime.Business.Apis.IO.Updates.Models;
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
    public class IOLookRequestHandler : IIORequestHandler
    {
        public IOLookRequestHandler()
        {
        }
        public void HandleRequest(IApi api, Terminal terminal, IIORequest terminalInput)
        {
            terminal.SendToClient(new IOLookUpdate()
            {

            });
        }
    }
}
