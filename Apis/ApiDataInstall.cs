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
        /// <param name="path">The relative or absolute path to the directory</param>
        public IApi DataInstall(string path)
        {
            if (!AccountRepo.Read(0, 1).Any())
            {
                DataInstallDirectory(path);
            }
            return this;
        }
        /// <summary>
        /// Install all JSON install files located in a single directory
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory</param>
        /// <returns></returns>
        public IApi DataInstallDirectory(string path)
        {
            Directory.GetFiles(path, "*.json").ToList().ForEach(delegate (string filePath)
            {
                DataInstallFile(filePath);
            });
            if (Directory.GetDirectories(path).ToList().Any())
            {
                Directory.GetDirectories(path).ToList().ForEach(delegate (string dirPath)
                {
                    DataInstallDirectory(dirPath);
                });
            }
            return this;
        }
        /// <summary>
        /// Parse a single JSON install file to create accounts and items
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public IApi DataInstallFile(string path)
        {
            string json = "";
            using (StreamReader r = new StreamReader(path))
            {
                json = r.ReadToEnd();
            }
            var jObj = JObject.Parse(json);
            if (jObj["Roles"] != null)
            {
                AuthorRoleRepo.Create(JsonConvert.DeserializeObject<List<AuthorizationRole>>(jObj["Roles"].ToString()));
            }
            if (jObj["Groups"] != null)
            {
                AuthorGroupRepo.Create(JsonConvert.DeserializeObject<List<AuthorizationGroup>>(jObj["Groups"].ToString()));
            }
            if (jObj["GroupRoles"] != null)
            {
                AuthorGroupRoleRepo.Create(JsonConvert.DeserializeObject<List<AuthorizationGroupRole>>(jObj["GroupRoles"].ToString()));
            }
            if (jObj["Accounts"] != null)
            {
                AccountRepo.Create(JsonConvert.DeserializeObject<List<Account>>(jObj["Accounts"].ToString()));
            }
            if (jObj["Authentication"] != null && jObj["Authentication"]["BotMeta"] != null)
            {
                AuthenBotMetaRepo.Create(JsonConvert.DeserializeObject<List<AuthenticationBotMeta>>(jObj["Authentication"]["BotMeta"].ToString()));
            }
            if (jObj["AccountGroups"] != null)
            {
                AuthorAccountGroupRepo.Create(JsonConvert.DeserializeObject<List<AuthorizationAccountGroup>>(jObj["AccountGroups"].ToString()));
            }
            if (jObj["Items"] != null)
            {
                ItemRepo.Create(JsonConvert.DeserializeObject<List<Item>>(jObj["Items"].ToString()));
            }
            return this;
        }
    }
}
