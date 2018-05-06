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
        /// Create a new character
        /// </summary>
        /// <param name="name">Public name of the character</param>
        /// <param name="accountId">Account to which this character belongs</param>
        /// <param name="parentId">Location of new item</param>
        public Character ItemCreateCharacter(
            string name, 
            Guid accountId,
            Guid parentId)
        {
            var parent = ItemRepo.Read(new List<Guid>() { parentId }).First();
            var game = ItemRepo.Read(new List<Guid>() { new Guid("487a7282-0cad-4081-be92-83b14671fc23") }).First();
            var character = new Character()
            {
                Name = name,
                AccountId = accountId,
                Type = ItemType.Character,
                Version = ItemVersion.Alpha,
                UuidType = Guid.NewGuid(),
                ParentId = parentId,
                Data = "{}",
                Script = "{ function onTick(e) {}; function onTerminalOutput(e) { terminalMessage(e.terminal.id, e.raw); }; function onItemMove(e) { }; }"
            };
            ItemCreate<Character>(game, parent, character);
            return character;
        }
    }
}
