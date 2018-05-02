using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Messages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Repository.Models;
using BeforeOurTime.Business.JsEvents;
using System.Linq;
using System.IO;
using Newtonsoft.Json.Linq;
using BeforeOurTime.Repository.Models.Accounts.Authorization;
using BeforeOurTime.Repository.Models.Accounts;
using BeforeOurTime.Repository.Models.Accounts.Authentication.Providers;

namespace BeforeOurTime.Business.Apis
{
    /// <summary>
    /// Interface into the game
    /// </summary>
    public partial class Api
    {
        /// <summary>
        /// Install initial accounts and database objects
        /// </summary>
        public IApi DataInstall()
        {
            if (!AccountRepo.Read(0, 1).Any())
            {
                string json = "";
                using (StreamReader r = new StreamReader("Setups\\Import.json"))
                {
                    json = r.ReadToEnd();
                }
                var jObj = JObject.Parse(json);
                AuthorRoleRepo.Create(JsonConvert.DeserializeObject<List<AuthorizationRole>>(jObj["Roles"].ToString()));
                AuthorGroupRepo.Create(JsonConvert.DeserializeObject<List<AuthorizationGroup>>(jObj["Groups"].ToString()));
                AuthorGroupRoleRepo.Create(JsonConvert.DeserializeObject<List<AuthorizationGroupRole>>(jObj["GroupRoles"].ToString()));
                AccountRepo.Create(JsonConvert.DeserializeObject<List<Account>>(jObj["Accounts"].ToString()));
                AuthenBotMetaRepo.Create(JsonConvert.DeserializeObject<List<AuthenticationBotMeta>>(jObj["Authentication"]["BotMeta"].ToString()));
                AuthorAccountGroupRepo.Create(JsonConvert.DeserializeObject<List<AuthorizationAccountGroup>>(jObj["AccountGroups"].ToString()));
                ItemRepo.Create(JsonConvert.DeserializeObject<List<Item>>(jObj["Items"].ToString()));
            }
            return this;
        }
    }
}
