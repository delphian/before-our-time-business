using BeforeOurTime.Business.Apis;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Responses;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using BeforeOurTime.Models.Messages.Systems.Ping;
using BeforeOurTime.Models.Terminals;
using Microsoft.Extensions.Configuration;
using BeforeOurTime.Models.Logs;
using BeforeOurTime.Business.Apis.Terminals;

namespace BeforeOurTime.Business.Servers.WebSocket
{
    /// <summary>
    /// Control a single websocket client
    /// </summary>
    public class WebSocketClient
    {
        private IServiceProvider Services { set; get; }
        private IBotLogger Logger { set; get; }
        private ITerminalManager TerminalManager { set; get; }
        private int LogLevel { set; get; }
        /// <summary>
        /// Unique WebSocket client identifier
        /// </summary>
        private Guid Id { set; get; }
        /// <summary>
        /// Single generic connection used by the environment to communicate with clients
        /// </summary>
        private ITerminal Terminal { set; get; }
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

            IServiceProvider services, 
            ITerminal terminal,
            HttpContext context,
            System.Net.WebSockets.WebSocket webSocket)
        {
            Id = Guid.NewGuid();
            Services = services;
            Logger = Services.GetService<IBotLogger>();
            TerminalManager = Services.GetService<ITerminalManager>();
            LogLevel = Convert.ToInt32(Services.GetService<IConfiguration>().GetSection("Servers").GetSection("WebSocket")["LogLevel"]);
            Terminal = terminal;
            Context = context;
            WebSocket = webSocket;
            Cts = new CancellationTokenSource();
            Terminal.SubscribeMessageToTerminal(OnMessageFromServer);
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
                    await Task.Delay(60000 * 2);
                    var timeoutTask = Task.Delay(10000);
                    var pingTask = SendAsync(new PingSystemMessage() { }, Cts.Token);
                    Logger.LogDebug($"Client {Id} websocket: {WebSocket.State.ToString()}");
                    if (await Task.WhenAny(pingTask, timeoutTask) == timeoutTask || pingTask.Status == TaskStatus.Faulted)
                    {
                        if (pingTask.Exception?.InnerException != null)
                        {
                            Logger.LogWarning($"Client {Id} {pingTask.Exception.InnerException.Message}");
                        }
                        else
                        {
                            Logger.LogWarning($"Client {Id} remote client is not responding!");
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
            Logger.LogInformation($"Client {Id} Listening to {Context.Connection.RemoteIpAddress}:{Context.Connection.RemotePort}...");
            var buffer = new Byte[1024 * 64];
            WebSocketReceiveResult result = null;
            // Listen to websocket
            while (WebSocket.State == WebSocketState.Open)
            {
                try
                {
                    // Wait for next incoming message
                    Array.Clear(buffer, 0, buffer.Length);
                    result = await WebSocket.ReceiveAsync(buffer, Cts.Token);
                    var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    while (!result.EndOfMessage)
                    {
                        Array.Clear(buffer, 0, buffer.Length);
                        result = await WebSocket.ReceiveAsync(buffer, Cts.Token);
                        messageJson += Encoding.UTF8.GetString(buffer, 0, result.Count);
                    }
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
                        var message = JsonConvert.DeserializeObject<Message>(messageJson);
//                        Logger.LogInformation($"<< {Id} {message.GetMessageName()}");
//                        Logger.LogDebug($"<< {Id} {messageJson}");
                        var request = (IRequest)JsonConvert.DeserializeObject(messageJson, Message.GetMessageTypeDictionary()[message.GetMessageId()]);
                        response = Terminal.SendToApi(request);
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
                    Logger.LogError($"Client {Id} while recieving data: {message}");
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
            if (WebSocket != null)
            {
                Logger.LogInformation($"Client {Id} closing connection...");
                var disconnectTask = WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                var timeoutTask = Task.Delay(10000);
                if (await Task.WhenAny(disconnectTask, timeoutTask) == timeoutTask || disconnectTask.Status == TaskStatus.Faulted)
                {
                    Logger.LogWarning($"Client {Id} killing websocket!");
                    Cts.Cancel();
                    while (WebSocket.State == WebSocketState.Open)
                    {
                        await Task.Delay(5000);
                        Logger.LogInformation($"Client {Id} waiting for websocket to die...");
                    }
                }
                Cts.Cancel();
                WebSocket.Dispose();
                WebSocket = null;
                Cts.Dispose();
                Logger.LogInformation($"Client {Id} connection closed");
                TerminalManager.DestroyTerminal(Terminal);
            }
        }
        /// <summary>
        /// Send a message
        /// </summary>
        /// <param name="message">All intra-item, environment and terminal communications are in the form of Message</param>
        /// <param name="cancelToken">Notify method that operation should be canceled</param>
        /// <returns></returns>
        public async Task SendAsync(IMessage message, CancellationToken cancelToken)
        {
            try
            {
                var messageJson = JsonConvert.SerializeObject(message);
                var byteMessage = new UTF8Encoding(false, true).GetBytes(messageJson);
                var offset = 0;
                var endOfMessage = false;
//                Logger.LogInformation($">> {Id} {message.GetMessageName()}");
//                Logger.LogDebug($">> {Id} {messageJson}");
                do
                {
                    var remainingBytes = byteMessage.Count() - (offset * 1024);
                    var sendBytes = Math.Min(1024, remainingBytes);
                    var segment = new ArraySegment<byte>(byteMessage, (offset++ * 1024), sendBytes);
                    endOfMessage = remainingBytes == sendBytes;
                    await WebSocket.SendAsync(segment, WebSocketMessageType.Text, endOfMessage, cancelToken);
                } while (!endOfMessage);
            }
            catch (Exception e)
            {
                Exception traverse = e;
                string error = "";
                while (traverse != null)
                {
                    error += $"({traverse.Message})";
                    traverse = traverse.InnerException;
                }
                Logger.LogError($"Client {Id} while sending data: {message.GetMessageName()}: {error}");
            }
        }
        /// <summary>
        /// Listen to incoming messages from server
        /// </summary>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        /// <param name="message">All intra-item, environment and terminal communications are in the form of Message</param>
        public async void OnMessageFromServer(ITerminal terminal, IMessage message)
        {
            await SendAsync(message, CancellationToken.None);
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
