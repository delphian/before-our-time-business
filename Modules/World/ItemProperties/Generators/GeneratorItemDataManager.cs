using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using BeforeOurTime.Models;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.World.ItemProperties.Generators;
using BeforeOurTime.Models.Logs;
using BeforeOurTime.Models.Modules.Core.ItemProperties.Visibles;
using Microsoft.Extensions.Configuration;


namespace BeforeOurTime.Business.Modules.World.ItemProperties.Generators
{
    public partial class GeneratorItemDataManager : ItemModelManager<Item>, IGeneratorItemDataManager
    {
        /// <summary>
        /// Manage all modules
        /// </summary>
        private IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Item manager
        /// </summary>
        private IItemManager ItemManager { set; get; }
        /// <summary>
        /// Repository for manager
        /// </summary>
        private IGeneratorItemDataRepo GeneratorItemDataRepo { set; get; }
        /// <summary>
        /// Logger
        /// </summary>
        private IBotLogger Logger { set; get; }
        /// <summary>
        /// Tick interval at which garbage collection will execute
        /// </summary>
        private int TickInterval { set; get; }
        /// <summary>
        /// Current tick interval on it's way to the maximum;
        /// </summary>
        private int TickCount { set; get; } = 0;
        /// <summary>
        /// Number of miliseconds between each tick
        /// </summary>
        private int TickTime { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public GeneratorItemDataManager(
            IModuleManager moduleManager,
            IGeneratorItemDataRepo generatorItemDataRepo)
        {
            ModuleManager = moduleManager;
            GeneratorItemDataRepo = generatorItemDataRepo;
            Logger = ModuleManager.GetLogger();
            TickTime = moduleManager.GetConfiguration()
                .GetSection("Timing")
                .GetValue<int>("Tick");
            TickInterval = moduleManager.GetConfiguration()
                .GetSection("Modules")
                .GetSection("World")
                .GetSection("Managers")
                .GetSection("Generator")
                .GetValue<int>("TickInterval");
            ModuleManager.Ticks += OnTick;
        }
        /// <summary>
        /// Handle recurring regular tasks
        /// </summary>
        public void OnTick()
        {
            if (TickCount++ >= TickInterval)
            {
                Logger.LogInformation("Running generators");
                TickCount = 0;
                var generatorItemDatas = GeneratorItemDataRepo.ReadReadyToRun();
                if (generatorItemDatas.Count > 0)
                {
                    generatorItemDatas.ForEach((generatorItemData) =>
                    {
                        try
                        {
                            var item = JsonConvert.DeserializeObject<Item>(generatorItemData.Json);
                            var parent = ModuleManager.GetManager<IItemManager>().Read(item.ParentId.Value);
                            if (parent.Children?.Count(x => x.TypeId == item.TypeId) < generatorItemData.Maximum)
                            {
                                Logger.LogDebug($"Generating {item.GetProperty<VisibleItemProperty>()?.Name ?? item.Id.ToString()}");
                                ModuleManager.GetManager<IItemManager>().Create(item);
                            }
                        }
                        catch (Exception e)
                        {
                            ModuleManager.GetLogger().LogException("Failed to generate item", e);
                        }
                        finally
                        {
                            generatorItemData.IntervalTime = DateTime.Now
                                .AddMilliseconds(generatorItemData.Interval * TickTime);
                        }
                    });
                    GeneratorItemDataRepo.Update(generatorItemDatas);
                }
            }
        }
        /// <summary>
        /// Get all repositories declared by manager
        /// </summary>
        /// <returns></returns>
        public List<ICrudModelRepository> GetRepositories()
        {
            return new List<ICrudModelRepository>() { GeneratorItemDataRepo };
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
        /// Get all unique item identifiers of managed items
        /// </summary>
        /// <returns></returns>
        public List<Guid> GetItemIds()
        {
            var itemIds = GeneratorItemDataRepo.GetItemIds();
            return itemIds;
        }
        /// <summary>
        /// Determine if a model type should be managed by this manager
        /// </summary>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public override bool IsManaging(Type modelType)
        {
            return false;
        }
        /// <summary>
        /// Determine if an item is managed
        /// </summary>
        /// <param name="item">Item that may have managable data</param>
        public bool IsManaging(Item item)
        {
            return (item.HasData<GeneratorItemData>());
        }
        /// <summary>
        /// Determine if item data type is managable
        /// </summary>
        /// <param name="propertyData">Item data type that might be managable</param>
        public bool IsManagingData(Type dataType)
        {
            return dataType == typeof(GeneratorItemData);
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
        #region On Item Hooks
        /// <summary>
        /// Create attribute, if present, after item is created
        /// </summary>
        /// <param name="item">Base item just created from datastore</param>
        public void OnItemCreate(Item item)
        {
            if (item.HasData<GeneratorItemData>())
            {
                var data = item.GetData<GeneratorItemData>();
                data.DataItemId = item.Id;
                GeneratorItemDataRepo.Create(data);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item just read from datastore</param>
        public void OnItemRead(Item item)
        {
            var data = GeneratorItemDataRepo.Read(item);
            if (data != null)
            {
                item.Data.Add(data);
                item.AddProperty(typeof(GeneratorItemProperty), new GeneratorItemProperty()
                {
                });
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item about to be persisted to datastore</param>
        public void OnItemUpdate(Item item)
        {
            if (item.HasData<GeneratorItemData>())
            {
                var data = item.GetData<GeneratorItemData>();
                if (data.Id == Guid.Empty)
                {
                    OnItemCreate(item);
                }
                else
                {
                    GeneratorItemDataRepo.Update(data);
                }
            }
        }
        /// <summary>
        /// Delete attribute of base item before base item is deleted
        /// </summary>
        /// <param name="item">Base item about to be deleted</param>
        public void OnItemDelete(Item item)
        {
            if (item.HasData<GeneratorItemData>())
            {
                var data = item.GetData<GeneratorItemData>();
                GeneratorItemDataRepo.Delete(data);
            }
        }
        #endregion
    }
}
