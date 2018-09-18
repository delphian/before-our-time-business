using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Business.Servers.Telnet;
using BeforeOurTime.Business.Terminals.Telnet.Ansi;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Events.Arrivals;

namespace BeforeOurTime.Business.Servers.Telnet.Translate
{
    public class ArrivalEventTranslate : Translate, ITranslate
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ArrivalEventTranslate() { }
        /// <summary>
        /// Register to handle a specific set of messages
        /// </summary>
        /// <returns></returns>
        public List<Type> RegisterForMessages()
        {
            return new List<Type>()
            {
                typeof(ArrivalEvent)
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
            var arrivalEvent = message.GetMessageAsType<ArrivalEvent>();
            telnetServer.SendMessageToClient(telnetClient, "\r\n"
                + $"{AnsiColors.green}{arrivalEvent.Name} has arrived{AnsiColors.reset}");
        }
    }
}
