using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Business.Servers.Telnet;
using BeforeOurTime.Business.Terminals.Telnet.Ansi;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses.List;
using BeforeOurTime.Repository.Models.Messages;

namespace BeforeOurTime.Business.Servers.Telnet.Translate
{
    public class LocationResponseTranslate : Translate, ITranslate
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LocationResponseTranslate() { }
        /// <summary>
        /// Register to handle a specific set of messages
        /// </summary>
        /// <returns></returns>
        public List<Type> RegisterForMessages()
        {
            return new List<Type>()
            {
                typeof(ListLocationResponse)
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
            var locationResponse = message.GetMessageAsType<ListLocationResponse>();
            telnetClient.ItemExits.Clear();
            // Send location name and description
            telnetServer.SendMessageToClient(telnetClient, "\r\n\r\n" 
                + $"{AnsiColors.greenB}{locationResponse.Item.Name}{AnsiColors.reset}\r\n"
                + locationResponse.Item.Description + "\r\n");
            locationResponse.Adendums.ForEach(delegate (string adendum)
            {
                telnetServer.SendMessageToClient(telnetClient, " - " + adendum + "\r\n");
            });
            // Send location exits
            string exits = null;
            if (locationResponse.Exits.Count() > 0)
            {
                locationResponse.Exits.ForEach(delegate (ListExitResponse ioExitUpdate)
                {
                    telnetClient.ItemExits.Add(ioExitUpdate);
                    exits = (exits != null) ? ", " : exits;
                    exits += $"{AnsiColors.purpleB}{ioExitUpdate.Name}{AnsiColors.reset}";
                });
            }
            else
            {
                exits = "None";
            }
            telnetServer.SendMessageToClient(telnetClient, $" - Exits: {exits}");
        }
    }
}
