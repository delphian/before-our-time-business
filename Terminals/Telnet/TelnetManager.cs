﻿using BeforeOurTime.Business.Terminals;
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
        public static TelnetServer TelnetServer { set; get; }
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
            TelnetServer = new TelnetServer(IPAddress.Any);
            TelnetServer.ClientConnected += ClientConnected;
            TelnetServer.ClientDisconnected += ClientDisconnected;
            TelnetServer.ConnectionBlocked += ClientBlocked;
            TelnetServer.MessageReceived += MessageFromClient;
            TelnetServer.start();
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
            TelnetServer.clearClientScreen(telnetClient);
            TelnetServer.sendMessageToClient(telnetClient, 
                "Terminal granted " + telnetClient.GetTerminal().Id + ".\r\n\r\n"
                + "Welcome to Before Our Time. For help type \"help\".\r\n\r\n"
                + "Welcome> ");
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
                new TranslateIOLocationUpdate(TelnetServer, Clients[terminal.Id]).Translate(environmentUpdate);
            }
            else
            {
                TelnetServer.sendMessageToClient(Clients[terminal.Id], "\r\n"
                    + "Unknown message from server:\r\n"
                    + JsonConvert.SerializeObject(environmentUpdate));
            }
            TelnetServer.sendMessageToClient(Clients[terminal.Id], "\r\n\r\n> ");
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
            string firstWord = message.Split(' ').First();
            switch (firstWord)
            {
                case "bye":
                case "q":
                case "exit":
                    TelnetServer.sendMessageToClient(telnetClient, "\r\nCya...\r\n");
                    TelnetServer.kickClient(telnetClient);
                    break;
                case "look":
                    telnetClient.GetTerminal().SendToApi(new IOLookRequest()
                    {

                    });
                    break;
                case "go":
                    MFCGo(telnetClient, message);
                    break;
                default:
                    TelnetServer.sendMessageToClient(telnetClient, "\r\nBad command.\r\n> ");
                    break;
            }
        }
        /// <summary>
        /// Handle Message From Client when Go command is issued
        /// </summary>
        /// <param name="telnetClient"></param>
        /// <param name="message"></param>
        private static void MFCGo(TelnetClient telnetClient, string message)
        {
            string secondWord = message.Split(' ').LastOrDefault();
            var exitAttribute = telnetClient.ItemExits.Where(x => x.Name.ToLower().Contains(secondWord.ToLower())).FirstOrDefault();
            if (exitAttribute != null)
            {
                telnetClient.GetTerminal().SendToApi(new IOGoRequest()
                {
                    ExitId = exitAttribute.ExitId
                });
            } else
            {
                TelnetServer.sendMessageToClient(telnetClient, "\r\nGo where??\r\n> ");
            }
        }
        /// <summary>
        /// Handle Message From Client when associated terminal is in guest status
        /// </summary>
        /// <param name="telnetClient">Telnet client</param>
        /// <param name="message">message from client</param>
        private static void MFCTerminalGuest(TelnetClient telnetClient, string message)
        {
            if (telnetClient.GetTerminal().DataBag["step"] == "connected")
            {
                switch (message.ToLower())
                {
                    case "?":
                    case "help":
                        TelnetServer.sendMessageToClient(telnetClient, "\r\n\r\n"
                            + "  new    - Create a new account\r\n"
                            + "  login  - Login to an existing account\r\n"
                            + "  bye    - Disconnect\r\n\r\n"
                            + "Welcome> ");
                        break;
                    case "q":
                    case "bye":
                        TelnetServer.sendMessageToClient(telnetClient, "\r\nCya...\r\n\r\n");
                        TelnetServer.kickClient(telnetClient);
                        break;
                    case "new":
                        TelnetServer.sendMessageToClient(telnetClient, "\r\n"
                            + "Name: ");
                        telnetClient.GetTerminal().DataBag["step"] = "create_name";
                        break;
                    case "login":
                        TelnetServer.sendMessageToClient(telnetClient, "\r\n"
                            + "Email: ");
                        telnetClient.GetTerminal().DataBag["step"] = "login_name";
                        break;
                    default:
                        TelnetServer.sendMessageToClient(telnetClient, "\r\n"
                            + "Unknown welcome command \"" + message + "\". Try \"help\".\r\n\r\n"
                            + "Welcome> ");
                        break;
                }
            }
            else if (telnetClient.GetTerminal().DataBag["step"] == "login_name")
            {
                telnetClient.GetTerminal().DataBag["login_name"] = message;
                telnetClient.GetTerminal().DataBag["step"] = "login_password";
                TelnetServer.sendMessageToClient(telnetClient, "\r\n"
                    + "Password: ");
            }
            else if (telnetClient.GetTerminal().DataBag["step"] == "login_password")
            {
                if (telnetClient.GetTerminal().Authenticate(telnetClient.GetTerminal().DataBag["login_name"], message))
                {
                    telnetClient.GetTerminal().OnMessageToTerminal += MessageFromTerminal;
                    Clients[telnetClient.GetTerminal().Id] = telnetClient;
                    telnetClient.GetTerminal().DataBag["step"] = "authenticated";
                    TelnetServer.sendMessageToClient(telnetClient, "\r\n"
                        + "Hello " + telnetClient.GetTerminal().DataBag["login_name"] + "\r\n\r\n"
                        + "Account> ");
                }
                else
                {
                    telnetClient.GetTerminal().DataBag["step"] = "connected";
                    TelnetServer.sendMessageToClient(telnetClient, "\r\n"
                        + "Bad username or password.\r\n\r\n"
                        + "Welcome> ");
                }
            }
            else if (telnetClient.GetTerminal().DataBag["step"] == "create_name")
            {
                telnetClient.GetTerminal().DataBag["create_name"] = message;
                telnetClient.GetTerminal().DataBag["step"] = "create_email";
                TelnetServer.sendMessageToClient(telnetClient, "\r\n"
                    + "Email: ");
            }
            else if (telnetClient.GetTerminal().DataBag["step"] == "create_email")
            {
                telnetClient.GetTerminal().DataBag["create_email"] = message;
                telnetClient.GetTerminal().DataBag["step"] = "create_password";
                TelnetServer.sendMessageToClient(telnetClient, "\r\n"
                    + "Password: ");
            }
            else if (telnetClient.GetTerminal().DataBag["step"] == "create_password")
            {
                if (telnetClient.GetTerminal().CreateAccount(
                    telnetClient.GetTerminal().DataBag["create_name"],
                    telnetClient.GetTerminal().DataBag["create_email"],
                    message))
                {
                    telnetClient.GetTerminal().OnMessageToTerminal += MessageFromTerminal;
                    Clients[telnetClient.GetTerminal().Id] = telnetClient;
                    telnetClient.GetTerminal().DataBag["step"] = "authenticated";
                    TelnetServer.sendMessageToClient(telnetClient, "\r\n"
                        + "Hello " + telnetClient.GetTerminal().DataBag["create_name"] + "\r\n\r\n"
                        + "Account> ");
                }
                else
                {
                    TelnetServer.sendMessageToClient(telnetClient, "\r\n"
                        + "Unable to create account (but I still like you).\r\n\r\n"
                        + "Welcome> ");
                }
            }
        }
        /// <summary>
        /// Handle Message From Client when associated terminal is in authenticated status
        /// </summary>
        /// <param name="telnetClient">Telnet client</param>
        /// <param name="message">message from client</param>
        private static void MFCTerminalAuthenticated(TelnetClient telnetClient, string message)
        {
            if (telnetClient.GetTerminal().DataBag["step"] == "create_character")
            {
                MFCTerminalAuthenticatedCreatePlayer(telnetClient, message);
            }
            else if (telnetClient.GetTerminal().DataBag["step"] == "authenticated")
            {
                switch (message.Split(' ').First().ToLower())
                {
                    case "?":
                    case "help":
                        TelnetServer.sendMessageToClient(telnetClient, "\r\n\r\n"
                            + "  new         - Create a new character\r\n"
                            + "  list        - List existing characters\r\n"
                            + "  play {name} - Play an existing character\r\n"
                            + "  back        - Return to previous screen\r\n"
                            + "  bye         - Disconnect\r\n\r\n"
                            + "Account> ");
                        break;
                    case "back":
                        telnetClient.GetTerminal().Status = TerminalStatus.Guest;
                        telnetClient.GetTerminal().DataBag["step"] = "connected";
                        TelnetServer.sendMessageToClient(telnetClient, "\r\nWelcome> ");
                        break;
                    case "new":
                        telnetClient.GetTerminal().DataBag["step"] = "create_character";
                        MFCTerminalAuthenticatedCreatePlayer(telnetClient, message);
                        break;
                    case "list":
                        TelnetServer.sendMessageToClient(telnetClient, "\r\n\r\n");
                        var characters = telnetClient.GetTerminal().GetAttachable();
                        characters.ForEach(delegate (AttributePlayer character)
                        {
                            TelnetServer.sendMessageToClient(telnetClient, "  " + character.Name + " (" + character.Id + ")\r\n");
                        });
                        TelnetServer.sendMessageToClient(telnetClient, "\r\n");
                        TelnetServer.sendMessageToClient(telnetClient, "Account> ");
                        break;
                    case "q":
                    case "bye":
                        TelnetServer.sendMessageToClient(telnetClient, "Cya...\r\n");
                        TelnetServer.kickClient(telnetClient);
                        break;
                    case "play":
                        var name = message.Split(' ').Last().ToLower();
                        telnetClient.GetTerminal().GetAttachable().ForEach(delegate (AttributePlayer character)
                        {
                            if (character.Name.ToLower() == name)
                            {
                                if (telnetClient.GetTerminal().Attach(character.Id))
                                {
                                    telnetClient.GetTerminal().DataBag["step"] = "attached";
                                    TelnetServer.sendMessageToClient(telnetClient, "\r\n"
                                        + "Terminal attached to avatar. Play!\r\n\r\n"
                                        + "> ");
                                }
                            }
                        });
                        if (telnetClient.GetTerminal().Status != TerminalStatus.Attached)
                        {
                            TelnetServer.sendMessageToClient(telnetClient, "\r\n"
                                + "Unknown character \"" + name + "\"\r\n"
                                + "Account> ");
                        }
                        break;
                    default:
                        TelnetServer.sendMessageToClient(telnetClient, "\r\n"
                            + "Unknown account command \"" + message + "\". Try \"help\".\r\n\r\n"
                            + "Account> ");
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
                    TelnetServer.sendMessageToClient(telnetClient, "\r\n"
                        + "Ok, let's create a new character.\r\n\r\n"
                        + " Name: ");
                    telnetClient.GetTerminal().DataBag["create_character_step"] = "save_name";
                    break;
                case "save_name":
                    telnetClient.GetTerminal().DataBag["create_character_name"] = message;
                    if (telnetClient.GetTerminal().CreateCharacter(telnetClient.GetTerminal().DataBag["create_character_name"]))
                    {
                        telnetClient.GetTerminal().DataBag["step"] = "attached";
                        TelnetServer.sendMessageToClient(telnetClient, "\r\n"
                            + "Terminal attached to avatar. Play!\r\n\r\n"
                            + "> ");
                    } else
                    {
                        telnetClient.GetTerminal().DataBag["step"] = "attached";
                        telnetClient.GetTerminal().DataBag.Remove("create_character_name");
                        telnetClient.GetTerminal().DataBag.Remove("create_character_step");
                        TelnetServer.sendMessageToClient(telnetClient, "\r\n"
                            + "Something went wrong. Character not created.\r\n\r\n"
                            + "Account> ");
                    }
                    break;
            }
        }

    }
}
