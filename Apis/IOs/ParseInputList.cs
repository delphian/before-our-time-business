using BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput;
using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Repository.Models.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO
{
    public class ParseInputList : IIOInputParser
    {
        private IApi Api { set; get; }
        public ParseInputList(IApi api)
        {
            Api = api;
        }
        public void ParseInput(Terminal terminal, string terminalInput)
        {
        }
    }
}
