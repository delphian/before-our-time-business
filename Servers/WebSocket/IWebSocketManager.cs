using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Servers.WebSocket
{
    public interface IWebSocketManager : IServer
    {
        /// <summary>
        /// Get all open WebSocket clients
        /// </summary>
        /// <returns></returns>
        List<WebSocketClient> GetWebSocketClients();
    }
}
