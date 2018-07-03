using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BeforeOurTime.Business.Servers
{
    /// <summary>
    /// Generic controls for client servers
    /// </summary>
    /// <remarks>
    /// An individual server may facilitate telnet, sockets, signalR, etc...
    /// </remarks>
    public interface IServer
    {
        /// <summary>
        /// Start the server
        /// </summary>
        void Start();
        /// <summary>
        /// Stop the server
        /// </summary>
        Task Stop();
        /// <summary>
        /// Get unique identifiers of all open clients
        /// </summary>
        /// <returns></returns>
        List<Guid> GetClientIds();
    }
}
