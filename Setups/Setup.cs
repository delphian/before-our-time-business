using BeforeOurTime.Repository.Models;
using BeforeOurTime.Repository.Models.Accounts;
using BeforeOurTime.Repository.Models.Accounts.Authentication.Providers;
using BeforeOurTime.Repository.Models.Accounts.Authorization;
using BeforeOurTime.Repository.Models.Items;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
                var roles = new List<AuthorizationRole>();
                // Account Roles
                var accountRoles = new List<AuthorizationRole>()
                    {
                        new AuthorizationRole { NameUnique = "AccountCreate", Name = "Create Account" },
                        new AuthorizationRole { NameUnique = "AccountRead", Name = "Read Account" },
                        new AuthorizationRole { NameUnique = "AccountUpdate", Name = "Update Account" },
                        new AuthorizationRole { NameUnique = "AccountDelete", Name = "Delete Account" }
                    };
                authorRoleRepo.Create(accountRoles);
                roles.AddRange(accountRoles);
                // Item Roles
                var itemRoles = new List<AuthorizationRole>()
                    {
                        new AuthorizationRole { NameUnique = "ItemCreate", Name = "Create Item" },
                        new AuthorizationRole { NameUnique = "ItemRead", Name = "Read Item" },
                        new AuthorizationRole { NameUnique = "ItemUpdate", Name = "Update Item" },
                        new AuthorizationRole { NameUnique = "ItemDelete", Name = "Delete Item" }
                    };
                authorRoleRepo.Create(itemRoles);
                roles.AddRange(itemRoles);
                // Sysop Group
                var groupSysop = authorGroupRepo.Create(new List<AuthorizationGroup>()
                    {
                        new AuthorizationGroup { Name = "Sysop" }
                    }).First();
                var groupRoles = authorGroupRoleRepo.Create(new List<AuthorizationGroupRole>() {
                        new AuthorizationGroupRole { Role = roles.Where(x => x.NameUnique == "AccountCreate").First(), Group = groupSysop },
                        new AuthorizationGroupRole { Role = roles.Where(x => x.NameUnique == "AccountRead").First(), Group = groupSysop },
                        new AuthorizationGroupRole { Role = roles.Where(x => x.NameUnique == "AccountUpdate").First(), Group = groupSysop },
                        new AuthorizationGroupRole { Role = roles.Where(x => x.NameUnique == "AccountDelete").First(), Group = groupSysop },
                        new AuthorizationGroupRole { Role = roles.Where(x => x.NameUnique == "ItemCreate").First(), Group = groupSysop },
                        new AuthorizationGroupRole { Role = roles.Where(x => x.NameUnique == "ItemRead").First(), Group = groupSysop },
                        new AuthorizationGroupRole { Role = roles.Where(x => x.NameUnique == "ItemUpdate").First(), Group = groupSysop },
                        new AuthorizationGroupRole { Role = roles.Where(x => x.NameUnique == "ItemDelete").First(), Group = groupSysop }
                    });
                // Accounts
                var account = accountRepo.Create(new List<Account>() {
                        new Account { Name = "First Born" }
                    }).First();
                // Account credentials
                var authenBotMeta = authenBotMetaRepo.Create(new List<AuthenticationBotMeta>()
                    {
                        new AuthenticationBotMeta {
                            Email = Configuration.GetSection("Admin").GetSection("Email").Value,
                            Password = Configuration.GetSection("Admin").GetSection("Password").Value,
                            Account = account
                        }
                    }).First();
                // Assign groups to accounts
                var accountGroup = authorAccountGroupRepo.Create(new List<AuthorizationAccountGroup>()
                    {
                        new AuthorizationAccountGroup { Account = account, Group = groupSysop }
                    }).First();
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
                    Uuid = new Guid("487a7282-0cad-4081-be92-83b14671fc23"),
                    UuidType = new Guid("75f55af3-3027-404c-b9f0-7b21ead826b2"),
                    ParentId = null,
                    Children = new List<Item>(),
                    Data = @"{
                    }",
                    Script = @"
                        function onEventTick(e) {
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
