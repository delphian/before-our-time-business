using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Business.Apis.IO.Updates.Models;
using BeforeOurTime.Business.Servers.Telnet;

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
        public void Translate(IIOUpdate environmentUpdate)
        {
            var ioLocationUpdate = (IOLocationUpdate)Convert.ChangeType(environmentUpdate, typeof(IOLocationUpdate));
            TelnetServer.sendMessageToClient(TelnetClient, "\r\n" + ioLocationUpdate.Name + "\r\n");
            TelnetServer.sendMessageToClient(TelnetClient, ioLocationUpdate.Description + "\r\n");
            ioLocationUpdate.Adendums.ForEach(delegate (string adendum)
            {
                TelnetServer.sendMessageToClient(TelnetClient, adendum + "\r\n");
            });
            TelnetServer.sendMessageToClient(TelnetClient, "Exits:\r\n");
            if (ioLocationUpdate.Exits.Count() > 0)
            {
                ioLocationUpdate.Exits.ForEach(delegate (IOExitUpdate ioExitUpdate)
                {
                    TelnetServer.sendMessageToClient(TelnetClient, " " + ioExitUpdate.Name + " (" + ioExitUpdate.ExitId + ")\r\n");
                });
            }
            else
            {
                TelnetServer.sendMessageToClient(TelnetClient, "None\r\n");
            }
        }
    }
}
