using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using BeforeOurTime.Repository.Models.Items;
using System.Linq;
using BeforeOurTime.Repository.Models.Items.Attributes;
using Newtonsoft.Json;
using BeforeOurTime.Business.Servers.Telnet.Translate;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Requests.Look;
using BeforeOurTime.Repository.Models.Messages.Events.Emotes;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Business.Apis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BeforeOurTime.Business.Servers.WebSocket
{
    public class WebSocketManager : IServer
    {
        /// <summary>
        /// Before Our Time API
        /// </summary>
        public IApi Api { set; get; }
        /// <summary>
        /// Web socket server
        /// </summary>
        public IWebHost WebSocketServer { set; get; }
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
        /// <param name="serviceProvider"></param>
        public WebSocketManager(IApi api)
        {
            Api = api;
            // MessageHandlers = BuildMessageHandlers();
            // MessageHandlersByType = BuildMessageHandlersByType(MessageHandlers);
            WebSocketServer = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    
                })
                .UseStartup<WebSocketStartup>()
                .Build();
        }
        /// <summary>
        /// Start the server
        /// </summary>
        public void Start()
        {
            WebSocketServer.RunAsync();
            Console.WriteLine("WEBSOCKET SERVER STARTED: " + DateTime.Now);
        }
        /// <summary>
        /// Stop the server
        /// </summary>
        public void Stop()
        {
            WebSocketServer.StopAsync();
            Console.WriteLine("WEBSOCKET SERVER STOPPED: " + DateTime.Now);
        }
    }
    /// <summary>
    /// Kestrel configuration and middleware startup
    /// </summary>
    public class WebSocketStartup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseWebSockets();
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        System.Net.WebSockets.WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await Echo(context, webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }
            });
        }
        /// <summary>
        /// Handle a websocket request
        /// </summary>
        /// <param name="context"></param>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        private async Task Echo(HttpContext context, System.Net.WebSockets.WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(
                    new ArraySegment<byte>(buffer, 0, result.Count), 
                    result.MessageType, 
                    result.EndOfMessage, 
                    CancellationToken.None);
                result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), 
                    CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
