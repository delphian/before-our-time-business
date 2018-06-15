using BeforeOurTime.Repository.Models.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Servers.Telnet.Translate
{
    public interface ITranslate
    {
        /// <summary>
        /// Register to handle a specific set of messages
        /// </summary>
        /// <returns></returns>
        List<Type> RegisterForMessages();
        /// <summary>
        /// Translate structured data from the environment to pure text
        /// </summary>
        /// <param name="message">Message from the environment</param>
        /// <param name="telnetServer"></param>
        /// <param name="telnetClient"></param>
        void Translate(
            IMessage message,
            TelnetServer telnetServer,
            TelnetClient telnetClient);
    }
}
