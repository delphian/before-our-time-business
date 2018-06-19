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
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Business.Apis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting.Server.Features;
using BeforeOurTime.Models.Messages.Requests;
using Microsoft.Extensions.Logging;

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
                    options.ListenAnyIP(5000);
                })
                .ConfigureServices(services => { services.AddSingleton<IApi>(Api); })
                .SuppressStatusMessages(true)
                .UseStartup<WebSocketStartup>()
                .Build();
        }
        /// <summary>
        /// Start the server
        /// </summary>
        public void Start()
        {
            WebSocketServer.RunAsync();
            var address = WebSocketServer.ServerFeatures.Get<IServerAddressesFeature>().Addresses.FirstOrDefault();
            Api.GetLogger().LogInformation($"Websocket server started on {address}");
        }
        /// <summary>
        /// Stop the server
        /// </summary>
        public void Stop()
        {
            WebSocketServer.StopAsync();
            Api.GetLogger().LogInformation($"Websocket server stopped");
        }
    }
    /// <summary>
    /// Kestrel configuration and middleware startup
    /// </summary>
    public class WebSocketStartup
    {
        public void Configure(IApplicationBuilder app)
        {
            var api = app.ApplicationServices.GetService<IApi>();
            app.UseWebSockets();
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        System.Net.WebSockets.WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        var terminal = api.GetTerminalManager().RequestTerminal(
                            "WebSocket", 
                            new IPEndPoint(context.Connection.RemoteIpAddress, context.Connection.RemotePort));
                        var webSocketClient = new WebSocketClient(api, terminal, context, webSocket);
                        await webSocketClient.HandleWebSocket();
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
    }
}
