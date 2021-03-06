﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using BeforeOurTime.Business.Servers.Telnet.Translate;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using BeforeOurTime.Models.Logs;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Terminal.Managers;

namespace BeforeOurTime.Business.Servers.WebSocket
{
    public class WebSocketManager : IWebSocketManager
    {
        /// <summary>
        /// Dependency injection provider
        /// </summary>
        public IServiceProvider BotServices { set; get; }
        /// <summary>
        /// IP address to listen on
        /// </summary>
        public IPAddress Address { set; get; }
        /// <summary>
        /// Port to listen on
        /// </summary>
        public int Port { set; get; }
        /// <summary>
        /// Web socket server
        /// </summary>
        public IWebHost WebSocketServer { set; get; }
        /// <summary>
        /// All open WebSocket clients
        /// </summary>
        private List<WebSocketClient> WebSocketClients { set; get; }
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
        public WebSocketManager(IServiceProvider services, IConfigurationRoot configuration)
        {
            WebSocketClients = new List<WebSocketClient>();
            BotServices = services;
            Address = IPAddress.Parse(configuration.GetSection("Servers").GetSection("WebSocket").GetSection("Listen").GetValue<string>("Address"));
            Port = configuration.GetSection("Servers").GetSection("WebSocket").GetSection("Listen").GetValue<int>("Port");
            // MessageHandlers = BuildMessageHandlers();
            // MessageHandlersByType = BuildMessageHandlersByType(MessageHandlers);
            WebSocketServer = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.ListenAnyIP(Port);
                })
                .ConfigureServices(kestrelServices => {
                    kestrelServices.AddSingleton(BotServices.GetService<IConfiguration>());
                    kestrelServices.AddSingleton(BotServices.GetService<IBotLogger>());
                    kestrelServices.AddSingleton(BotServices.GetService<IModuleManager>());
                    kestrelServices.AddSingleton<IWebSocketManager>(this);
                })
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
            BotServices.GetService<IBotLogger>().LogInformation($"Websocket server started on {address}");
        }
        /// <summary>
        /// Stop the server
        /// </summary>
        public async Task Stop()
        {
            BotServices.GetService<IBotLogger>().LogInformation($"Websocket server stopping all clients...");
            WebSocketClients.ForEach(async delegate (WebSocketClient client)
            {
                await client.CloseAsync();
            });
            await WebSocketServer.StopAsync();
            BotServices.GetService<IBotLogger>().LogInformation($"Websocket server stopped");
        }
        /// <summary>
        /// Get all open WebSocket clients
        /// </summary>
        /// <returns></returns>
        public List<WebSocketClient> GetWebSocketClients()
        {
            return WebSocketClients;
        }
        /// <summary>
        /// Get unique identifiers of all open clients
        /// </summary>
        /// <returns></returns>
        public List<Guid> GetClientIds()
        {
            return WebSocketClients.Select(x => x.GetId()).ToList();
        }
    }
    /// <summary>
    /// Kestrel configuration and middleware startup
    /// </summary>
    public class WebSocketStartup
    {
        public void Configure(IApplicationBuilder app)
        {
            var webSocketManager = app.ApplicationServices.GetService<IWebSocketManager>();
            var terminalManager = app.ApplicationServices.GetService<IModuleManager>().GetManager<ITerminalManager>();
            app.UseWebSockets(new WebSocketOptions() {
                ReceiveBufferSize = 1024 * 8
            });
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        System.Net.WebSockets.WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        var terminal = terminalManager.RequestTerminal(
                            "WebSocket", 
                            new IPEndPoint(context.Connection.RemoteIpAddress, context.Connection.RemotePort));
                        var webSocketClient = new WebSocketClient(app.ApplicationServices, terminal, context, webSocket);
                        webSocketManager.GetWebSocketClients().Add(webSocketClient);
                        await webSocketClient.MonitorAsync();
                        await webSocketClient.ListenAsync();
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
