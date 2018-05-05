﻿using BeforeOurTime.Business.Terminals;
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
            }
            else if (c.GetTerminal().Status == TerminalStatus.Authenticated)
            {
                if (c.GetTerminal().DataBag["step"] == "authenticated")
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
    }
}
