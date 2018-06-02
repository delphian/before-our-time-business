using BeforeOurTime.Business.Terminals;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using BeforeOurTime.Repository.Models.Items;
using System.Linq;
using BeforeOurTime.Repository.Models.Items.Attributes;
using BeforeOurTime.Business.Apis.IO.Updates.Models;
using BeforeOurTime.Business.Apis.IO.Requests.Models;
using Newtonsoft.Json;
using BeforeOurTime.Business.Terminals.Telnet.TranslateIOUpdates;

namespace BeforeOurTime.Business.Servers.Telnet
{
    public class TelnetManager
    {
        public static IServiceProvider ServiceProvider { set; get; }
        public static ITerminalManager TerminalManager { set; get; }
        public static TelnetServer s { set; get; }
        public static Dictionary<uint, string> UserName = new Dictionary<uint, string>();
        public static Dictionary<Guid, TelnetClient> Clients = new Dictionary<Guid, TelnetClient>();
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceProvider"></param>
        public TelnetManager(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            TerminalManager = ServiceProvider.GetService<ITerminalManager>();
            s = new TelnetServer(IPAddress.Any);
            s.ClientConnected += ClientConnected;
            s.ClientDisconnected += ClientDisconnected;
            s.ConnectionBlocked += ClientBlocked;
            s.MessageReceived += MessageFromClient;
            s.start();
            Console.WriteLine("TELNET SERVER STARTED: " + DateTime.Now);
        }
        /// <summary>
        /// Assign new telnet client an environment terminal and send greeting
        /// </summary>
        /// <param name="telnetClient"></param>
        private static void ClientConnected(TelnetClient telnetClient)
        {
            telnetClient.SetTerminal(TerminalManager.RequestTerminal());
            telnetClient.GetTerminal().DataBag["step"] = "connected";
            s.clearClientScreen(telnetClient);
            s.sendMessageToClient(telnetClient, "Terminal granted " + telnetClient.GetTerminal().Id + ".\r\n\r\n");
            s.sendMessageToClient(telnetClient, "Welcome to Before Our Time. For help type \"help\".\r\n\r\n");
            s.sendMessageToClient(telnetClient, "Welcome> ");
        }
        /// <summary>
        /// Remove terminal from disconnected telnet client
        /// </summary>
        /// <param name="telnetClient"></param>
        private static void ClientDisconnected(TelnetClient telnetClient)
        {
            TerminalManager.DestroyTerminal(telnetClient.GetTerminal());
            telnetClient.SetTerminal(null);
        }
        /// <summary>
        /// TODO : Record attempt from blocked ip address
        /// </summary>
        /// <param name="ep"></param>
        private static void ClientBlocked(IPEndPoint ep)
        {
        }
        /// <summary>
        /// Process a message from the terminal to the telnet client
        /// </summary>
        /// <param name="terminal"></param>
        /// <param name="environmentUpdate"></param>
        private static void MessageFromTerminal(Terminal terminal, IIOUpdate environmentUpdate)
        {
            if (environmentUpdate.GetType() == typeof(IOLocationUpdate))
            {
                new TranslateIOLocationUpdate(s, Clients[terminal.Id]).Translate(environmentUpdate);
            }
            else
            {
                s.sendMessageToClient(Clients[terminal.Id], "\r\nUnknown message from server:\r\n");
                s.sendMessageToClient(Clients[terminal.Id], JsonConvert.SerializeObject(environmentUpdate));
            }
            s.sendMessageToClient(Clients[terminal.Id], "\r\n\r\n> ");
        }
        /// <summary>
        /// Process a message from the telnet client (MFC) to the terminal
        /// </summary>
        /// <param name="telnetClient"></param>
        /// <param name="message"></param>
        private static void MessageFromClient(TelnetClient telnetClient, string message)
        {
            if (telnetClient.GetTerminal().Status == TerminalStatus.Guest)
            {
                MFCTerminalGuest(telnetClient, message);
            }
            else if (telnetClient.GetTerminal().Status == TerminalStatus.Authenticated)
            {
                MFCTerminalAuthenticated(telnetClient, message);
            }
            else if (telnetClient.GetTerminal().Status == TerminalStatus.Attached)
            {
                MFCTerminalAttached(telnetClient, message);
            }
        }
        /// <summary>
        /// Handle Message From client when associated terminal is in attached status (playing!)
        /// </summary>
        /// <param name="telnetClient"></param>
        /// <param name="message"></param>
        private static void MFCTerminalAttached(TelnetClient telnetClient, string message)
        {
            switch (message)
            {
                case "bye":
                case "q":
                case "exit":
                    s.sendMessageToClient(telnetClient, "\r\nCya...\r\n");
                    s.kickClient(telnetClient);
                    break;
                case "look":
                    s.sendMessageToClient(telnetClient, "\r\n");
                    telnetClient.GetTerminal().SendToApi(new IOLookRequest()
                    {

                    });
                    break;
                default:
                    s.sendMessageToClient(telnetClient, "\r\nBad command.\r\n> ");
                    break;
            }
        }
        /// <summary>
        /// Handle Message From Client when associated terminal is in guest status
        /// </summary>
        /// <param name="c">Telnet client</param>
        /// <param name="message">message from client</param>
        private static void MFCTerminalGuest(TelnetClient c, string message)
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
                        s.sendMessageToClient(c, "Email: ");
                        c.GetTerminal().DataBag["step"] = "login_name";
                        break;
                    default:
                        s.sendMessageToClient(c, "\r\nUnknown welcome command \"" + message + "\". Try \"help\".\r\n\r\n");
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
                    c.GetTerminal().OnMessageToTerminal += MessageFromTerminal;
                    Clients[c.GetTerminal().Id] = c;
                    c.GetTerminal().DataBag["step"] = "authenticated";
                    s.sendMessageToClient(c, "\r\n");
                    s.sendMessageToClient(c, "Hello " + c.GetTerminal().DataBag["login_name"] + "\r\n\r\n");
                    s.sendMessageToClient(c, "Account> ");
                }
                else
                {
                    c.GetTerminal().DataBag["step"] = "connected";
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
                    c.GetTerminal().OnMessageToTerminal += MessageFromTerminal;
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
        /// Handle Message From Client when associated terminal is in authenticated status
        /// </summary>
        /// <param name="c">Telnet client</param>
        /// <param name="message">message from client</param>
        private static void MFCTerminalAuthenticated(TelnetClient c, string message)
        {
            if (c.GetTerminal().DataBag["step"] == "create_character")
            {
                MFCTerminalAuthenticatedCreatePlayer(c, message);
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
                        MFCTerminalAuthenticatedCreatePlayer(c, message);
                        break;
                    case "list":
                        s.sendMessageToClient(c, "\r\n\r\n");
                        var characters = c.GetTerminal().GetAttachable();
                        characters.ForEach(delegate (AttributePlayer character)
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
                        c.GetTerminal().GetAttachable().ForEach(delegate (AttributePlayer character)
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
                        s.sendMessageToClient(c, "\r\nUnknown account command \"" + message + "\". Try \"help\".\r\n\r\n");
                        s.sendMessageToClient(c, "Account> ");
                        break;
                }
            }
        }
        /// <summary>
        /// Handle Message From Client when associated terminal is in authenticated status
        /// </summary>
        /// <remarks>
        /// Handles all messages when DataBag step is "create_character"
        /// </remarks>
        /// <param name="telnetClient">Telnet client</param>
        /// <param name="message">message from client</param>
        private static void MFCTerminalAuthenticatedCreatePlayer(TelnetClient telnetClient, string message)
        {
            if (!telnetClient.GetTerminal().DataBag.ContainsKey("create_character_step"))
            {
                telnetClient.GetTerminal().DataBag["create_character_step"] = "create";
            }
            switch (telnetClient.GetTerminal().DataBag["create_character_step"])
            {
                case "create":
                    s.sendMessageToClient(telnetClient, "\r\nOk, let's create a new character.\r\n\r\n");
                    s.sendMessageToClient(telnetClient, "\r\n Name: ");
                    telnetClient.GetTerminal().DataBag["create_character_step"] = "save_name";
                    break;
                case "save_name":
                    telnetClient.GetTerminal().DataBag["create_character_name"] = message;
                    if (telnetClient.GetTerminal().CreateCharacter(telnetClient.GetTerminal().DataBag["create_character_name"]))
                    {
                        telnetClient.GetTerminal().DataBag["step"] = "attached";
                        s.sendMessageToClient(telnetClient, "\r\nTerminal attached to avatar. Play!\r\n\r\n");
                        s.sendMessageToClient(telnetClient, "> ");
                    } else
                    {
                        telnetClient.GetTerminal().DataBag["step"] = "attached";
                        telnetClient.GetTerminal().DataBag.Remove("create_character_name");
                        telnetClient.GetTerminal().DataBag.Remove("create_character_step");
                        s.sendMessageToClient(telnetClient, "\r\nSomething went wrong. Character not created.\r\n\r\n");
                        s.sendMessageToClient(telnetClient, "Account> ");
                    }
                    break;
            }
        }

    }
}
