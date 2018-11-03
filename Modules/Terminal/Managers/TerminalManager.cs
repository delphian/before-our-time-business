using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Models.Messages.Responses;
using Microsoft.Extensions.Logging;
using BeforeOurTime.Models.Logs;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Terminal.Models.Data;
using BeforeOurTime.Models.Modules.Account.Managers;
using BeforeOurTime.Models.Modules.Terminal.Models;
using System.Net;
using BeforeOurTime.Models.Modules.Terminal.Managers;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Account.Dbs;

namespace BeforeOurTime.Business.Modules.Terminal.Managers
{
    public partial class TerminalManager : ModelManager<TerminalData>, ITerminalManager
    {
        /// <summary>
        /// Log errors
        /// </summary>
        private IBotLogger Logger { set; get; }
        /// <summary>
        /// Manage all modules
        /// </summary>
        private IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Manage accounts
        /// </summary>
        private IAccountManager AccountManager { set; get; }
        /// <summary>
        /// List of all active terminals
        /// </summary>
        protected List<ITerminal> Terminals = new List<ITerminal>();
        /// <summary>
        /// Definition of function that may subscribe to OnTerminalCreated
        /// </summary>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        public delegate void TerminalCreated(ITerminal terminal);
        /// <summary>
        /// Subscribe to notification after a new terminal has been created
        /// </summary>
        public event TerminalCreated OnTerminalCreated;
        /// <summary>
        /// Definition of function that may subscribe to OnTerminalDestroyed 
        /// </summary>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        public delegate void TerminalDestroyed(ITerminal terminal);
        /// <summary>
        /// Subscribe to notification before a terminal is destroyed
        /// </summary>
        public event TerminalDestroyed OnTerminalDestroyed;
        /// <summary>
        /// Constructor
        /// </summary>
        public TerminalManager(
            IModuleManager moduleManager)
        {
            ModuleManager = moduleManager;
            AccountManager = ModuleManager.GetManager<IAccountManager>();
            Logger = ModuleManager.GetLogger();
        }
        /// <summary>
        /// Get all repositories declared by manager
        /// </summary>
        /// <returns></returns>
        public List<ICrudModelRepository> GetRepositories()
        {
            return new List<ICrudModelRepository>();
        }
        /// <summary>
        /// Get repository as interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetRepository<T>() where T : ICrudModelRepository
        {
            return GetRepositories().Where(x => x is T).Select(x => (T)x).FirstOrDefault();
        }
        /// <summary>
        /// Create a new terminal
        /// </summary>
        /// <param name="serverName">Name of server module</param>
        /// <param name="address">IPAddress of connection</param>
        /// <returns></returns>
        public ITerminal RequestTerminal(string serverName, IPEndPoint address)
        {
            var terminal = new Terminal(this, Logger) as ITerminal;
            terminal.SetStatus(TerminalStatus.Guest);
            Terminals.Add(terminal);
            Logger.LogInformation($"Terminal ({terminal.GetId()}) granted {terminal.GetStatus()} status for {address.ToString()} through {serverName}");
            OnTerminalCreated?.Invoke(terminal);
            return terminal;
        }
        /// <summary>
        /// Destroy a terminal and notify subscribers
        /// </summary>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        public ITerminalManager DestroyTerminal(ITerminal terminal)
        {
            Terminals.Remove(terminal);
            Logger.LogInformation($"Terminal ({terminal.GetId()}) removed");
            OnTerminalDestroyed?.Invoke((ITerminal)terminal.Clone());
            terminal = null;
            return this;
        }
        /// <summary>
        /// Send a message to a specific terminal
        /// </summary>
        /// <param name="terminalId">Unique terminal identifier</param>
        /// <param name="environmentUpdate"></param>
        public void SendToTerminalId(Guid terminalId, IMessage environmentUpdate)
        {
            Terminals.FirstOrDefault(x => x.GetId() == terminalId).SendToClient(environmentUpdate);
        }
        /// <summary>
        /// Get list of all active terminals
        /// </summary>
        /// <returns></returns>
        public List<ITerminal> GetTerminals()
        {
            return Terminals;
        }
        /// <summary>
        /// Instantite response object and wrap request handlers in try catch
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IResponse HandleRequestWrapper<T>(
            IRequest request,
            Action<IResponse> callback) where T : Response, new()
        {
            var response = new T()
            {
                _requestInstanceId = request.GetRequestInstanceId(),
            };
            try
            {
                callback(response);
            }
            catch (Exception e)
            {
                ModuleManager.GetLogger().LogException($"While handling {request.GetMessageName()}", e);
                response._responseSuccess = false;
                response._responseMessage = e.Message;
            }
            return response;
        }
        /// <summary>
        /// Attach terminal data to item
        /// </summary>
        /// <param name="item"></param>
        public void OnItemRead(Item item)
        {
            if (item.Type == ItemType.Character)
            {
                var terminal = GetTerminals().Where(x => x.GetPlayerId() == item.Id).FirstOrDefault();
                if (terminal != null)
                {
                    item.Data.Add(new TerminalData()
                    {
                        TerminalId = terminal.GetId(),
                        Terminal = terminal
                    });
                }
            }
        }
    }
}