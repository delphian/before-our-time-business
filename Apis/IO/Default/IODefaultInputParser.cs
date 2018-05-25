using BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput;
using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Repository.Models.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Default
{
    public class IODefaultInputParser : IIOInputParser
    {
        private IApi Api { set; get; }
        public IODefaultInputParser(IApi api)
        {
            Api = api;
        }
        public void ParseInput(Terminal terminal, string terminalInput)
        {
            var from = Api.GetItemManager().Read(new List<Guid>() { terminal.AvatarId }).First();
            var scriptDelegate = Api.GetScriptManager().GetDelegateDefinition("onTerminalInput");
            var clientMessage = new Message()
            {
                DelegateId = scriptDelegate.GetId(),
                Sender = from,
                Package = JsonConvert.SerializeObject(new OnTerminalInputArgument()
                {
                    Terminal = terminal,
                    Raw = terminalInput
                })
            };
            Api.GetMessageManager().SendMessage(clientMessage, Api.GetItemManager().Read());
        }
    }
}
