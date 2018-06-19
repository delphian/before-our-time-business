﻿using BeforeOurTime.Business.Apis;
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
                IResponse response;
                string responseJson;
                try
                {
                    var message = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(buffer, 0, buffer.Length));
                    var request = (IRequest)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(buffer, 0, buffer.Length),
                        Message.GetMessageTypeDictionary()[message.GetMessageId()]);
                    if (Terminal.Status == TerminalStatus.Guest)
                    {
                        response = HandleMessageFromGuest(request);
                    } else
                    {
                        response = Terminal.SendToApi(request);
                    }
                    responseJson = JsonConvert.SerializeObject(Terminal.SendToApi(request));
                } catch(Exception e)
                {
                    responseJson = e.Message;
                }
                Encoding.UTF8.GetBytes(responseJson, 0, responseJson.Length, buffer, 0);
                await WebSocket.SendAsync(
                    new ArraySegment<byte>(buffer, 0, responseJson.Length),
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
    }
}