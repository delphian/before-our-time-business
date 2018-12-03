using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Business.Terminals.Telnet.Ansi;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Modules.Core.ItemProperties.Visibles;
using BeforeOurTime.Models.Modules.Core.Models.Properties;
using BeforeOurTime.Models.Modules.World.Messages.Emotes;

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
                typeof(WorldEmoteEvent)
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
            var emoteEvent = message.GetMessageAsType<WorldEmoteEvent>();
            var emote = "";
            var itemName = emoteEvent.Origin.GetProperty<VisibleItemProperty>()?.Name ?? "** Something **";
            emote = emoteEvent.EmoteType == WorldEmoteType.Smile ? $"{itemName} smiles happily" : emote;
            emote = emoteEvent.EmoteType == WorldEmoteType.Frown ? $"{itemName} frowns" : emote;
            telnetServer.SendMessageToClient(telnetClient, "\r\n"
                + $"{AnsiColors.whiteB}{emote}{AnsiColors.reset}");
        }
    }
}
