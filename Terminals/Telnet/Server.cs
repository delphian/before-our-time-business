using BeforeOurTime.Business.Terminals;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using BeforeOurTime.Repository.Models.Items;
using System.Linq;

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

        private static void MessageFromServer(Terminal terminal, string messageFromServer)
        {
            s.sendMessageToClient(Clients[terminal.Id], "\r\n");
            s.sendMessageToClient(Clients[terminal.Id], messageFromServer);
            s.sendMessageToClient(Clients[terminal.Id], "\r\n> ");
        }

        private static void clientConnected(TelnetClient c)
        {
            c.SetTerminal(TerminalManager.RequestTerminal());
            c.GetTerminal().DataBag["step"] = "connected";
            s.clearClientScreen(c);
            s.sendMessageToClient(c, "Terminal granted " + c.GetTerminal().Id + ".\r\n\r\n");
            s.sendMessageToClient(c, "Welcome to Before Our Time. For help type \"help\".\r\n\r\n");
            s.sendMessageToClient(c, "Welcome> ");
        }

        private static void clientDisconnected(TelnetClient c)
        {
            TerminalManager.DestroyTerminal(c.GetTerminal());
            c.SetTerminal(null);
        }

        private static void connectionBlocked(IPEndPoint ep)
        {
        }

        private static void messageReceived(TelnetClient c, string message)
        {
            if (c.GetTerminal().Status == TerminalStatus.Guest)
            {
                HandleMessageFromGuest(c, message);
            }
            else if (c.GetTerminal().Status == TerminalStatus.Authenticated)
            {
                HandleMessageFromAuthenticated(c, message);
            }
            else if (c.GetTerminal().Status == TerminalStatus.Attached)
            {
                switch (message)
                {
                    case "bye":
                    case "q":
                    case "exit":
                        s.sendMessageToClient(c, "\r\nCya...\r\n");
                        s.kickClient(c);
                        break;
                    default:
                        c.GetTerminal().SendToApi(message);
                        s.sendMessageToClient(c, "\r\n> ");
                        break;
                }
            }
        }
        /// <summary>
        /// Handle all client messages from terminal with Guest status
        /// </summary>
        /// <param name="c">Telnet client</param>
        /// <param name="message">message from client</param>
        private static void HandleMessageFromGuest(TelnetClient c, string message)
        {
            if (c.GetTerminal().DataBag["step"] == "connected")
            {
                switch (message.ToLower())
                {
                    case "?":
                    case "help":
                        s.sendMessageToClient(c, "\r\n\r\n");
                        s.sendMessageToClient(c, "  new    - Create a new account\r\n");
                        s.sendMessageToClient(c, "  login  - Login to an existing account\r\n");
                        s.sendMessageToClient(c, "  bye    - Disconnect\r\n\r\n");
                        s.sendMessageToClient(c, "Welcome> ");
                        break;
                    case "q":
                    case "bye":
                        s.sendMessageToClient(c, "Cya...\r\n");
                        s.kickClient(c);
                        break;
                    case "new":
                        s.sendMessageToClient(c, "\r\n");
                        s.sendMessageToClient(c, "Name: ");
                        c.GetTerminal().DataBag["step"] = "create_name";
                        break;
                    case "login":
                        s.sendMessageToClient(c, "\r\n");
                        s.sendMessageToClient(c, "Name: ");
                        c.GetTerminal().DataBag["step"] = "login_name";
                        break;
                    default:
                        s.sendMessageToClient(c, "\r\nUnknown welcome command \"" + message + "\".\r\n\r\n");
                        s.sendMessageToClient(c, "Welcome> ");
                        break;
                }
            }
            else if (c.GetTerminal().DataBag["step"] == "login_name")
            {
                c.GetTerminal().DataBag["login_name"] = message;
                c.GetTerminal().DataBag["step"] = "login_password";
                s.sendMessageToClient(c, "\r\n");
                s.sendMessageToClient(c, "Password: ");
            }
            else if (c.GetTerminal().DataBag["step"] == "login_password")
            {
                if (c.GetTerminal().Authenticate(c.GetTerminal().DataBag["login_name"], message))
                {
                    c.GetTerminal().OnMessageToTerminal += MessageFromServer;
                    Clients[c.GetTerminal().Id] = c;
                    c.GetTerminal().DataBag["step"] = "authenticated";
                    s.sendMessageToClient(c, "\r\n");
                    s.sendMessageToClient(c, "Hello " + c.GetTerminal().DataBag["login_name"] + "\r\n\r\n");
                    s.sendMessageToClient(c, "Account> ");
                }
                else
                {
                    s.sendMessageToClient(c, "\r\n");
                    s.sendMessageToClient(c, "Bad username or password.\r\n\r\n");
                    s.sendMessageToClient(c, "Welcome> ");
                }
            }
            else if (c.GetTerminal().DataBag["step"] == "create_name")
            {
                c.GetTerminal().DataBag["create_name"] = message;
                c.GetTerminal().DataBag["step"] = "create_email";
                s.sendMessageToClient(c, "\r\n");
                s.sendMessageToClient(c, "Email: ");
            }
            else if (c.GetTerminal().DataBag["step"] == "create_email")
            {
                c.GetTerminal().DataBag["create_email"] = message;
                c.GetTerminal().DataBag["step"] = "create_password";
                s.sendMessageToClient(c, "\r\n");
                s.sendMessageToClient(c, "Password: ");
            }
            else if (c.GetTerminal().DataBag["step"] == "create_password")
            {
                if (c.GetTerminal().CreateAccount(
                    c.GetTerminal().DataBag["create_name"],
                    c.GetTerminal().DataBag["create_email"],
                    message))
                {
                    c.GetTerminal().OnMessageToTerminal += MessageFromServer;
                    Clients[c.GetTerminal().Id] = c;
                    c.GetTerminal().DataBag["step"] = "authenticated";
                    s.sendMessageToClient(c, "\r\n");
                    s.sendMessageToClient(c, "Hello " + c.GetTerminal().DataBag["create_name"] + "\r\n\r\n");
                    s.sendMessageToClient(c, "Account> ");
                }
                else
                {
                    s.sendMessageToClient(c, "\r\n");
                    s.sendMessageToClient(c, "Unable to create account.\r\n\r\n");
                    s.sendMessageToClient(c, "Welcome> ");
                }
            }
        }
        /// <summary>
        /// Handle all client messages from terminal with Guest status
        /// </summary>
        /// <param name="c">Telnet client</param>
        /// <param name="message">message from client</param>
        private static void HandleMessageFromAuthenticated(TelnetClient c, string message)
        {
            if (c.GetTerminal().DataBag["step"] == "create_character")
            {
                MessageStepCreateCharacter(c, message);
            }
            else if (c.GetTerminal().DataBag["step"] == "authenticated")
            {
                switch (message.Split(' ').First().ToLower())
                {
                    case "?":
                    case "help":
                        s.sendMessageToClient(c, "\r\n\r\n");
                        s.sendMessageToClient(c, "  new         - Create a new character\r\n");
                        s.sendMessageToClient(c, "  list        - List existing characters\r\n");
                        s.sendMessageToClient(c, "  play {name} - Play an existing character\r\n");
                        s.sendMessageToClient(c, "  back        - Return to previous screen\r\n");
                        s.sendMessageToClient(c, "  bye         - Disconnect\r\n\r\n");
                        s.sendMessageToClient(c, "Account> ");
                        break;
                    case "back":
                        c.GetTerminal().Status = TerminalStatus.Guest;
                        c.GetTerminal().DataBag["step"] = "connected";
                        s.sendMessageToClient(c, "\r\nWelcome> ");
                        break;
                    case "new":
                        c.GetTerminal().DataBag["step"] = "create_character";
                        MessageStepCreateCharacter(c, message);
                        break;
                    case "list":
                        s.sendMessageToClient(c, "\r\n\r\n");
                        var characters = c.GetTerminal().GetAttachable();
                        characters.ForEach(delegate (Character character)
                        {
                            s.sendMessageToClient(c, "  " + character.Name + " (" + character.Id + ")\r\n");
                        });
                        s.sendMessageToClient(c, "\r\n");
                        s.sendMessageToClient(c, "Account> ");
                        break;
                    case "q":
                    case "bye":
                        s.sendMessageToClient(c, "Cya...\r\n");
                        s.kickClient(c);
                        break;
                    case "play":
                        var name = message.Split(' ').Last().ToLower();
                        c.GetTerminal().GetAttachable().ForEach(delegate (Character character)
                        {
                            if (character.Name.ToLower() == name)
                            {
                                if (c.GetTerminal().Attach(character.Id))
                                {
                                    c.GetTerminal().DataBag["step"] = "attached";
                                    s.sendMessageToClient(c, "\r\n");
                                    s.sendMessageToClient(c, "Terminal attached to avatar. Play!\r\n\r\n");
                                    s.sendMessageToClient(c, "> ");
                                }
                            }
                        });
                        if (c.GetTerminal().Status != TerminalStatus.Attached)
                        {
                            s.sendMessageToClient(c, "\r\nUnknown character \"" + name + "\"\r\n");
                            s.sendMessageToClient(c, "Account> ");
                        }
                        break;
                    default:
                        s.sendMessageToClient(c, "\r\nUnknown account command \"" + message + "\".\r\n\r\n");
                        s.sendMessageToClient(c, "Account> ");
                        break;
                }
            }
        }
        /// <summary>
        /// Create new character during login process
        /// </summary>
        /// <remarks>
        /// Handles all messages when DataBag step is "create_character"
        /// </remarks>
        /// <param name="c">Telnet client</param>
        /// <param name="message">message from client</param>
        private static void MessageStepCreateCharacter(TelnetClient c, string message)
        {
            if (!c.GetTerminal().DataBag.ContainsKey("create_character_step"))
            {
                c.GetTerminal().DataBag["create_character_step"] = "create";
            }
            switch (c.GetTerminal().DataBag["create_character_step"])
            {
                case "create":
                    s.sendMessageToClient(c, "\r\nOk, let's create a new character.\r\n\r\n");
                    s.sendMessageToClient(c, "\r\n Name: ");
                    c.GetTerminal().DataBag["create_character_step"] = "save_name";
                    break;
                case "save_name":
                    c.GetTerminal().DataBag["create_character_name"] = message;
                    if (c.GetTerminal().CreateCharacter(c.GetTerminal().DataBag["create_character_name"]))
                    {
                        c.GetTerminal().DataBag["step"] = "attached";
                        s.sendMessageToClient(c, "\r\nTerminal attached to avatar. Play!\r\n\r\n");
                        s.sendMessageToClient(c, "> ");
                    } else
                    {
                        c.GetTerminal().DataBag["step"] = "attached";
                        c.GetTerminal().DataBag.Remove("create_character_name");
                        c.GetTerminal().DataBag.Remove("create_character_step");
                        s.sendMessageToClient(c, "\r\nSomething went wrong. Character not created.\r\n\r\n");
                        s.sendMessageToClient(c, "Account> ");
                    }
                    break;
            }
        }

    }
}
