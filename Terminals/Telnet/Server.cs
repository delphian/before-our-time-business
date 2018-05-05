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
            c.SetTerminal(TerminalManager.RequestTerminal());
            UserName[c.getClientID()] = "";
            Console.WriteLine("Terminal granted " + c.GetTerminal().Id + "\r\n");
            s.sendMessageToClient(c, "Terminal granted " + c.GetTerminal().Id + "\r\nLogin: ");
        }

        private static void clientDisconnected(TelnetClient c)
        {
            Console.WriteLine("Terminal disconnected " + c.GetTerminal().Id + "\r\n");
            TerminalManager.DestroyTerminal(c.GetTerminal());
            c.SetTerminal(null);
        }

        private static void connectionBlocked(IPEndPoint ep)
        {
            Console.WriteLine(string.Format("BLOCKED: {0}:{1} at {2}", ep.Address, ep.Port, DateTime.Now));
        }

        private static void messageReceived(TelnetClient c, string message)
        {
            Terminal terminal = c.GetTerminal();
            Console.WriteLine(terminal.Id + ": " + message);
            EClientStatus status = c.getCurrentStatus();
            if (terminal.Status == TerminalStatus.Guest && UserName[c.getClientID()] == "")
            {
                UserName[c.getClientID()] = message;
                s.sendMessageToClient(c, "\r\nPassword: ");
                c.setStatus(EClientStatus.Authenticating);
            }
            else if (terminal.Status == TerminalStatus.Guest && UserName[c.getClientID()] != "")
            {
                if (terminal.Authenticate(UserName[c.getClientID()], message))
                {
                    terminal.OnMessageToTerminal += MessageFromServer;
                    Clients[terminal.Id] = c;
                    s.clearClientScreen(c);
                    s.sendMessageToClient(c, "Terminal authenticated on account " + c.GetTerminal().AccountId + ".\r\n");
                    s.sendMessageToClient(c, "\r\nCharacter: ");
                    c.setStatus(EClientStatus.LoggedIn);
                } else
                {
                    s.sendMessageToClient(c, "\r\nFAIL!\r\nLogin: ");
                    c.setStatus(EClientStatus.Guest);
                }
            }
            else if (terminal.Status == TerminalStatus.Authenticated)
            {
                Guid characterId;
                Guid.TryParse(message, out characterId);
                if (terminal.Attach(characterId))
                {
                    s.sendMessageToClient(c, "Terminal attached to avatar " + terminal.ItemUuid + ".\r\n > ");

                } else
                {
                    s.sendMessageToClient(c, "\r\nNo such avatar\r\nCharacter: ");
                }
            }
            else if (terminal.Status == TerminalStatus.Attached)
            {
                c.GetTerminal().SendToApi(message);
                s.sendMessageToClient(c, "\r\n > ");
            }
            // s.kickClient(c);
        }
    }
}
