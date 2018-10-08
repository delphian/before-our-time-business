using BeforeOurTime.Business.Apis;
using BeforeOurTime.Business.Apis.Items;
using BeforeOurTime.Business.Apis.Logs;
using BeforeOurTime.Business.Modules.Core.Dbs.EF;
using BeforeOurTime.Business.Modules.Core.Managers;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Logs;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Core;
using BeforeOurTime.Models.Modules.Core.Dbs;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.Core.Messages.ItemCrud.DeleteItem;
using BeforeOurTime.Models.Modules.Core.Messages.ItemCrud.ReadItem;
using BeforeOurTime.Models.Modules.Core.Messages.ItemCrud.UpdateItem;
using BeforeOurTime.Models.Modules.Core.Messages.ItemGraph;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson.CreateItemJson;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson.ReadItemJson;
using BeforeOurTime.Models.Modules.Core.Messages.ItemJson.UpdateItemJson;
using BeforeOurTime.Models.Modules.Core.Models.Data;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Core.Models.Properties;
using BeforeOurTime.Models.Terminals;
using BeforeOurTime.ModelsModels.Modules.Core.Messages.ItemCrud.CreateItem;
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
    public partial class CoreModule : ICoreModule
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
        private IBotLogger Logger { set; get; }
        /// <summary>
        /// Access to items in the data store
        /// </summary>
        private IItemRepo ItemRepo { set; get; }
        /// <summary>
        /// Managers created or required by the module
        /// </summary>
        private List<IModelManager> Managers { set; get; } = new List<IModelManager>();
        /// <summary>
        /// Data repositories created or required by the module
        /// </summary>
        private List<ICrudModelRepository> Repositories { set; get; } = new List<ICrudModelRepository>();
        /// <summary>
        /// Game data repository
        /// </summary>
        private IGameDataRepo GameDataRepo { set; get; }
        /// <summary>
        /// Location data repository
        /// </summary>
        private ILocationDataRepo LocationDataRepo { set; get; }
        /// <summary>
        /// Character data repository
        /// </summary>
        private ICharacterDataRepo CharacterDataRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemRepo">Access to items in the data store</param>
        public CoreModule(
            IConfiguration configuration,
            IBotLogger logger,
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
            Managers = BuildManagers(Logger, Db, ItemRepo);
            Repositories = Managers.SelectMany(x => x.GetRepositories()).ToList();
            ItemRepo.OnItemCreate += OnItemCreate;
            ItemRepo.OnItemRead += OnItemRead;
            ItemRepo.OnItemUpdate += OnItemUpdate;
            ItemRepo.OnItemDelete += OnItemDelete;
        }
        /// <summary>
        /// Build all the item managers for the module
        /// </summary>
        /// <param name="db"></param>
        /// <param name="itemRepo"></param>
        /// <returns></returns>
        List<IModelManager> BuildManagers(IBotLogger logger, EFCoreModuleContext db, IItemRepo itemRepo)
        {
            var managers = new List<IModelManager>
            {
                new GameItemManager(logger, itemRepo, new EFGameDataRepo(Db, itemRepo)),
                new LocationItemManager(logger, itemRepo, new EFLocationDataRepo(Db, itemRepo)),
                new CharacterItemManager(logger, itemRepo, new EFCharacterDataRepo(db, itemRepo))
            };
            return managers;
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
            return new List<Guid>()
            {
                CoreReadItemGraphRequest._Id,
                CoreReadItemJsonRequest._Id,
                CoreUpdateItemJsonRequest._Id,
                CoreCreateItemJsonRequest._Id,
                CoreCreateItemCrudRequest._Id,
                CoreReadItemCrudRequest._Id,
                CoreUpdateItemCrudRequest._Id,
                CoreDeleteItemCrudRequest._Id
            };
        }
        /// <summary>
        /// Initialize module
        /// </summary>
        /// <param name="repositories"></param>
        public void Initialize(List<ICrudModelRepository> repositories)
        {
            GameDataRepo = repositories
                .Where(x => x is IGameDataRepo)
                .Select(x => (IGameDataRepo)x).FirstOrDefault();
            LocationDataRepo = repositories
                .Where(x => x is ILocationDataRepo)
                .Select(x => (ILocationDataRepo)x).FirstOrDefault();
            CharacterDataRepo = repositories
                .Where(x => x is ICharacterDataRepo)
                .Select(x => (ICharacterDataRepo)x).FirstOrDefault();
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
                    Data = new List<IItemData>()
                    {
                        new LocationData()
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
                defaultGameData.DefaultLocationId = locationItem.GetData<LocationData>().DataItemId;
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
            Managers.Where(x => x is IItemModelManager).ToList().ForEach(manager =>
            {
                ((IItemModelManager)manager).OnItemCreate(item, options);
            });
            if (item.HasData<LocationData>())
            {
                var data = item.GetData<LocationData>();
                data.DataItemId = item.Id;
                LocationDataRepo.Create(data, options);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item just read from datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemRead(Item item, TransactionOptions options = null)
        {
            Managers.Where(x => x is IItemModelManager).ToList().ForEach(manager =>
            {
                ((IItemModelManager)manager).OnItemRead(item, options);
            });
            var locationData = LocationDataRepo.Read(item, options);
            if (locationData != null)
            {
                item.Data.Add(locationData);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item about to be persisted to datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemUpdate(Item item, TransactionOptions options = null)
        {
            Managers.Where(x => x is IItemModelManager).ToList().ForEach(manager =>
            {
                ((IItemModelManager)manager).OnItemUpdate(item, options);
            });
            if (item.HasData<LocationData>())
            {
                var data = item.GetData<LocationData>();
                LocationDataRepo.Update(data, options);
            }
        }
        /// <summary>
        /// Delete attribute of base item before base item is deleted
        /// </summary>
        /// <param name="item">Base item about to be deleted</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemDelete(Item item, TransactionOptions options = null)
        {
            Managers.Where(x => x is IItemModelManager).ToList().ForEach(manager =>
            {
                ((IItemModelManager)manager).OnItemDelete(item, options);
            });
            if (item.HasAttribute<LocationData>())
            {
                var data = item.GetData<LocationData>();
                LocationDataRepo.Delete(data, options);
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
            if (message.GetMessageId() == CoreReadItemGraphRequest._Id)
                response = HandleCoreReadItemGraphRequest(message, api, terminal, response);
            if (message.GetMessageId() == CoreCreateItemCrudRequest._Id)
                response = HandleCoreCreateItemCrudRequest(message, api, terminal, response);
            if (message.GetMessageId() == CoreReadItemCrudRequest._Id)
                response = HandleCoreReadItemCrudRequest(message, api, terminal, response);
            if (message.GetMessageId() == CoreUpdateItemCrudRequest._Id)
                response = HandleCoreUpdateItemCrudRequest(message, api, terminal, response);
            if (message.GetMessageId() == CoreDeleteItemCrudRequest._Id)
                response = HandleCoreDeleteItemCrudRequest(message, api, terminal, response);
            if (message.GetMessageId() == CoreReadItemJsonRequest._Id)
                response = HandleCoreReadItemJsonRequest(message, api, terminal, response);
            if (message.GetMessageId() == CoreUpdateItemJsonRequest._Id)
                response = HandleCoreUpdateItemJsonRequest(message, api, terminal, response);
            if (message.GetMessageId() == CoreCreateItemJsonRequest._Id)
                response = HandleCoreCreateItemJsonRequest(message, api, terminal, response);
            return response;
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
                ((FileLogger)Logger).LogException($"While handling {request.GetMessageName()}", e);
                response._responseSuccess = false;
                response._responseMessage = e.Message;
            }
            return response;
        }
        #endregion
    }
}
