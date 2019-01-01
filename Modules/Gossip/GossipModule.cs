using BeforeOurTime.Business.Modules.Terminal.Managers;
using BeforeOurTime.Gossip;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.ItemProperties.Visibles;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Gossip;
using BeforeOurTime.Models.Modules.Terminal.Managers;
using BeforeOurTime.Models.Modules.Terminal.Models;
using BeforeOutTime.Business.Dbs.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Modules.Gossip
{
    public partial class GossipModule : IGossipModule
    {
        /// <summary>
        /// Subscribe to be notified when this module and it's managers have been loaded
        /// </summary>
        public event ModuleReadyDelegate ModuleReadyEvent;
        /// <summary>
        /// Manage all modules
        /// </summary>
        private IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Managers created or required by the module
        /// </summary>
        private List<IModelManager> Managers { set; get; } = new List<IModelManager>();
        /// <summary>
        /// Data repositories created or required by the module
        /// </summary>
        private List<ICrudModelRepository> Repositories { set; get; } = new List<ICrudModelRepository>();
        /// <summary>
        /// Constructor
        /// </summary>
        public GossipModule(
            IModuleManager moduleManager)
        {
            ModuleManager = moduleManager;
            Repositories = Managers.SelectMany(x => x.GetRepositories()).ToList();
        }
        /// <summary>
        /// Get repositories declared by the module
        /// </summary>
        /// <returns></returns>
        public List<ICrudModelRepository> GetRepositories()
        {
            return Repositories;
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
        /// Get item managers declared by the module
        /// </summary>
        /// <returns></returns>
        public List<IModelManager> GetManagers()
        {
            return Managers;
        }
        /// <summary>
        /// Get manager of specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetManager<T>()
        {
            return Managers.Where(x => x is T).Select(x => (T)x).FirstOrDefault();
        }
        /// <summary>
        /// Get message identifiers of messages handled by module
        /// </summary>
        /// <returns></returns>
        public List<Guid> RegisterForMessages()
        {
            return new List<Guid>() { };
        }
        /// <summary>
        /// Initialize module
        /// </summary>
        /// <param name="repositories"></param>
        public void Initialize(List<ICrudModelRepository> repositories)
        {
            var terminalManager = ModuleManager.GetManager<ITerminalManager>();
            var configuration = ModuleManager.GetConfiguration();
            ((TerminalManager)terminalManager).OnTerminalCreated += (ITerminal terminal) =>
            {
                GossipConfig.Members.Add(new GossipMembers()
                {
                    Terminal = terminal
                });
            };
            ((TerminalManager)terminalManager).OnTerminalDestroyed += (ITerminal terminal) =>
            {
                GossipConfig.Members.RemoveAll(x => x.Terminal.GetId() == terminal.GetId());
            };
            if (configuration.GetSection("Gossip") != null)
            {
                var clientId = configuration.GetSection("Gossip").GetValue<string>("ClientId");
                var clientSecret = configuration.GetSection("Gossip").GetValue<string>("ClientSecret");
                var gossipConfig = new GossipConfig()
                {
                    GossipActive = true,
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    ClientName = "Before Our Time",
                    UserAgent = "bot-0.2.16",
                    SuspendMultiplier = 2,
                    SuspendMultiplierMaximum = 4,
                    SupportedChannels = new HashSet<string>() { "testing" },
                    SupportedFeatures = new HashSet<string>() { "channels" }
                };
                var gossipClient = new GossipClient(
                    gossipConfig,
                    (Exception e) => {
                        Console.WriteLine($"EXCEPTION: {e.Message}");
                    },
                    (string message) => {
                        Console.WriteLine($"LOG: {message}");
                    },
                    () =>
                    {
                        GossipConfig.Members.ForEach((gossipMember) =>
                        {
                            if (gossipMember.Terminal.GetPlayerId() != null && gossipMember.Member == null)
                            {
                                var item = ModuleManager
                                    .GetManager<IItemManager>()
                                    .Read(gossipMember.Terminal.GetPlayerId().Value);
                                gossipMember.Member = new Member()
                                {
                                    Name = item.GetProperty<VisibleItemProperty>().Name,
                                    WriteTo = (message) =>
                                    {
                                        Console.WriteLine($"GOSSIP: {message}\n");
                                    }
                                };
                            }
                        });
                        var members = GossipConfig.Members.Where(x => x.Terminal.GetPlayerId() != null)
                            .Select(x => x.Member).ToArray<Member>();
                        return members;
                    }
                );
                gossipClient.Launch();
            }
        }
        /// <summary>
        /// Get module's self assigned order. 
        /// </summary>
        /// <remarks>
        /// Lower numbers execute first, therefore a higher module order
        /// allows for previous module loaded values to be altered.
        /// </remarks>
        /// <returns></returns>
        public int GetOrder()
        {
            return 300;
        }
        #region Message Handlers
        /// <summary>
        /// Handle a message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="origin">Item that initiated request</param>
        /// <param name="mm">Module manager</param>
        /// <param name="response"></param>
        public IResponse HandleMessage(
            IMessage message,
            Item origin,
            IModuleManager mm,
            IResponse response)
        {
            return response;
        }
        #endregion
    }
    public class GossipConfig : IConfig
    {
        public static List<GossipMembers> Members { set; get; } = new List<GossipMembers>();
        public bool GossipActive { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ClientName { get; set; }
        public string UserAgent { get; set; }
        public double SuspendMultiplierMaximum { get; set; }
        public double SuspendMultiplier { get; set; }
        public HashSet<string> SupportedChannels { get; set; }
        public HashSet<string> SupportedFeatures { get; set; }
    }
    public class GossipMembers
    {
        public Member Member { set; get; }
        public ITerminal Terminal { set; get; }
        public Item Item { set; get; }
    }
}
