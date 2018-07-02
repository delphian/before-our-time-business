using BeforeOurTime.Business.Apis;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Requests.Login;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Messages.Responses.Login;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BeforeOurTime.Models.Messages.Requests.List;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Attributes;
using System.Linq;
using BeforeOurTime.Models.Messages.Systems.Ping;

namespace BeforeOurTime.Business.Servers.WebSocket
{
    /// <summary>
    /// Control a single websocket client
    /// </summary>
    public class WebSocketClient
    {
        /// <summary>
        /// Before Our Time API
        /// </summary>
        private IApi Api { set; get; }
        /// <summary>
        /// Unique WebSocket client identifier
        /// </summary>
        private Guid Id { set; get; }
        /// <summary>
        /// Single generic connection used by the environment to communicate with clients
        /// </summary>
        private Terminal Terminal { set; get; }
        /// <summary>
        /// HTTP Context at time websocket was established
        /// </summary>
        private HttpContext Context { set; get; }
        /// <summary>
        /// Websocket connection of client
        /// </summary>
        private System.Net.WebSockets.WebSocket WebSocket { set; get; }
        /// <summary>
        /// Cancelation token for recieving loop
        /// </summary>
        private CancellationTokenSource Cts { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="api">Before Our Time API</param>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        public WebSocketClient(
            IApi api, 
            Terminal terminal,
            HttpContext context,
            System.Net.WebSockets.WebSocket webSocket)
        {
            Id = Guid.NewGuid();
            Api = api;
            Terminal = terminal;
            Context = context;
            WebSocket = webSocket;
            Cts = new CancellationTokenSource();
            Terminal.OnMessageToTerminal += OnMessageFromServer;
        }
        /// <summary>
        /// Monitor websocket health
        /// </summary>
        /// <returns></returns>
        public async Task MonitorAsync()
        {
            await Task.Factory.StartNew(async () =>
            {
                while (WebSocket.State == WebSocketState.Open)
                {
                    await Task.Delay(60000);
                    var timeoutTask = Task.Delay(10000);
                    var pingTask = SendAsync(new PingSystemMessage() { }, Cts.Token);
                    Api.GetLogger().LogDebug($"Client {Id} websocket: {WebSocket.State.ToString()}");
                    if (await Task.WhenAny(pingTask, timeoutTask) == timeoutTask || pingTask.Status == TaskStatus.Faulted)
                    {
                        if (pingTask.Exception?.InnerException != null)
                        {
                            Api.GetLogger().LogWarning($"Client {Id} {pingTask.Exception.InnerException.Message}");
                        }
                        else
                        {
                            Api.GetLogger().LogWarning($"Client {Id} remote client is not responding!");
                        }
                        await CloseAsync();
                    }
                }
            }, Cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        /// <summary>
        /// Listen to websocket
        /// </summary>
        /// <returns></returns>
        public async Task ListenAsync()
        {
            Api.GetLogger().LogInformation($"Client {Id} Listening to {Context.Connection.RemoteIpAddress}:{Context.Connection.RemotePort}...");
            var buffer = new Byte[1024 * 4];
            WebSocketReceiveResult result = null;
            // Listen to websocket
            while (WebSocket.State == WebSocketState.Open)
            {
                try
                {
                    // Wait for next incoming message
                    Array.Clear(buffer, 0, buffer.Length);
                    result = await WebSocket.ReceiveAsync(buffer, Cts.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await CloseAsync();
                    }
                    else if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        await CloseAsync();
                    }
                    else
                    {
                        IResponse response;
                        var messageJson = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                        Api.GetLogger().LogInformation($"Client {Id} request: {messageJson}");
                        var message = JsonConvert.DeserializeObject<Message>(messageJson);
                        var request = (IRequest)JsonConvert.DeserializeObject(messageJson, Message.GetMessageTypeDictionary()[message.GetMessageId()]);
                        if (Terminal.Status == TerminalStatus.Guest)
                        {
                            response = HandleMessageFromGuest(request);
                        }
                        else if (Terminal.Status == TerminalStatus.Authenticated)
                        {
                            response = HandleMessageFromAuthenticated(request);
                        }
                        else
                        {
                            response = Terminal.SendToApi(request);
                        }
                        // Send response
                        await SendAsync(response, Cts.Token);
                    }
                }
                catch (Exception e)
                {
                    Exception traverse = e;
                    string message = "";
                    while (traverse != null)
                    {
                        message += $"({traverse.Message})";
                        traverse = traverse.InnerException;
                    }
                    Api.GetLogger().LogError($"Client {Id} while recieving data: {message}");
                    await CloseAsync();
                }
            }
        }
        /// <summary>
        /// Close the websocket connection
        /// </summary>
        /// <returns></returns>
        public async Task CloseAsync()
        {
            Api.GetLogger().LogInformation($"Client {Id} closing connection...");
            var disconnectTask = WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            var timeoutTask = Task.Delay(10000);
            if (await Task.WhenAny(disconnectTask, timeoutTask) == timeoutTask || disconnectTask.Status == TaskStatus.Faulted)
            {
                Api.GetLogger().LogWarning($"Client {Id} killing websocket!");
                Cts.Cancel();
                while (WebSocket.State == WebSocketState.Open)
                {
                    await Task.Delay(5000);
                    Api.GetLogger().LogInformation($"Client {Id} waiting for websocket to die...");
                }
            }
            Cts.Cancel();
            WebSocket.Dispose();
            Cts.Dispose();
            Api.GetLogger().LogInformation($"Client {Id} connection closed");
            Api.GetTerminalManager().DestroyTerminal(Terminal);
        }
        /// <summary>
        /// Send a message
        /// </summary>
        /// <param name="message">All intra-item, environment and terminal communications are in the form of Message</param>
        /// <param name="cancelToken">Notify method that operation should be canceled</param>
        /// <returns></returns>
        public async Task SendAsync(IMessage message, CancellationToken cancelToken)
        {
            var buffer = new byte[1024 * 4];
            var messageJson = JsonConvert.SerializeObject(message);
            Api.GetLogger().LogInformation($"Client {Id} to client: {messageJson}");
            Encoding.UTF8.GetBytes(messageJson, 0, messageJson.Length, buffer, 0);
            await WebSocket.SendAsync(
                new ArraySegment<byte>(buffer, 0, messageJson.Length),
                WebSocketMessageType.Text,
                true,
                cancelToken);
        }
        /// <summary>
        /// Listen to incoming messages from server
        /// </summary>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        /// <param name="message">All intra-item, environment and terminal communications are in the form of Message</param>
        public async void OnMessageFromServer(Terminal terminal, IMessage message)
        {
            await SendAsync(message, CancellationToken.None);
        }
        /// <summary>
        /// Handle all messages from terminals with guest status
        /// </summary>
        /// <param name="request"></param>
        public IResponse HandleMessageFromGuest(IRequest request)
        {
            IResponse response = new Response()
            {
                ResponseSuccess = false
            };
            if (request.IsMessageType<LoginRequest>()) {
                response = Terminal.SendToApi(request);
                var loginResponse = response.GetMessageAsType<LoginResponse>();
                if (loginResponse.IsSuccess())
                {
                    Terminal.Authenticate(loginResponse.AccountId.Value);
                }
            }
            return response;
        }
        /// <summary>
        /// Handle all messages from terminals with authenticated status
        /// </summary>
        /// <param name="request"></param>
        public IResponse HandleMessageFromAuthenticated(IRequest request)
        {
            IResponse response = new Response()
            {
                ResponseSuccess = false
            };
            if (request.IsMessageType<LogoutRequest>())
            {
                response = Terminal.SendToApi(request);
                var logoutResponse = response.GetMessageAsType<LogoutResponse>();
                if (logoutResponse.IsSuccess())
                {
                    Terminal.Guest();
                }
            }
            if (request.IsMessageType<ListAccountCharactersRequest>())
            {
                response = Terminal.SendToApi(request);
            }
            if (request.IsMessageType<LoginAccountCharacterRequest>())
            {
                var loginAccountCharacterRequest = request.GetMessageAsType<LoginAccountCharacterRequest>();
                var character = Terminal.GetAttachable().Where(x => x.Id == loginAccountCharacterRequest.ItemId).FirstOrDefault();
                if (character != null && Terminal.Attach(character.Id))
                {
                    Terminal.Attach(character.Id);
                    response = new LoginAccountCharacterResponse()
                    {
                        ResponseSuccess = true
                    };
                } else
                {
                    response = new LoginAccountCharacterResponse()
                    {
                        ResponseSuccess = false
                    };
                }
            }
            return response;
        }
        /// <summary>
        /// Get unique client identifier
        /// </summary>
        /// <returns></returns>
        public Guid GetId()
        {
            return Id;
        }
    }
}
