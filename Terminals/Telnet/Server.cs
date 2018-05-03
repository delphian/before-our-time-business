using BeforeOurTime.Business.Terminals;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;

namespace BeforeOurTime.Business.Servers.Telnet
{
    public class Server
    {
        public static TelnetServer s { set; get; }
        public static IServiceProvider ServiceProvider { set; get; }
        public static Dictionary<uint, Terminal> Terminals = new Dictionary<uint, Terminal>();
        public static Dictionary<uint, string> UserName = new Dictionary<uint, string>();
        public static Dictionary<Guid, TelnetClient> Clients = new Dictionary<Guid, TelnetClient>();

        public Server(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            s = new TelnetServer(IPAddress.Any);
            s.ClientConnected += clientConnected;
            s.ClientDisconnected += clientDisconnected;
            s.ConnectionBlocked += connectionBlocked;
            s.MessageReceived += messageReceived;
            s.start();

            Console.WriteLine("SERVER STARTED: " + DateTime.Now);
        }

        private static void MessageFromServer(Guid terminalId, string messageFromServer)
        {
            s.sendMessageToClient(Clients[terminalId], messageFromServer);
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
                UserName[c.getClientID()] = message;
                s.sendMessageToClient(c, "\r\nPassword: ");
                c.setStatus(EClientStatus.Authenticating);
            }
            else if (status == EClientStatus.Authenticating)
            {
                var terminalManager = ServiceProvider.GetService<ITerminalManager>();
                var terminal = terminalManager.RequestTerminal(UserName[c.getClientID()], message);
                if (terminal != null)
                {
                    Terminals[c.getClientID()] = terminal;
                    Terminals[c.getClientID()].OnMessageToTerminal += MessageFromServer;
                    Clients[terminal.Id] = c;
                    s.clearClientScreen(c);
                    s.sendMessageToClient(c, "Terminal granted " + terminal.Id + ".\r\n > ");
                    c.setStatus(EClientStatus.LoggedIn);
                } else
                {
                    s.sendMessageToClient(c, "\r\nFAIL!\r\nLogin: ");
                    c.setStatus(EClientStatus.Guest);
                }
            }
            else
            {
                Terminals[c.getClientID()].SendToServer(message);
                s.sendMessageToClient(c, "\r\n > ");
            }
            // s.kickClient(c);
        }
    }
}
