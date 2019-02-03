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
using BeforeOurTime.Models.Logs;
using Microsoft.Extensions.Configuration;
using BeforeOurTime.Models.Modules.Script.ItemProperties.Javascripts;
using BeforeOurTime.Models.Modules.Script;
using Jint;
using BeforeOurTime.Models.Modules.Core.Models.Properties;
using BeforeOurTime.Models.Modules.Core.Messages.UseItem;
using BeforeOurTime.Models.Messages;
using System.Collections;
using BeforeOurTime.Models.Exceptions;
using BeforeOurTime.Business.Modules.Script.ItemProperties.Javascripts.Functions;

namespace BeforeOurTime.Business.Modules.Script.ItemProperties.Javascripts
{
    public partial class JavascriptItemDataManager : ItemModelManager<Item>, IJavascriptItemDataManager
    {
        private readonly object _lock = new object();
        public static readonly Guid CommandJavascript = new Guid("22a73822-6655-4b7b-aa2d-100b5c4a00a7");
        /// <summary>
        /// Manage all modules
        /// </summary>
        private IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Manage all script managers
        /// </summary>
        private IScriptModule ScriptModule { set; get; }
        /// <summary>
        /// Item manager
        /// </summary>
        private IItemManager ItemManager { set; get; }
        /// <summary>
        /// Repository for manager
        /// </summary>
        private IJavascriptItemDataRepo JavascriptItemDataRepo { set; get; }
        /// <summary>
        /// Logger
        /// </summary>
        private IBotLogger Logger { set; get; }
        /// <summary>
        /// All javascript function classes
        /// </summary>
        private List<IJavascriptFunction> FunctionClasses = new List<IJavascriptFunction>();
        /// <summary>
        /// All javascript function definitions
        /// </summary>
        private List<JavascriptFunctionDefinition> FunctionDefinitions = new List<JavascriptFunctionDefinition>();
        /// <summary>
        /// Javascript engine
        /// </summary>
        public Engine JSEngine { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        public JavascriptItemDataManager(
            IModuleManager moduleManager,
            IScriptModule scriptModule,
            IJavascriptItemDataRepo javascriptItemDataRepo)
        {
            ModuleManager = moduleManager;
            ScriptModule = scriptModule;
            JavascriptItemDataRepo = javascriptItemDataRepo;
            Logger = ModuleManager.GetLogger();
            JSEngine = new Engine((cfg => cfg.AllowClr()));
            ModuleManager.RegisterForItemCommands(HandleUseItemCommand);
            FunctionClasses = BuildFunctions(ModuleManager);
            ModuleManager.ModuleManagerReadyEvent += () =>
            {
                ItemManager = ModuleManager.GetManager<IItemManager>();
            };
        }
        /// <summary>
        /// Register any classes that implemented IJavascriptFunction
        /// </summary>
        /// <param name="moduleManager"></param>
        public List<IJavascriptFunction> BuildFunctions(IModuleManager moduleManager)
        {
            List<IJavascriptFunction> classes = new List<IJavascriptFunction>();
            var interfaceType = typeof(IJavascriptFunction);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .ToList();
            types.ForEach(type =>
            {
                try
                {
                    classes.Add((IJavascriptFunction)Activator.CreateInstance(type, moduleManager));
                }
                catch (Exception e)
                {
                    Logger.LogException($"Failed to load javascript definition \"{type.ToString()}\"", e);
                }
            });
            return classes;
        }
        /// <summary>
        /// Add a javascript function definition
        /// </summary>
        /// <param name="function"></param>
        public void AddFunctionDefinition(JavascriptFunctionDefinition function)
        {
            FunctionDefinitions.Add(function);
        }
        public Engine GetJSEngine()
        {
            return JSEngine;
        }
        public void SetupJintItem(Engine jsEngine, Item item)
        {
            jsEngine.SetValue("me", item);
        }
        /// <summary>
        /// Get all repositories declared by manager
        /// </summary>
        /// <returns></returns>
        public List<ICrudModelRepository> GetRepositories()
        {
            return new List<ICrudModelRepository>() { JavascriptItemDataRepo };
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
            var itemIds = JavascriptItemDataRepo.GetItemIds();
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
            return (item.HasData<JavascriptItemData>());
        }
        /// <summary>
        /// Determine if item data type is managable
        /// </summary>
        /// <param name="propertyData">Item data type that might be managable</param>
        public bool IsManagingData(Type dataType)
        {
            return dataType == typeof(JavascriptItemData);
        }
        /// <summary>
        /// Get a string of function declarations delimited by semicolons.
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public string GetFunctionDeclarations(string script)
        {
            string declarations = "";
            if (script != null)
            {
                var parser = new Jint.Parser.JavaScriptParser();
                var program = parser.Parse(script);
                if (program.FunctionDeclarations.Any())
                {
                    declarations = ":";
                    program.FunctionDeclarations.ToList().ForEach((declaration) =>
                    {
                        declarations += declaration.Id.Name + ":";
                    });
                }
                if (program.VariableDeclarations.Any())
                {
                    declarations = (declarations.Length == 0) ? ":" : declarations;
                    program.VariableDeclarations.ToList().ForEach((declaration) =>
                    {
                        declarations += declaration.Declarations.First().Id.Name + ":";
                    });
                }
            }
            return declarations;
        }
        /// <summary>
        /// Execute a javascript function of an item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object ExecuteFunction(Item item, string name, params object[] parameters)
        {
            object report = null;
            if (item.GetData<JavascriptItemData>() is JavascriptItemData data)
            {
                if (data.ScriptFunctions.Contains($":{name}:", StringComparison.InvariantCultureIgnoreCase) &&
                    data.Disabled == false)
                {
                    try
                    {
                        lock (_lock)
                        {
                            JSEngine.EnterExecutionContext(JSEngine.GlobalEnvironment, JSEngine.GlobalEnvironment, null);
                            SetupJintItem(JSEngine, item);
                            JSEngine.Execute(data.Script);
                            report = JSEngine.Invoke(name, parameters)?.ToObject();
                            JSEngine.LeaveExecutionContext();
                        }
                    }
                    catch (Exception e)
                    {
                        data.ErrCount += 1;
                        data.ErrDescriptions += $"Executing {name}: {e.Message}\n";
                        Logger.LogException($"JS Engine executing {name} in {item.Id}", e);
                        if (data.ErrCount >= 10)
                        {
                            data.Disabled = true;
                            Logger.LogError($"Disabled javascript for item {item.Id}");
                            Logger.LogException($"JS Engine executing {name} in {item.Id}", e);
                        }
                        JavascriptItemDataRepo.Update(data);
                    }
                }
            }
            return report;
        }
        /// <summary>
        /// Execute a javascript function of many items
        /// </summary>
        /// <param name="items"></param>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public void ExecuteFunction(List<Item> items, string name, params object[] parameters)
        {
            items.ForEach(item =>
            {
                ExecuteFunction(item, name, parameters);
            });
        }
        /// <summary>
        /// Handle request to invoke an item command
        /// </summary>
        /// <param name="itemCommand"></param>
        /// <param name="origin"></param>
        public CoreUseItemEvent HandleUseItemCommand(ItemCommand itemCommand, Item origin)
        {
            CoreUseItemEvent continueIfNull = null;
            if (itemCommand.Id == CommandJavascript)
            {
                var itemManager = ModuleManager.GetManager<IItemManager>();
                var item = itemManager.Read(itemCommand.ItemId.Value);
                ExecuteFunction(item, "onUse", itemCommand, origin);
                continueIfNull = new CoreUseItemEvent()
                {
                    Success = true,
                    Used = item,
                    Using = origin,
                    Use = itemCommand
                };
            }
            return continueIfNull;
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
        /// Read all javascript function definitions
        /// </summary>
        /// <param name="message"></param>
        /// <param name="origin">Item that initiated request</param>
        /// <param name="mm">Module manager</param>
        /// <param name="response"></param>
        public IResponse HandleReadJSDefinitionsRequest(
            IMessage message,
            Item origin,
            IModuleManager mm,
            IResponse response)
        {
            var request = message.GetMessageAsType<ScriptReadJSDefinitionsRequest>();
            response = HandleRequestWrapper<ScriptReadJSDefinitionsResponse>(request, res =>
            {
                var messageManager = mm.GetManager<IMessageManager>();
                var itemManager = mm.GetManager<IItemManager>();
                var location = itemManager.Read(origin.ParentId.Value);
                ((ScriptReadJSDefinitionsResponse)res).Definitions = FunctionDefinitions;
                res.SetSuccess(true);
            });
            return response;
        }
        #region On Item Hooks
        /// <summary>
        /// Create attribute, if present, after item is created
        /// </summary>
        /// <param name="item">Base item just created from datastore</param>
        public void OnItemCreate(Item item)
        {
            if (item.HasData<JavascriptItemData>())
            {
                var data = item.GetData<JavascriptItemData>();
                data.DataItemId = item.Id;
                data.ScriptFunctions = GetFunctionDeclarations(data.Script);
                JavascriptItemDataRepo.Create(data);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item just read from datastore</param>
        public void OnItemRead(Item item)
        {
            var data = JavascriptItemDataRepo.Read(item);
            if (data != null)
            {
                item.Data.Add(data);
                item.AddProperty(typeof(JavascriptItemProperty), new JavascriptItemProperty()
                {
                });
                ExecuteFunction(item, "onItemRead", item);
            }
        }
        /// <summary>
        /// Append attribute to base item when it is loaded
        /// </summary>
        /// <param name="item">Base item about to be persisted to datastore</param>
        public void OnItemUpdate(Item item)
        {
            if (item.HasData<JavascriptItemData>())
            {
                var data = item.GetData<JavascriptItemData>();
                if (data.Id == Guid.Empty)
                {
                    OnItemCreate(item);
                }
                else
                {
                    data.ScriptFunctions = GetFunctionDeclarations(data.Script);
                    JavascriptItemDataRepo.Update(data);
                }
            }
            else if (JavascriptItemDataRepo.Read(item) is JavascriptItemData data)
            {
                JavascriptItemDataRepo.Delete(data);
            }
        }
        /// <summary>
        /// Delete attribute of base item before base item is deleted
        /// </summary>
        /// <param name="item">Base item about to be deleted</param>
        public void OnItemDelete(Item item)
        {
            if (item.HasData<JavascriptItemData>())
            {
                var data = item.GetData<JavascriptItemData>();
                JavascriptItemDataRepo.Delete(data);
            }
        }
        #endregion
    }
}
