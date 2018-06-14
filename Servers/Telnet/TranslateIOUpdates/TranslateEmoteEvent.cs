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

namespace BeforeOurTime.Business.Terminals.Telnet.TranslateIOUpdates
{
    public class TranslateEmoteEvent : TranslateIOUpdate, ITranslateIOUpdate
    {
        protected TelnetServer TelnetServer { set; get; }
        protected TelnetClient TelnetClient { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="telnetServer"></param>
        /// <param name="telnetClient"></param>
        public TranslateEmoteEvent(
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
            var emoteEvent = message.GetMessageAsType<EmoteEvent>();
            var emote = "";
            emote = emoteEvent.Type == EmoteType.Smile ? $"{emoteEvent.Name} smiles happily" : emote;
            emote = emoteEvent.Type == EmoteType.Frown ? $"{emoteEvent.Name} frowns" : emote;
            TelnetServer.SendMessageToClient(TelnetClient, "\r\n"
                + $"{AnsiColors.whiteB}{emote}{AnsiColors.reset}");
        }
    }
}
