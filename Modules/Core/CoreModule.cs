using BeforeOurTime.Business.Apis;
using BeforeOurTime.Business.Apis.Logs;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Business.Models;
using BeforeOurTime.Business.Modules.Core.Dbs;
using BeforeOurTime.Business.Modules.Core.Dbs.EF;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.ItemAttributes;
using BeforeOurTime.Models.ItemAttributes.Locations;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Items.Games;
using BeforeOurTime.Models.Items.Locations;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.CRUD.Items.CreateItem;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Core;
using BeforeOurTime.Models.Modules.Core.Dbs;
using BeforeOurTime.Models.Modules.Core.Messages.ReadItemJson;
using BeforeOurTime.Models.Modules.Core.Models.Data;
using BeforeOurTime.Models.Terminals;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Modules.Core
{
    public class CoreModule : ICoreModule
    {
        /// <summary>
        /// Entity framework database context
        /// </summary>
        private EFCoreModuleContext Db { set; get; }
        /// <summary>
        /// System configuration
        /// </summary>
        private IConfiguration Configuration { set; get; }
        /// <summary>
        /// Centralized log messages
        /// </summary>
        private ILogger Logger { set; get; }
        /// <summary>
        /// Access to items in the data store
        /// </summary>
        private IItemRepo ItemRepo { set; get; }
        /// <summary>
        /// Access to Game Data in the data store
        /// </summary>
        private IGameDataRepo GameDataRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemRepo">Access to items in the data store</param>
        public CoreModule(
            IConfiguration configuration,
            ILogger logger,
            IItemRepo itemRepo)
        {
            Configuration = configuration;
            Logger = logger;
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var dbOptions = new DbContextOptionsBuilder<EFCoreModuleContext>();
                dbOptions.UseSqlServer(connectionString);
                dbOptions.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            Db = new EFCoreModuleContext(dbOptions.Options);
            ItemRepo = itemRepo;
            ItemRepo.OnItemCreate += OnItemCreate;
            ItemRepo.OnItemRead += OnItemRead;
            ItemRepo.OnItemUpdate += OnItemUpdate;
            ItemRepo.OnItemDelete += OnItemDelete;
            GameDataRepo = new EFGameDataRepo(Db, ItemRepo);
        }
        /// <summary>
        /// Get repositories declared by the module
        /// </summary>
        /// <returns></returns>
        public List<ICrudDataRepository> GetRepositories()
        {
            var repositories = new List<ICrudDataRepository>()
            {
                GameDataRepo
            };
            return repositories;
        }
        /// <summary>
        /// Get message identifiers of messages handled by module
        /// </summary>
        /// <returns></returns>
        public List<Guid> RegisterForMessages()
        {
            return new List<Guid>()
            {
                CoreReadItemJsonRequest._Id
            };
        }
        /// <summary>
        /// Initialize module
        /// </summary>
        /// <param name="repositories"></param>
        public void Initialize(List<ICrudDataRepository> repositories)
        {
            GameDataRepo = (IGameDataRepo)repositories.Where(x => x is IGameDataRepo).FirstOrDefault();
        }
        /// <summary>
        /// Get the default game
        /// </summary>
        /// <remarks>
        /// Will create the default game and a location if one does not already exist
        /// </remarks>
        /// <returns></returns>
        public Item GetDefaultGame()
        {
            var defaultGameData = GameDataRepo.Read().FirstOrDefault();
            if (defaultGameData == null)
            {
                var gameItem = ItemRepo.Create(new GameItem()
                {
                    ParentId = null,
                    Data = new List<IItemData>()
                    {
                        new GameData()
                        {
                            Name = "Brave New World",
                            DefaultLocationId = null
                        }
                    }
                });
                var locationItem = ItemRepo.Create(new LocationItem()
                {
                    ParentId = gameItem.Id,
                    Attributes = new List<ItemAttribute>()
                    {
                        new LocationAttribute()
                        {
                            Name = "A Dark Void",
                            Description = "Cool mists and dark shadows shroud "
                                + "everything in this place. Straining your eyes does little to resolve the "
                                + "amorphous blobs that are circulating about. The oppresive silence is occationaly "
                                + "puncuated by a distressed weeping or soft sob. A chill runs through your blood "
                                + "when you realise these forms may have once been human. The smell of rain "
                                + "and rotting wood pains your nose while the occational drip of water tickles "
                                + "the top of skulls both real and imagined. Any attempt to navigate in this damp "
                                + "cavern causes disorientation.",
                        }
                    }
                });
                defaultGameData = gameItem.GetData<GameData>();
                defaultGameData.DefaultLocationId = locationItem.GetAttribute<LocationAttribute>().ItemId;
                GameDataRepo.Update(defaultGameData);
            }
            var defaultGameItem = ItemRepo.Read(defaultGameData.DataItemId);
            return defaultGameItem;
        }
        /// <summary>
        /// Get default game location
        /// </summary>
        /// <returns></returns>
        public Item GetDefaultLocation()
        {
            Item defaultLocationItem = null;
            var game = GetDefaultGame();
            if (game.GetData<GameData>().DefaultLocationId != null)
            {
                defaultLocationItem = ItemRepo.Read(game.GetData<GameData>().DefaultLocationId.Value);
            }
            return defaultLocationItem;
        }
        #region On Item Hooks
        /// <summary>
        /// Create attribute, if present, after item is created
        /// </summary>
        /// <param name="item">Base item just created from datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemCreate(Item item, TransactionOptions options = null)
        {
            if (item.HasData<GameData>())
            {
                var data = item.GetData<GameData>();
                data.DataItemId = item.Id;
                GameDataRepo.Create(data, options);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item just read from datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemRead(Item item, TransactionOptions options = null)
        {
            var data = GameDataRepo.Read(item, options);
            if (data != null)
            {
                item.Data.Add(data);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item about to be persisted to datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemUpdate(Item item, TransactionOptions options = null)
        {
            if (item.HasData<GameData>())
            {
                var data = item.GetData<GameData>();
                GameDataRepo.Update(data, options);
            }
        }
        /// <summary>
        /// Delete attribute of base item before base item is deleted
        /// </summary>
        /// <param name="item">Base item about to be deleted</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemDelete(Item item, TransactionOptions options = null)
        {
            if (item.HasAttribute<GameData>())
            {
                var data = item.GetData<GameData>();
                GameDataRepo.Delete(data, options);
            }
        }
        #endregion
        #region Message Handlers
        /// <summary>
        /// Handle a message
        /// </summary>
        /// <param name="api"></param>
        /// <param name="message"></param>
        /// <param name="terminal"></param>
        /// <param name="response"></param>
        public IResponse HandleMessage(IMessage message, IApi api, ITerminal terminal, IResponse response)
        {
            if (message.GetMessageId() == CoreReadItemJsonRequest._Id)
            {
                response = HandleCoreReadItemJsonRequest(message, api, terminal, response);
            }
            return response;
        }
        /// <summary>
        /// Handle a message
        /// </summary>
        /// <param name="api"></param>
        /// <param name="message"></param>
        /// <param name="terminal"></param>
        /// <param name="response"></param>
        private IResponse HandleCoreReadItemJsonRequest(IMessage message, IApi api, ITerminal terminal, IResponse response)
        {
            var request = message.GetMessageAsType<CoreReadItemJsonRequest>();
            response = new CoreReadItemJsonResponse()
            {
                _requestInstanceId = request.GetRequestInstanceId(),
            };
            try
            {
                var player = api.GetItemManager().Read(terminal.GetPlayerId().Value);
                var coreItemsJson = new List<CoreItemJson>();
                // Read enumerated list of items
                if (request.ItemIds != null)
                {
                    var items = api.GetItemManager().Read(request.ItemIds);
                    items.ForEach(item =>
                    {
                        coreItemsJson.Add(new CoreItemJson()
                        {
                            Id = item.Id.ToString(),
                            JSON = JsonConvert.SerializeObject(item, Formatting.Indented)
                        });
                    });
                }
                ((CoreReadItemJsonResponse)response)._responseSuccess = true;
                ((CoreReadItemJsonResponse)response).CoreReadItemJsonEvent = new CoreReadItemJsonEvent()
                {
                    ItemsJson = coreItemsJson
                };
            }
            catch (Exception e)
            {
                ((FileLogger)Logger).LogException($"While handling {request.GetMessageName()}", e);
                ((CoreReadItemJsonResponse)response)._responseMessage = e.Message;
            }
            return response;
        }
        #endregion
    }
}
