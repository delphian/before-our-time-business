using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using BeforeOurTime.Business.Servers.Telnet.Translate;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Events.Emotes;
using BeforeOurTime.Models.Messages.Requests.Emote;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Terminals;
using BeforeOurTime.Models.Modules.Account.Messages.CreateAccount;
using BeforeOurTime.Models.Modules.Account.Messages.LoginAccount;
using BeforeOurTime.Models.Modules.World.Messages.Location.ReadLocationSummary;
using BeforeOurTime.Models.Modules.World.Models.Items;
using BeforeOurTime.Models.Modules.Core.Messages.UseItem;
using BeforeOurTime.Models.Logs;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson.ReadItemJson;
using BeforeOurTime.Models.Modules.Core.Messages.ItemGraph;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson.UpdateItemJson;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson;

namespace BeforeOurTime.Business.Servers.Telnet
{
    public class TelnetManager : IServer
    {
        /// <summary>
        /// Dependency injection provider
        /// </summary>
        public IServiceProvider Services { set; get; }
        /// <summary>
        /// IP address to listen on
        /// </summary>
        public IPAddress Address { set; get; }
        /// <summary>
        /// Port to listen on
        /// </summary>
        public int Port { set; get; }
        public TelnetServer TelnetServer { set; get; }
        public Dictionary<uint, string> UserName = new Dictionary<uint, string>();
        public Dictionary<Guid, TelnetClient> Clients = new Dictionary<Guid, TelnetClient>();
        /// <summary>
        /// Classes which will handle a message from terminal
        /// </summary>
        public List<ITranslate> MessageHandlers { set; get; }
        /// <summary>
        /// Organize message handlers by the type of message's they register for
        /// </summary>
        public Dictionary<Type, List<ITranslate>> MessageHandlersByType { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="services">Dependency injection provider</param>
        public TelnetManager(IServiceProvider services, IConfigurationRoot configuration)
        {
            Services = services;
            Address = IPAddress.Parse(configuration.GetSection("Servers").GetSection("Telnet").GetSection("Listen").GetValue<string>("Address"));
            Port = configuration.GetSection("Servers").GetSection("Telnet").GetSection("Listen").GetValue<int>("Port");
            TelnetServer = new TelnetServer(Address, 1024 * 1024);
            TelnetServer.ClientConnected += ClientConnected;
            TelnetServer.ClientDisconnected += ClientDisconnected;
            TelnetServer.ConnectionBlocked += ClientBlocked;
            TelnetServer.MessageReceived += MessageFromClient;
            MessageHandlers = BuildMessageHandlers();
            MessageHandlersByType = BuildMessageHandlersByType(MessageHandlers);
        }
        /// <summary>
        /// Start the server
        /// </summary>
        public void Start()
        {
            TelnetServer.Start(Address, Port);
            Services.GetService<IBotLogger>().LogInformation($"Telnet server started on {TelnetServer.GetIPEndPoint()}");
        }
        /// <summary>
        /// Stop the server
        /// </summary>
        public async Task Stop()
        {
            await Task.Run(() =>
            {
                TelnetServer.Stop();
                Services.GetService<IBotLogger>().LogInformation($"Telnet sever stopped");
            });
        }
        /// <summary>
        /// Assign new telnet client an environment terminal and send greeting
        /// </summary>
        /// <param name="telnetClient"></param>
        private void ClientConnected(TelnetClient telnetClient)
        {
            telnetClient.SetTerminal(Services.GetService<ITerminalManager>().RequestTerminal("Telnet", telnetClient.GetRemoteAddress()));
            telnetClient.GetTerminal().GetDataBag()["step"] = "connected";
            TelnetServer.ClearClientScreen(telnetClient);
            TelnetServer.SendMessageToClient(telnetClient, 
                "Terminal granted " + telnetClient.GetTerminal().GetId() + ".\r\n\r\n"
                + "Welcome to Before Our Time. For help type \"help\".\r\n\r\n"
                + "Welcome> ");
        }
        /// <summary>
        /// Remove terminal from disconnected telnet client
        /// </summary>
        /// <param name="telnetClient"></param>
        private void ClientDisconnected(TelnetClient telnetClient)
        {
            Services.GetService<ITerminalManager>().DestroyTerminal(telnetClient.GetTerminal());
            telnetClient.SetTerminal(null);
        }
        /// <summary>
        /// TODO : Record attempt from blocked ip address
        /// </summary>
        /// <param name="ep"></param>
        private void ClientBlocked(IPEndPoint ep)
        {
        }
        /// <summary>
        /// Use reflection to register all classes which will handle a message from terminal
        /// </summary>
        /// <returns></returns>
        private List<ITranslate> BuildMessageHandlers()
        {
            var interfaceType = typeof(ITranslate);
            var messageHandlers = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => (ITranslate)Activator.CreateInstance(x))
                .ToList();
            return messageHandlers;
        }
        /// <summary>
        /// Organize message handlers by the type of message's they register for
        /// </summary>
        /// <param name="messageHandlers"></param>
        /// <returns></returns>
        private Dictionary<Type, List<ITranslate>> BuildMessageHandlersByType(
            List<ITranslate> messageHandlers)
        {
            var messageHandlersByType = new Dictionary<Type, List<ITranslate>>();
            messageHandlers.ForEach(delegate (ITranslate handler)
            {
                List<Type> handledTypes = handler.RegisterForMessages();
                handledTypes.ForEach(delegate (Type type)
                {
                    if (messageHandlersByType.ContainsKey(type))
                    {
                        messageHandlersByType[type].Add(handler);
                    } else
                    {
                        messageHandlersByType.Add(type, new List<ITranslate>() { handler });
                    }
                });
            });
            return messageHandlersByType;
        }
        /// <summary>
        /// Get unique identifiers of all open clients
        /// </summary>
        /// <returns></returns>
        public List<Guid> GetClientIds()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process a message from the terminal to the telnet client
        /// </summary>
        /// <param name="terminal"></param>
        /// <param name="environmentUpdate"></param>
        private void MessageFromTerminal(ITerminal terminal, IMessage message)
        {
            if (MessageHandlersByType.ContainsKey(message.GetType())) {
                MessageHandlersByType[message.GetType()].ForEach(delegate (ITranslate handler)
                {
                    handler.Translate(message, TelnetServer, Clients[terminal.GetId()]);
                });
            }
            else
            {
                TelnetServer.SendMessageToClient(Clients[terminal.GetId()], "\r\n\r\n"
                    + $"Unknown message from server ({message.GetType().ToString()}):\r\n"
                    + JsonConvert.SerializeObject(message));
            }
            TelnetServer.SendMessageToClient(Clients[terminal.GetId()], "\r\n\r\n> ");
        }
        /// <summary>
        /// Process a message from the telnet client (MFC) to the terminal
        /// </summary>
        /// <param name="telnetClient"></param>
        /// <param name="message"></param>
        private void MessageFromClient(TelnetClient telnetClient, string message)
        {
            if (telnetClient.GetTerminal().GetStatus() == TerminalStatus.Guest)
            {
                MFCTerminalGuest(telnetClient, message);
            }
            else if (telnetClient.GetTerminal().GetStatus() == TerminalStatus.Authenticated)
            {
                MFCTerminalAuthenticated(telnetClient, message);
            }
            else if (telnetClient.GetTerminal().GetStatus() == TerminalStatus.Attached)
            {
                MFCTerminalAttached(telnetClient, message);
            }
        }
        /// <summary>
        /// Handle Message From client when associated terminal is in attached status (playing!)
        /// </summary>
        /// <param name="telnetClient"></param>
        /// <param name="message"></param>
        private void MFCTerminalAttached(TelnetClient telnetClient, string message)
        {
            string firstWord = message.Split(' ').First();
            switch (firstWord)
            {
                case "admin":
                    var words = message.Split(' ');
                    if (message == "admin item graph")
                    {
                        MFCAdminItemTree(telnetClient, message);
                    }
                    if (words[1] == "item" && words[2] == "json" && words[3] == "update")
                    {
                        var json = message.Remove(0, message.IndexOf('{'));
                        MFCAdminItemJsonUpdate(telnetClient, words[4], json);
                    }
                break;
                case "bye":
                case "q":
                case "exit":
                    TelnetServer.SendMessageToClient(telnetClient, "\r\n\r\nCya...\r\n");
                    TelnetServer.KickClient(telnetClient);
                    break;
                case "look":
                    var response = telnetClient.GetTerminal().SendToApi(new WorldReadLocationSummaryRequest()
                        {
                        });
                    telnetClient.GetTerminal().SendToClient(response);
                    break;
                case "go":
                    MFCGo(telnetClient, message);
                    break;
                case "smile":
                case "frown":
                    MFCEmote(telnetClient, message);
                    break;
                case "help":
                    TelnetServer.SendMessageToClient(telnetClient, "\r\n\r\n" +
                        " - Admin item graph\r\n" +
                        " - Admin item json update {guid} {json}" +
                        " - Look\r\n" +
                        " - Bye\r\n" +
                        " - Go {exit name, or partial exit name}\r\n" +
                        " - Smile\r\n" +
                        " - Frown\r\n\r\n> ");
                    break;
                default:
                    TelnetServer.SendMessageToClient(telnetClient, "\r\nBad command. Try \"help\"\r\n> ");
                    break;
            }
        }
        private void MFCAdminItemTree(TelnetClient telnetClient, string message)
        {
            var response = telnetClient.GetTerminal().SendToApi(new CoreReadItemGraphRequest()
            {
                ItemId = new Guid("f4212bfe-ef65-4632-df2b-08d63af92e75")
            });
            telnetClient.GetTerminal().SendToClient(response);
        }
        private void MFCAdminItemJsonUpdate(TelnetClient telnetClient, string itemId, string json)
        {
            var response = telnetClient.GetTerminal().SendToApi(new CoreUpdateItemJsonRequest()
            {
                ItemsJson = new List<CoreItemJson>()
                {
                    new CoreItemJson()
                    {
                        Id = itemId,
                        IncludeChildren = true,
                        JSON = json
                    }
                }
            });
            telnetClient.GetTerminal().SendToClient(response);
        }
        /// <summary>
        /// Handle Message From Client when Go command is issued
        /// </summary>
        /// <param name="telnetClient"></param>
        /// <param name="message"></param>
        private void MFCGo(TelnetClient telnetClient, string message)
        {
            string secondWord = message.Split(' ').LastOrDefault();
            var exitResponse = telnetClient.ItemExits
                .Where(x => x.Name.ToLower()
                .Contains(secondWord.ToLower()))
                .FirstOrDefault();
            if (exitResponse != null)
            {
                var response = telnetClient.GetTerminal().SendToApi(new CoreUseItemRequest()
                    {
                        ItemId = exitResponse.Item.Id
                    });
                telnetClient.GetTerminal().SendToClient(response);
            } else
            {
                TelnetServer.SendMessageToClient(telnetClient, "\r\nGo where??\r\n> ");
            }
        }
        /// <summary>
        /// Handle Message From Client when emote command is issued
        /// </summary>
        /// <param name="telnetClient"></param>
        /// <param name="message"></param>
        private void MFCEmote(TelnetClient telnetClient, string message)
        {
            EmoteType? emoteType = null;
            emoteType = (message == "smile") ? EmoteType.Smile : emoteType;
            emoteType = (message == "frown") ? EmoteType.Frown : emoteType;
            telnetClient.GetTerminal().SendToApi(new EmoteRequest()
            {
                Type = emoteType.Value
            });
        }
        /// <summary>
        /// Handle Message From Client when associated terminal is in guest status
        /// </summary>
        /// <param name="telnetClient">Telnet client</param>
        /// <param name="message">message from client</param>
        private void MFCTerminalGuest(TelnetClient telnetClient, string message)
        {
            if (telnetClient.GetTerminal().GetDataBag()["step"] == "connected")
            {
                switch (message.ToLower())
                {
                    case "?":
                    case "help":
                        TelnetServer.SendMessageToClient(telnetClient, "\r\n\r\n"
                            + "  new    - Create a new account\r\n"
                            + "  login  - Login to an existing account\r\n"
                            + "  bye    - Disconnect\r\n\r\n"
                            + "Welcome> ");
                        break;
                    case "q":
                    case "bye":
                        TelnetServer.SendMessageToClient(telnetClient, "\r\nCya...\r\n\r\n");
                        TelnetServer.KickClient(telnetClient);
                        break;
                    case "new":
                        TelnetServer.SendMessageToClient(telnetClient, "\r\n"
                            + "Name: ");
                        telnetClient.GetTerminal().GetDataBag()["step"] = "create_name";
                        break;
                    case "login":
                        TelnetServer.SendMessageToClient(telnetClient, "\r\n"
                            + "Email: ");
                        telnetClient.GetTerminal().GetDataBag()["step"] = "login_name";
                        break;
                    default:
                        TelnetServer.SendMessageToClient(telnetClient, "\r\n"
                            + "Unknown welcome command \"" + message + "\". Try \"help\".\r\n\r\n"
                            + "Welcome> ");
                        break;
                }
            }
            else if (telnetClient.GetTerminal().GetDataBag()["step"] == "login_name")
            {
                telnetClient.GetTerminal().GetDataBag()["login_name"] = message;
                telnetClient.GetTerminal().GetDataBag()["step"] = "login_password";
                TelnetServer.SendMessageToClient(telnetClient, "\r\n"
                    + "Password: ");
            }
            else if (telnetClient.GetTerminal().GetDataBag()["step"] == "login_password")
            {
                var loginRequest = new AccountLoginAccountRequest()
                {
                    Email = telnetClient.GetTerminal().GetDataBag()["login_name"],
                    Password = message
                };
                AccountLoginAccountResponse loginResponse = (AccountLoginAccountResponse)telnetClient.GetTerminal().SendToApi(loginRequest);
                if (loginResponse.IsSuccess())
                {
                    telnetClient.GetTerminal().Authenticate(loginResponse.Account.Id);
                    telnetClient.GetTerminal().SubscribeMessageToTerminal(MessageFromTerminal);
                    Clients[telnetClient.GetTerminal().GetId()] = telnetClient;
                    telnetClient.GetTerminal().GetDataBag()["step"] = "authenticated";
                    TelnetServer.SendMessageToClient(telnetClient, "\r\n"
                        + "Hello " + telnetClient.GetTerminal().GetDataBag()["login_name"] + "\r\n\r\n"
                        + "Account> ");
                }
                else
                {
                    telnetClient.GetTerminal().GetDataBag()["step"] = "connected";
                    TelnetServer.SendMessageToClient(telnetClient, "\r\n"
                        + "Bad username or password.\r\n\r\n"
                        + "Welcome> ");
                }
            }
            else if (telnetClient.GetTerminal().GetDataBag()["step"] == "create_name")
            {
                telnetClient.GetTerminal().GetDataBag()["create_name"] = message;
                telnetClient.GetTerminal().GetDataBag()["step"] = "create_email";
                TelnetServer.SendMessageToClient(telnetClient, "\r\n"
                    + "Email: ");
            }
            else if (telnetClient.GetTerminal().GetDataBag()["step"] == "create_email")
            {
                telnetClient.GetTerminal().GetDataBag()["create_email"] = message;
                telnetClient.GetTerminal().GetDataBag()["step"] = "create_password";
                TelnetServer.SendMessageToClient(telnetClient, "\r\n"
                    + "Password: ");
            }
            else if (telnetClient.GetTerminal().GetDataBag()["step"] == "create_password")
            {
                var createAccountResponse = telnetClient.GetTerminal().SendToApi(new AccountCreateAccountRequest()
                {
                    Email = telnetClient.GetTerminal().GetDataBag()["create_email"],
                    Password = message
                });
                if (createAccountResponse.IsSuccess())
                {
                    telnetClient.GetTerminal().SubscribeMessageToTerminal(MessageFromTerminal);
                    Clients[telnetClient.GetTerminal().GetId()] = telnetClient;
                    telnetClient.GetTerminal().GetDataBag()["step"] = "authenticated";
                    TelnetServer.SendMessageToClient(telnetClient, "\r\n"
                        + "Hello " + telnetClient.GetTerminal().GetDataBag()["create_name"] + "\r\n\r\n"
                        + "Account> ");
                }
                else
                {
                    TelnetServer.SendMessageToClient(telnetClient, "\r\n"
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
        private void MFCTerminalAuthenticated(TelnetClient telnetClient, string message)
        {
            if (telnetClient.GetTerminal().GetDataBag()["step"] == "create_character")
            {
                MFCTerminalAuthenticatedCreatePlayer(telnetClient, message);
            }
            else if (telnetClient.GetTerminal().GetDataBag()["step"] == "authenticated")
            {
                switch (message.Split(' ').First().ToLower())
                {
                    case "?":
                    case "help":
                        TelnetServer.SendMessageToClient(telnetClient, "\r\n\r\n"
                            + "  new         - Create a new character\r\n"
                            + "  list        - List existing characters\r\n"
                            + "  play {name} - Play an existing character\r\n"
                            + "  back        - Return to previous screen\r\n"
                            + "  bye         - Disconnect\r\n\r\n"
                            + "Account> ");
                        break;
                    case "back":
                        telnetClient.GetTerminal().SetStatus(TerminalStatus.Guest);
                        telnetClient.GetTerminal().GetDataBag()["step"] = "connected";
                        TelnetServer.SendMessageToClient(telnetClient, "\r\nWelcome> ");
                        break;
                    case "new":
                        telnetClient.GetTerminal().GetDataBag()["step"] = "create_character";
                        MFCTerminalAuthenticatedCreatePlayer(telnetClient, message);
                        break;
                    case "list":
                        TelnetServer.SendMessageToClient(telnetClient, "\r\n\r\n");
                        var characters = telnetClient.GetTerminal().GetAttachable();
                        characters.ForEach(delegate (CharacterItem player)
                        {
                            TelnetServer.SendMessageToClient(telnetClient, "  " + 
                                player.Visible.Name + 
                                " (" + player.Id + ")\r\n");
                        });
                        TelnetServer.SendMessageToClient(telnetClient, "\r\n");
                        TelnetServer.SendMessageToClient(telnetClient, "Account> ");
                        break;
                    case "q":
                    case "bye":
                        TelnetServer.SendMessageToClient(telnetClient, "Cya...\r\n");
                        TelnetServer.KickClient(telnetClient);
                        break;
                    case "play":
                        var name = message.Split(' ').Last().ToLower();
                        telnetClient.GetTerminal().GetAttachable().ForEach(delegate (CharacterItem player)
                        {
                            if (player.Visible.Name.ToLower() == name)
                            {
                                telnetClient.GetTerminal().Attach(player.Id);
                                telnetClient.GetTerminal().GetDataBag()["step"] = "attached";
                                TelnetServer.SendMessageToClient(telnetClient, "\r\n"
                                    + "Terminal attached to avatar. Play!\r\n\r\n"
                                    + "> ");
                            }
                        });
                        if (telnetClient.GetTerminal().GetStatus() != TerminalStatus.Attached)
                        {
                            TelnetServer.SendMessageToClient(telnetClient, "\r\n"
                                + "Unknown character \"" + name + "\"\r\n"
                                + "Account> ");
                        }
                        break;
                    default:
                        TelnetServer.SendMessageToClient(telnetClient, "\r\n"
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
        private void MFCTerminalAuthenticatedCreatePlayer(TelnetClient telnetClient, string message)
        {
            if (!telnetClient.GetTerminal().GetDataBag().ContainsKey("create_character_step"))
            {
                telnetClient.GetTerminal().GetDataBag()["create_character_step"] = "create";
            }
            switch (telnetClient.GetTerminal().GetDataBag()["create_character_step"])
            {
                case "create":
                    TelnetServer.SendMessageToClient(telnetClient, "\r\n"
                        + "Ok, let's create a new character.\r\n\r\n"
                        + " Name: ");
                    telnetClient.GetTerminal().GetDataBag()["create_character_step"] = "save_name";
                    break;
                case "save_name":
                    telnetClient.GetTerminal().GetDataBag()["create_character_name"] = message;
                    if (telnetClient.GetTerminal().CreatePlayer(telnetClient.GetTerminal().GetDataBag()["create_character_name"]))
                    {
                        telnetClient.GetTerminal().GetDataBag()["step"] = "attached";
                        TelnetServer.SendMessageToClient(telnetClient, "\r\n"
                            + "Terminal attached to avatar. Play!\r\n\r\n"
                            + "> ");
                    } else
                    {
                        telnetClient.GetTerminal().GetDataBag()["step"] = "attached";
                        telnetClient.GetTerminal().GetDataBag().Remove("create_character_name");
                        telnetClient.GetTerminal().GetDataBag().Remove("create_character_step");
                        TelnetServer.SendMessageToClient(telnetClient, "\r\n"
                            + "Something went wrong. Character not created.\r\n\r\n"
                            + "Account> ");
                    }
                    break;
            }
        }

    }
}
