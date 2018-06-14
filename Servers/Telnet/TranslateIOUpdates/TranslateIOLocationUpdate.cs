using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Business.Servers.Telnet;
using BeforeOurTime.Business.Terminals.Telnet.Ansi;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Responses.Enumerate;

namespace BeforeOurTime.Business.Terminals.Telnet.TranslateIOUpdates
{
    public class TranslateIOLocationUpdate : TranslateIOUpdate, ITranslateIOUpdate
    {
        protected TelnetServer TelnetServer { set; get; }
        protected TelnetClient TelnetClient { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="telnetServer"></param>
        /// <param name="telnetClient"></param>
        public TranslateIOLocationUpdate(
            TelnetServer telnetServer, 
            TelnetClient telnetClient)
        {
            TelnetServer = telnetServer;
            TelnetClient = telnetClient;
        }
        /// <summary>
        /// Translate structured data from the environment to pure text
        /// </summary>
        /// <param name="environmentUpdate">Update from the environment</param>
        public void Translate(IMessage environmentUpdate)
        {
            var ioLocationUpdate = (LocationResponse)Convert.ChangeType(environmentUpdate, typeof(LocationResponse));
            TelnetClient.ItemExits.Clear();
            // Send location name and description
            TelnetServer.SendMessageToClient(TelnetClient, "\r\n\r\n" 
                + $"{AnsiColors.greenB}{ioLocationUpdate.Name}{AnsiColors.reset}\r\n"
                + ioLocationUpdate.Description + "\r\n");
            ioLocationUpdate.Adendums.ForEach(delegate (string adendum)
            {
                TelnetServer.SendMessageToClient(TelnetClient, " - " + adendum + "\r\n");
            });
            // Send location exits
            string exits = null;
            if (ioLocationUpdate.Exits.Count() > 0)
            {
                ioLocationUpdate.Exits.ForEach(delegate (ExitResponse ioExitUpdate)
                {
                    TelnetClient.ItemExits.Add(ioExitUpdate);
                    exits = (exits != null) ? ", " : exits;
                    exits += $"{AnsiColors.purpleB}{ioExitUpdate.Name}{AnsiColors.reset}";
                });
            }
            else
            {
                exits = "None";
            }
            TelnetServer.SendMessageToClient(TelnetClient, $" - Exits: {exits}");
        }
    }
}
