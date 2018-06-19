using BeforeOurTime.Business.Apis;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models.Messages.Requests;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            Api = api;
            Terminal = terminal;
            Context = context;
            WebSocket = webSocket;
        }
        /// <summary>
        /// Handle a websocket request
        /// </summary>
        /// <param name="context"></param>
        /// <param name="webSocket"></param>
        /// <param name="api"></param>
        /// <returns></returns>
        public async Task HandleWebSocket()
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                string response;
                try
                {
                    var requestString = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    var requestConcrete = JsonConvert.DeserializeObject<Request>(requestString);
                    // implement static method Message.GetType(Guid)
                    // concrete discards other properties! Json Convert must have correct subclass
                    IRequest request = (IRequest)requestConcrete;
                    response = JsonConvert.SerializeObject(Terminal.SendToApi(request));
                } catch(Exception e)
                {
                    response = e.Message;
                }
                System.Text.Encoding.UTF8.GetBytes(response, 0, response.Length, buffer, 0);
                await WebSocket.SendAsync(
                    new ArraySegment<byte>(buffer, 0, response.Length),
                    result.MessageType,
                    result.EndOfMessage,
                    CancellationToken.None);
                result = await WebSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None);
            }
            await WebSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            Api.GetTerminalManager().DestroyTerminal(Terminal);
        }
    }
}
