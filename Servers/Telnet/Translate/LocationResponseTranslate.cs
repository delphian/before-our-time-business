﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Business.Terminals.Telnet.Ansi;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses.List;
using BeforeOurTime.Models.Modules.World.Messages.Location.ReadLocationSummary;
using BeforeOurTime.Models.Modules.World.Models.Items;

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
                typeof(WorldReadLocationSummaryResponse)
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
            var locationResponse = message.GetMessageAsType<WorldReadLocationSummaryResponse>();
            telnetClient.ItemExits.Clear();
            // Send location name and description
            telnetServer.SendMessageToClient(telnetClient, "\r\n\r\n" 
                + $"{AnsiColors.greenB}{locationResponse.Item.Visible.Name}{AnsiColors.reset}\r\n"
                + locationResponse.Item.Visible.Description + "\r\n");
            locationResponse.Adendums.ForEach(delegate (string adendum)
            {
                telnetServer.SendMessageToClient(telnetClient, " - " + adendum + "\r\n");
            });
            // Send location exits
            string exits = null;
            if (locationResponse.Exits.Count() > 0)
            {
                locationResponse.Exits.ForEach(ioExitUpdate =>
                {
                    telnetClient.ItemExits.Add(ioExitUpdate);
                    var exit = ioExitUpdate.Item.GetAsItem<ExitItem>();
                    var commands = "";
                    exit.CommandList.Commands.ForEach(use => {
                        commands += (commands == "") ? $"{use.Name}" : $"|{use.Name}";
                    });
                    exits = (exits == null) ? "" : $"{exits}, ";
                    exits += $"{AnsiColors.purpleB}{exit.Visible.Name} [{commands}]{AnsiColors.reset}";
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
