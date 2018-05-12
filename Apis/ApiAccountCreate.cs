using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Messages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Repository.Models;
using BeforeOurTime.Repository.Models.Accounts;
using System.Linq;
using BeforeOurTime.Repository.Models.Accounts.Authentication.Providers;

namespace BeforeOurTime.Business.Apis
{
    /// <summary>
    /// Interface into the game
    /// </summary>
    public partial class Api
    {
        /// <summary>
        /// Create a new account and local authentication credentials
        /// </summary>
        /// <param name="name">Friendly account name</param>
        /// <param name="email">Login email address for account</param>
        /// <param name="password">Password for account</param>
        public Account AccountCreate(string name, string email, string password)
        {
            var account = AccountRepo.Create(new List<Account>()
            {
                new Account()
                {
                    Name = name
                }
            }).FirstOrDefault();
            var credentials = AuthenBotMetaRepo.Create(new List<AuthenticationBotMeta>()
            {
                new AuthenticationBotMeta()
                {
                    Email = email,
                    Password = password,
                    AccountId = account.Id
                }
            }).FirstOrDefault();
            return account;
        }
    }
}
