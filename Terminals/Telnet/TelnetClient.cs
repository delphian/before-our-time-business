using BeforeOurTime.Business.Apis.IO.Updates.Models;
using BeforeOurTime.Business.Terminals;
using System;
using System.Collections.Generic;
using System.Net;

namespace BeforeOurTime.Business.Servers.Telnet
{
    public enum EClientStatus
    {
        /// <summary>
        /// The client has not been
        /// authenticated yet.
        /// </summary>
        Guest = 0,
        /// <summary>
        /// The client is authenticating.
        /// </summary>
        Authenticating = 1,
        /// <summary>
        /// The client is logged in.
        /// </summary>
        LoggedIn = 2
    }

    public class TelnetClient
    {
        /// <summary>
        /// The client's identifier.
        /// </summary>
        private uint id;
        /// <summary>
        /// The client's remote address.
        /// </summary>
        private IPEndPoint remoteAddr;
        /// <summary>
        /// The connection datetime.
        /// </summary>
        private DateTime connectedAt;
        /// <summary>
        /// The client's current status.
        /// </summary>
        private EClientStatus status;
        /// <summary>
        /// The last received data from the client.
        /// </summary>
        private string receivedData;
        /// <summary>
        /// Single generic connection used by the environment to communicate with clients
        /// </summary>
        private Terminal Terminal { set; get; }
        /// <summary>
        /// Currently tracked available location exits
        /// </summary>
        public List<IOExitUpdate> ItemExits = new List<IOExitUpdate>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TelnetClient"/> class.
        /// </summary>
        /// <param name="clientId">The client's identifier.</param>
        /// <param name="remoteAddress">The remote address.</param>
        public TelnetClient(uint clientId, IPEndPoint remoteAddress)
        {
            this.id = clientId;
            this.remoteAddr = remoteAddress;
            this.connectedAt = DateTime.Now;
            this.status = EClientStatus.Guest;
            this.receivedData = string.Empty;
        }

        /// <summary>
        /// Gets the client identifier.
        /// </summary>
        /// <returns>Client's identifier.</returns>
        public uint getClientID()
        {
            return id;
        }

        /// <summary>
        /// Gets the remote address.
        /// </summary>
        /// <returns>Client's remote address.</returns>
        public IPEndPoint getRemoteAddress()
        {
            return remoteAddr;
        }

        /// <summary>
        /// Gets the connection time.
        /// </summary>
        /// <returns>The connection time.</returns>
        public DateTime getConnectionTime()
        {
            return connectedAt;
        }

        /// <summary>
        /// Gets the client's current status.
        /// </summary>
        /// <returns>The client's status.</returns>
        public EClientStatus getCurrentStatus()
        {
            return status;
        }

        /// <summary>
        /// Gets the client's last received data.
        /// </summary>
        /// <returns>Client's last received data.</returns>
        public string getReceivedData()
        {
            return receivedData;
        }
        
        /// <summary>
        /// Get the client's associated environment terminal
        /// </summary>
        /// <returns></returns>
        public Terminal GetTerminal()
        {
            return Terminal;
        }
   
        /// <summary>
        /// Sets the client's current status.
        /// </summary>
        /// <param name="newStatus">The new status.</param>
        public void setStatus(EClientStatus newStatus)
        {
            this.status = newStatus;
        }

        /// <summary>
        /// Sets the client's last received data.
        /// </summary>
        /// <param name="newReceivedData">The new received data.</param>
        public void setReceivedData(string newReceivedData)
        {
            this.receivedData = newReceivedData;
        }

        /// <summary>
        /// Sets the client's associated environment terminal
        /// </summary>
        /// <param name="terminal"></param>
        public void SetTerminal(Terminal terminal)
        {
            Terminal = terminal;
        }
        /// <summary>
        /// Appends a string to the client's last
        /// received data.
        /// </summary>
        /// <param name="dataToAppend">The data to append.</param>
        public void appendReceivedData(string dataToAppend)
        {
            this.receivedData += dataToAppend;
        }

        /// <summary>
        /// Removes the last character from the
        /// client's last received data.
        /// </summary>
        public void removeLastCharacterReceived()
        {
            receivedData = receivedData.Substring(0, receivedData.Length - 1);
        }

        /// <summary>
        /// Resets the last received data.
        /// </summary>
        public void resetReceivedData()
        {
            receivedData = string.Empty;
        }

        public override string ToString()
        {
            string ip = string.Format("{0}:{1}", remoteAddr.Address.ToString(), remoteAddr.Port);

            string res = string.Format("Client #{0} (From: {1}, Status: {2}, Connection time: {3})", id, ip, status, connectedAt);

            return res;
        }
    }
}