using BeforeOurTime.Repository.Models;
using BeforeOurTime.Repository.Models.Accounts;
using BeforeOurTime.Repository.Models.Accounts.Authentication.Providers;
using BeforeOurTime.Repository.Models.Accounts.Authorization;
using BeforeOurTime.Repository.Models.Items;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Setups
{
    public class Setup
    {
        public static IConfigurationRoot Configuration { set; get; }
        public static IServiceProvider ServiceProvider { set; get; }

        public Setup(IConfigurationRoot configuration, IServiceProvider serviceProvider)
        {
            Configuration = configuration;
            ServiceProvider = serviceProvider;
        }
        public Setup Install()
        {
            InstallItems();
            InstallAccounts();
            return this;
        }
        public Setup InstallAccounts()
        {
            var accountRepo = ServiceProvider.GetService<IAccountRepo>();
            var authorRoleRepo = ServiceProvider.GetService<IRepository<AuthorizationRole>>();
            var authorGroupRepo = ServiceProvider.GetService<IRepository<AuthorizationGroup>>();
            var authorGroupRoleRepo = ServiceProvider.GetService<IRepository<AuthorizationGroupRole>>();
            var authorAccountGroupRepo = ServiceProvider.GetService<IRepository<AuthorizationAccountGroup>>();
            var authenBotMetaRepo = ServiceProvider.GetService<IRepository<AuthenticationBotMeta>>();
            if (!accountRepo.Read(0, 1).Any())
            {
                string json = "";
                using (StreamReader r = new StreamReader("Setups\\Import.json"))
                {
                    json = r.ReadToEnd();
                }
                var jObj = JObject.Parse(json);
                authorRoleRepo.Create(JsonConvert.DeserializeObject<List<AuthorizationRole>>(jObj["Roles"].ToString()));
                authorGroupRepo.Create(JsonConvert.DeserializeObject<List<AuthorizationGroup>>(jObj["Groups"].ToString()));
                authorGroupRoleRepo.Create(JsonConvert.DeserializeObject<List<AuthorizationGroupRole>>(jObj["GroupRoles"].ToString()));
                accountRepo.Create(JsonConvert.DeserializeObject<List<Account>>(jObj["Accounts"].ToString()));
                authenBotMetaRepo.Create(JsonConvert.DeserializeObject<List<AuthenticationBotMeta>>(jObj["Authentication"]["BotMeta"].ToString()));
                authorAccountGroupRepo.Create(JsonConvert.DeserializeObject<List<AuthorizationAccountGroup>>(jObj["AccountGroups"].ToString()));
            }
            return this;
        }
        public Setup InstallItems()
        {
            var itemRepo = ServiceProvider.GetService<IItemRepo<Item>>();
            if (!itemRepo.Read(0, 1).Any())
            {
                var gameItem = itemRepo.Create(new List<Item>() { new Item()
                {
                    Type = ItemType.Game,
                    Id = new Guid("487a7282-0cad-4081-be92-83b14671fc23"),
                    UuidType = new Guid("75f55af3-3027-404c-b9f0-7b21ead826b2"),
                    ParentId = null,
                    Children = new List<Item>(),
                    Data = @"{
                    }",
                    Script = @"
                        function onTick(e) {
                        };
                    "
                } }).First();
                var interfaceType = typeof(ISetup);
                var setupItems = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(x => x.GetTypes())
                    .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Select(x => Activator.CreateInstance(x))
                    .ToList();
                List<Item> items = setupItems
                    .SelectMany(x => ((ISetup)x).Items(Configuration, ServiceProvider))
                    .ToList();
                itemRepo.Create(items);
            }
            return this;
        }
    }
}
