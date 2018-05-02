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
            var accountRepo = ServiceProvider.GetService<IAccountRepo>();
            var itemRepo = ServiceProvider.GetService<IItemRepo<Item>>();
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
                var items = JsonConvert.DeserializeObject<List<Item>>(jObj["Items"].ToString());
                itemRepo.Create(items);
            }
            return this;
        }
    }
}
