using BeforeOurTime.Business.Modules.ItemProperties.Characters;
using BeforeOurTime.Business.Modules.ItemProperties.Games;
using BeforeOurTime.Business.Modules.World.Dbs.EF;
using BeforeOurTime.Business.Modules.World.ItemProperties.Characters;
using BeforeOurTime.Business.Modules.World.ItemProperties.Exits;
using BeforeOurTime.Business.Modules.World.ItemProperties.Games;
using BeforeOurTime.Business.Modules.World.ItemProperties.Generators;
using BeforeOurTime.Business.Modules.World.ItemProperties.Locations;
using BeforeOurTime.Business.Modules.World.ItemProperties.Physicals;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.ItemProperties.Visibles;
using BeforeOurTime.Models.Modules.Core.Models.Data;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.World;
using BeforeOurTime.Models.Modules.World.ItemProperties.Characters;
using BeforeOurTime.Models.Modules.World.ItemProperties.Exits;
using BeforeOurTime.Models.Modules.World.ItemProperties.Games;
using BeforeOurTime.Models.Modules.World.ItemProperties.Locations;
using BeforeOurTime.Models.Modules.World.ItemProperties.Locations.Messages.CreateLocation;
using BeforeOurTime.Models.Modules.World.ItemProperties.Locations.Messages.ReadLocationSummary;
using BeforeOurTime.Models.Modules.World.Messages.Emotes.PerformEmote;
using BeforeOutTime.Business.Dbs.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Modules.World
{
    public partial class WorldModule : IWorldModule
    {
        /// <summary>
        /// Entity framework database context
        /// </summary>
        private EFWorldModuleContext Db { set; get; }
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
        /// Game data repository
        /// </summary>
        private IGameItemDataRepo GameDataRepo { set; get; }
        /// <summary>
        /// Location data repository
        /// </summary>
        private ILocationItemDataRepo LocationDataRepo { set; get; }
        /// <summary>
        /// Character data repository
        /// </summary>
        private ICharacterItemDataRepo CharacterDataRepo { set; get; }
        /// <summary>
        /// Character data repository
        /// </summary>
        private IExitItemDataRepo ExitDataRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public WorldModule(
            IModuleManager moduleManager)
        {
            ModuleManager = moduleManager;
            var connectionString = ModuleManager.GetConfiguration().GetConnectionString("DefaultConnection");
            var dbOptions = new DbContextOptionsBuilder<BaseContext>();
                dbOptions.UseSqlServer(connectionString);
            Db = new EFWorldModuleContext(dbOptions.Options);
            Managers = BuildManagers(ModuleManager, Db);
            Repositories = Managers.SelectMany(x => x.GetRepositories()).ToList();
        }
        /// <summary>
        /// Build all the item managers for the module
        /// </summary>
        /// <param name="db"></param>
        /// <param name="itemRepo"></param>
        /// <returns></returns>
        List<IModelManager> BuildManagers(IModuleManager moduleManager, EFWorldModuleContext db)
        {
            var itemRepo = moduleManager.GetItemRepo();
            var managers = new List<IModelManager>
            {
                new GameItemDataManager(moduleManager, new EFGameItemDataRepo(db, itemRepo)),
                new LocationItemDataManager(moduleManager, new EFLocationItemDataRepo(db, itemRepo)),
                new CharacterItemDataManager(moduleManager, new EFCharacterItemDataRepo(db, itemRepo)),
                new ExitItemDataManager(moduleManager, new EFExitItemDataRepo(db, itemRepo)),
                new PhysicalItemDataManager(moduleManager, new EFPhysicalItemDataRepo(db, itemRepo)),
                new GeneratorItemDataManager(moduleManager, new EFGeneratorItemDataRepo(db, itemRepo))
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
                WorldReadLocationSummaryRequest._Id,
                WorldCreateLocationQuickRequest._Id,
                WorldPerformEmoteRequest._Id
            };
        }
        /// <summary>
        /// Initialize module
        /// </summary>
        /// <param name="repositories"></param>
        public void Initialize(List<ICrudModelRepository> repositories)
        {
            GameDataRepo = repositories
                .Where(x => x is IGameItemDataRepo)
                .Select(x => (IGameItemDataRepo)x).FirstOrDefault();
            LocationDataRepo = repositories
                .Where(x => x is ILocationItemDataRepo)
                .Select(x => (ILocationItemDataRepo)x).FirstOrDefault();
            CharacterDataRepo = repositories
                .Where(x => x is ICharacterItemDataRepo)
                .Select(x => (ICharacterItemDataRepo)x).FirstOrDefault();
            ExitDataRepo = repositories
                .Where(x => x is IExitItemDataRepo)
                .Select(x => (IExitItemDataRepo)x).FirstOrDefault();
            ModuleManager.GetItemRepo().OnItemCreate += OnItemCreate;
            ModuleManager.GetItemRepo().OnItemRead += OnItemRead;
            ModuleManager.GetItemRepo().OnItemUpdate += OnItemUpdate;
            ModuleManager.GetItemRepo().OnItemDelete += OnItemDelete;
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
                var gameItem = ModuleManager.GetItemRepo().Create(new Item()
                {
                    Id = new Guid("f4212bfe-ef65-4632-df2b-08d63af92e75"),
                    ParentId = null,
                    Data = new List<IItemData>()
                    {
                        new GameItemData()
                        {
                            Id = new Guid("0f290372-6812-4eba-6f6c-08d63af92e80"),
                            DataItemId = new Guid("f4212bfe-ef65-4632-df2b-08d63af92e75"),
                            DefaultLocationId = new Guid("91f4a03f-8cb8-467c-df2c-08d63af92e75")
                        },
                        new VisibleItemData()
                        {
                            Name = "Brave New World"
                        }
                    }
                });
                var locationItem = ModuleManager.GetItemRepo().Create(new Item()
                {
                    Id = new Guid("91f4a03f-8cb8-467c-df2c-08d63af92e75"),
                    ParentId = gameItem.Id,
                    Data = new List<IItemData>()
                    {
                        new LocationItemData()
                        {
                            Id = new Guid("e370301f-2b91-43a0-9a30-08d63af92e86"),
                            DataItemId = new Guid("91f4a03f-8cb8-467c-df2c-08d63af92e75")
                        },
                        new VisibleItemData()
                        {
                            Name = "A Dark Void",
                            Description = "Cool mists and dark shadows shroud "
                                + "everything in this place. Straining your eyes does little to resolve the "
                                + "amorphous blobs that are circulating about. The oppresive silence is occationaly "
                                + "puncuated by a distressed weeping or soft sob. A chill runs through your blood "
                                + "when you realise these forms may have once been human. The smell of rain "
                                + "and rotting wood pains your nose while the occational drip of water tickles "
                                + "the top of skulls both real and imagined. Any attempt to navigate in this damp "
                                + "cavern causes disorientation."
                        }
                    }
                });
                defaultGameData = gameItem.GetData<GameItemData>();
                defaultGameData.DefaultLocationId = locationItem.GetData<LocationItemData>().DataItemId;
                GameDataRepo.Update(defaultGameData);
            }
            var defaultGameItem = ModuleManager.GetItemRepo().Read(defaultGameData.DataItemId);
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
            if (game.GetData<GameItemData>().DefaultLocationId != null)
            {
                defaultLocationItem = ModuleManager.GetItemRepo().Read(game.GetData<GameItemData>().DefaultLocationId.Value);
            }
            return defaultLocationItem;
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
        #region On Item Hooks
        /// <summary>
        /// Create attribute, if present, after item is created
        /// </summary>
        /// <param name="item">Base item just created from datastore</param>
        public void OnItemCreate(Item item)
        {
            Managers.Where(x => x is IItemModelManager).ToList().ForEach(manager =>
            {
                ((IItemModelManager)manager).OnItemCreate(item);
            });
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item just read from datastore</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        public void OnItemRead(Item item)
        {
            Managers.Where(x => x is IItemModelManager).ToList().ForEach(manager =>
            {
                ((IItemModelManager)manager).OnItemRead(item);
            });
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item about to be persisted to datastore</param>
        public void OnItemUpdate(Item item)
        {
            Managers.Where(x => x is IItemModelManager).ToList().ForEach(manager =>
            {
                ((IItemModelManager)manager).OnItemUpdate(item);
            });
        }
        /// <summary>
        /// Delete attribute of base item before base item is deleted
        /// </summary>
        /// <param name="item">Base item about to be deleted</param>
        public void OnItemDelete(Item item)
        {
            Managers.Where(x => x is IItemModelManager).ToList().ForEach(manager =>
            {
                ((IItemModelManager)manager).OnItemDelete(item);
            });
        }
        #endregion
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
            if (message.GetMessageId() == WorldReadLocationSummaryRequest._Id)
                response = GetManager<ILocationItemDataManager>().HandleReadLocationSummaryRequest(message, origin, mm, response);
            if (message.GetMessageId() == WorldCreateLocationQuickRequest._Id)
                response = GetManager<ILocationItemDataManager>().HandleCreateLocationQuickRequest(message, origin, mm, response);
            if (message.GetMessageId() == WorldPerformEmoteRequest._Id)
                response = HandlePerformEmoteRequest(message, origin, mm, response);
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
                ModuleManager.GetLogger().LogException($"While handling {request.GetMessageName()}", e);
                response._responseSuccess = false;
                response._responseMessage = e.Message;
            }
            return response;
        }
        #endregion
    }
}
