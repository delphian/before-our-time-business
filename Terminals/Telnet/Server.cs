using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BeforeOurTime.Business.Servers.Telnet
{
    public class Server
    {
        public static TelnetServer s { set; get; }

        public Server()
        {
            s = new TelnetServer(IPAddress.Any);
            s.ClientConnected += clientConnected;
            s.ClientDisconnected += clientDisconnected;
            s.ConnectionBlocked += connectionBlocked;
            s.MessageReceived += messageReceived;
            s.start();

            Console.WriteLine("SERVER STARTED: " + DateTime.Now);
        }

        private static void clientConnected(TelnetClient c)
        {
            Console.WriteLine("CONNECTED: " + c);

            s.sendMessageToClient(c, "Telnet Server\r\nLogin: ");
        }

        private static void clientDisconnected(TelnetClient c)
        {
            Console.WriteLine("DISCONNECTED: " + c);
        }

        private static void connectionBlocked(IPEndPoint ep)
        {
            Console.WriteLine(string.Format("BLOCKED: {0}:{1} at {2}", ep.Address, ep.Port, DateTime.Now));
        }

        private static void messageReceived(TelnetClient c, string message)
        {
            Console.WriteLine("MESSAGE: " + message);
            EClientStatus status = c.getCurrentStatus();
            if (status == EClientStatus.Guest)
            {
                if (message == "root")
                {
                    s.sendMessageToClient(c, "\r\nPassword: ");
                    c.setStatus(EClientStatus.Authenticating);
                }
            }
            else if (status == EClientStatus.Authenticating)
            {
                if (message == "r00t")
                {
                    s.clearClientScreen(c);
                    s.sendMessageToClient(c, "Successfully authenticated.\r\n > ");
                    // Request terminal from API
                    c.setStatus(EClientStatus.LoggedIn);
                }
            }
            else
            {
                // Forward message to API
                s.sendMessageToClient(c, "\r\n > ");
            }
            // s.kickClient(c);
        }
    }
}
