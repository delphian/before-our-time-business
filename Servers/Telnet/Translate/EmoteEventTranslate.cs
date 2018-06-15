using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Business.Servers.Telnet;
using BeforeOurTime.Business.Terminals.Telnet.Ansi;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Events;
using BeforeOurTime.Repository.Models.Messages.Events.Emotes;
using BeforeOurTime.Repository.Models.Messages.Responses.Enumerate;

namespace BeforeOurTime.Business.Servers.Telnet.Translate
{
    public class EmoteEventTranslate : Translate, ITranslate
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public EmoteEventTranslate() { }
        /// <summary>
        /// Register to handle a specific set of messages
        /// </summary>
        /// <returns></returns>
        public List<Type> RegisterForMessages()
        {
            return new List<Type>()
            {
                typeof(EmoteEvent)
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
            var emoteEvent = message.GetMessageAsType<EmoteEvent>();
            var emote = "";
            emote = emoteEvent.Type == EmoteType.Smile ? $"{emoteEvent.Name} smiles happily" : emote;
            emote = emoteEvent.Type == EmoteType.Frown ? $"{emoteEvent.Name} frowns" : emote;
            telnetServer.SendMessageToClient(telnetClient, "\r\n"
                + $"{AnsiColors.whiteB}{emote}{AnsiColors.reset}");
        }
    }
}
