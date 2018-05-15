﻿using BeforeOurTime.Repository.Models.Accounts;
using BeforeOurTime.Repository.Models.Accounts.Authentication.Providers;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Accounts
{
    public interface IAccountManager
    {
        /// <summary>
        /// Create a new account and local authentication credentials
        /// </summary>
        /// <param name="name">Friendly account name</param>
        /// <param name="email">Login email address for account</param>
        /// <param name="password">Password for account</param>
        Account Create(string name, string email, string password);
        /// <summary>
        /// Authenticate a user name and password
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>User account if authenticated, otherwise null</returns>
        Account Authenticate(string email, string password);
        /// <summary>
        /// Read a local authentication credential
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        AuthenticationBotMeta ReadCredential(Guid accountId);
        /// <summary>
        /// Update a credential's password
        /// </summary>
        /// <param name="credential">Local authentication credential</param>
        /// <param name="password">New password</param>
        /// <returns></returns>
        AuthenticationBotMeta UpdateCredentialPassword(AuthenticationBotMeta credential, string password);
    }
}