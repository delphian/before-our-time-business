using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Business.Servers.Telnet;
using BeforeOurTime.Business.Terminals.Telnet.Ansi;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Events;
using BeforeOurTime.Repository.Models.Messages.Responses.Enumerate;

namespace BeforeOurTime.Business.Terminals.Telnet.TranslateIOUpdates
{
    public class TranslateDepartureEvent : TranslateIOUpdate, ITranslateIOUpdate
    {
        protected TelnetServer TelnetServer { set; get; }
        protected TelnetClient TelnetClient { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="telnetServer"></param>
        /// <param name="telnetClient"></param>
        public TranslateDepartureEvent(
            TelnetServer telnetServer, 
            TelnetClient telnetClient)
        {
            TelnetServer = telnetServer;
            TelnetClient = telnetClient;
        }
        /// <summary>
        /// Translate structured data from the environment to pure text
        /// </summary>
        /// <param name="message">Message from the environment</param>
        public void Translate(IMessage message)
        {
            var departureEvent = message.GetMessageAsType<DepartureEvent>();
            TelnetServer.SendMessageToClient(TelnetClient, "\r\n"
                + $"{AnsiColors.green}{departureEvent.Name} has departed{AnsiColors.reset}");
        }
    }
}
