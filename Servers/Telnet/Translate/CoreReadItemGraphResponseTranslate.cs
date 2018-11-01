using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Business.Terminals.Telnet.Ansi;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Modules.Core.Messages.ItemGraph;
using BeforeOurTime.Models.Modules.Core.Messages.MoveItem;
using BeforeOurTime.Models.Modules.Core.Models.Properties;

namespace BeforeOurTime.Business.Servers.Telnet.Translate
{
    public class CoreReadItemGraphResponseTranslate : Translate, ITranslate
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CoreReadItemGraphResponseTranslate() { }
        /// <summary>
        /// Register to handle a specific set of messages
        /// </summary>
        /// <returns></returns>
        public List<Type> RegisterForMessages()
        {
            return new List<Type>()
            {
                typeof(CoreReadItemGraphResponse)
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
            var graph = message.GetMessageAsType<CoreReadItemGraphResponse>();
            void displayGraph(List<ItemGraph> graphItems, int level = 0)
            {
                if (graphItems != null && graphItems.Count > 0)
                {
                    graphItems.ForEach(graphItem =>
                    {
                        var line = "\r\n" + new string(' ', level * 2) +
                            $"{AnsiColors.purpleB}{graphItem.Name} ({graphItem.Id}){AnsiColors.reset}";
                        telnetServer.SendMessageToClient(telnetClient, line);
                        displayGraph(graphItem.Children, level + 1);
                    });
                }
            }
            displayGraph(new List<ItemGraph>() { graph.CoreReadItemGraphEvent.ItemGraph });
        }
    }
}
