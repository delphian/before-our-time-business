using BeforeOurTime.Business.Modules.World.Dbs.EF;
using BeforeOurTime.Business.Modules.World.Managers;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Models.Data;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.World;
using BeforeOurTime.Models.Modules.World.Dbs;
using BeforeOurTime.Models.Modules.World.Managers;
using BeforeOurTime.Models.Modules.World.Messages.Location.CreateLocation;
using BeforeOurTime.Models.Modules.World.Messages.Location.ReadLocationSummary;
using BeforeOurTime.Models.Modules.World.Models.Data;
using BeforeOurTime.Models.Modules.World.Models.Items;
using BeforeOutTime.Business.Dbs.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        /// Character data repository
        /// </summary>
        private IExitDataRepo ExitDataRepo { set; get; }
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
            var managers = new List<IModelManager>
            {
                new GameItemManager(moduleManager, new EFGameDataRepo(db, moduleManager.GetItemRepo())),
                new LocationItemManager(moduleManager, new EFLocationDataRepo(db, moduleManager.GetItemRepo())),
                new CharacterItemManager(moduleManager, new EFCharacterDataRepo(db, moduleManager.GetItemRepo())),
                new ExitItemManager(moduleManager, new EFExitDataRepo(db, moduleManager.GetItemRepo()))
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
                WorldCreateLocationQuickRequest._Id
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
            ExitDataRepo = repositories
                .Where(x => x is IExitDataRepo)
                .Select(x => (IExitDataRepo)x).FirstOrDefault();
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
                var gameItem = ModuleManager.GetItemRepo().Create(new GameItem()
                {
                    Id = new Guid("f4212bfe-ef65-4632-df2b-08d63af92e75"),
                    ParentId = null,
                    Data = new List<IItemData>()
                    {
                        new GameData()
                        {
                            Id = new Guid("0f290372-6812-4eba-6f6c-08d63af92e80"),
                            DataItemId = new Guid("f4212bfe-ef65-4632-df2b-08d63af92e75"),
                            Name = "Brave New World",
                            DefaultLocationId = new Guid("91f4a03f-8cb8-467c-df2c-08d63af92e75")
                        }
                    }
                });
                var locationItem = ModuleManager.GetItemRepo().Create(new LocationItem()
                {
                    Id = new Guid("91f4a03f-8cb8-467c-df2c-08d63af92e75"),
                    ParentId = gameItem.Id,
                    Data = new List<IItemData>()
                    {
                        new LocationData()
                        {
                            Id = new Guid("e370301f-2b91-43a0-9a30-08d63af92e86"),
                            DataItemId = new Guid("91f4a03f-8cb8-467c-df2c-08d63af92e75"),
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
            if (game.GetData<GameData>().DefaultLocationId != null)
            {
                defaultLocationItem = ModuleManager.GetItemRepo().Read(game.GetData<GameData>().DefaultLocationId.Value);
            }
            return defaultLocationItem;
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
                response = GetManager<ILocationItemManager>().HandleReadLocationSummaryRequest(message, origin, mm, response);
            if (message.GetMessageId() == WorldCreateLocationQuickRequest._Id)
                response = GetManager<ILocationItemManager>().HandleCreateLocationQuickRequest(message, origin, mm, response);
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
