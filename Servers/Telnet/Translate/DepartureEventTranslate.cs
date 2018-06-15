using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Business.Servers.Telnet;
using BeforeOurTime.Business.Terminals.Telnet.Ansi;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Events;
using BeforeOurTime.Repository.Models.Messages.Responses.Enumerate;

namespace BeforeOurTime.Business.Servers.Telnet.Translate
{
    public class DepartureEventTranslate : Translate, ITranslate
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DepartureEventTranslate() { }
        /// <summary>
        /// Register to handle a specific set of messages
        /// </summary>
        /// <returns></returns>
        public List<Type> RegisterForMessages()
        {
            return new List<Type>()
            {
                typeof(DepartureEvent)
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
            var departureEvent = message.GetMessageAsType<DepartureEvent>();
            telnetServer.SendMessageToClient(telnetClient, "\r\n"
                + $"{AnsiColors.green}{departureEvent.Name} has departed{AnsiColors.reset}");
        }
    }
}
