using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Business.Terminals.Telnet.Ansi;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Modules.Core.Messages.ItemGraph;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson.ReadItemJson;
using BeforeOurTime.Models.Modules.Core.Messages.MoveItem;
using BeforeOurTime.Models.Modules.Core.Models.Properties;

namespace BeforeOurTime.Business.Servers.Telnet.Translate
{
    public class CoreReadItemJsonResponseTranslate : Translate, ITranslate
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CoreReadItemJsonResponseTranslate() { }
        /// <summary>
        /// Register to handle a specific set of messages
        /// </summary>
        /// <returns></returns>
        public List<Type> RegisterForMessages()
        {
            return new List<Type>()
            {
                typeof(CoreReadItemJsonResponse)
            };
        }
        /// <summary>
        /// Translate structured data from the environment to pure text
        /// </summary>
        /// <param name="message">Message from the environment</param>
        /// <param name="telnetServer"></param>
        /// <param name="telnetClient"></param>
        public void Translate(
            IMessage message,
            TelnetServer telnetServer,
            TelnetClient telnetClient)
        {
            var response = message.GetMessageAsType<CoreReadItemJsonResponse>();
            telnetServer.SendMessageToClient(telnetClient, $"\r\n{AnsiColors.purple}{response.CoreReadItemJsonEvent.ItemsJson.First().Id}{AnsiColors.reset}\r\n");
            telnetServer.SendMessageToClient(telnetClient, $"{AnsiColors.purpleB}{response.CoreReadItemJsonEvent.ItemsJson.First().JSON}{AnsiColors.reset}\r\n");
        }
    }
}
