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
        public static IServiceProvider ServiceProvider { set; get; }
        public static ITerminalManager TerminalManager { set; get; }
        public static TelnetServer s { set; get; }
        public static Dictionary<uint, Terminal> Terminals = new Dictionary<uint, Terminal>();
        public static Dictionary<uint, string> UserName = new Dictionary<uint, string>();
        public static Dictionary<Guid, TelnetClient> Clients = new Dictionary<Guid, TelnetClient>();

        public Server(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            TerminalManager = ServiceProvider.GetService<ITerminalManager>();
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
            var terminal = TerminalManager.RequestTerminal();
            Terminals[c.getClientID()] = terminal;
            UserName[c.getClientID()] = "";
            Console.WriteLine("Terminal granted " + terminal.Id + "\r\n");
            s.sendMessageToClient(c, "Terminal granted " + terminal.Id + "\r\nLogin: ");
        }

        private static void clientDisconnected(TelnetClient c)
        {
            Console.WriteLine("Terminal disconnected " + Terminals[c.getClientID()].Id + "\r\n");
            TerminalManager.DestroyTerminal(Terminals[c.getClientID()]);
            Terminals[c.getClientID()] = null;
        }

        private static void connectionBlocked(IPEndPoint ep)
        {
            Console.WriteLine(string.Format("BLOCKED: {0}:{1} at {2}", ep.Address, ep.Port, DateTime.Now));
        }

        private static void messageReceived(TelnetClient c, string message)
        {
            Console.WriteLine(Terminals[c.getClientID()].Id + ": " + message);
            EClientStatus status = c.getCurrentStatus();
            if (Terminals[c.getClientID()].Status == TerminalStatus.Guest && UserName[c.getClientID()] == "")
            {
                UserName[c.getClientID()] = message;
                s.sendMessageToClient(c, "\r\nPassword: ");
                c.setStatus(EClientStatus.Authenticating);
            }
            else if (Terminals[c.getClientID()].Status == TerminalStatus.Guest && UserName[c.getClientID()] != "")
            {
                TerminalManager.AuthenticateTerminal(Terminals[c.getClientID()], UserName[c.getClientID()], message);
                if (Terminals[c.getClientID()].Status == TerminalStatus.Authenticated)
                {
                    Terminals[c.getClientID()].OnMessageToTerminal += MessageFromServer;
                    Clients[Terminals[c.getClientID()].Id] = c;
                    s.clearClientScreen(c);
                    s.sendMessageToClient(c, "Terminal authenticated on account " + Terminals[c.getClientID()].AccountId + ".\r\n > ");
                    s.sendMessageToClient(c, "\r\nCharacter: ");
                    c.setStatus(EClientStatus.LoggedIn);
                } else
                {
                    s.sendMessageToClient(c, "\r\nFAIL!\r\nLogin: ");
                    c.setStatus(EClientStatus.Guest);
                }
            }
            else if (Terminals[c.getClientID()].Status == TerminalStatus.Authenticated)
            {
                s.sendMessageToClient(c, "\r\nCharacter: ");
            }
            else if (Terminals[c.getClientID()].Status == TerminalStatus.Attached)
            {
                Terminals[c.getClientID()].SendToApi(message);
                s.sendMessageToClient(c, "\r\n > ");
            }
            // s.kickClient(c);
        }
    }
}
