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

namespace BeforeOurTime.Business.Apis
{
    /// <summary>
    /// Interface into the game
    /// </summary>
    public partial class Api
    {
        /// <summa+y>
        /// Truncate all tables in database
        /// </summary>
        public IApi DataReset()
        {
            AccountRepo.Delete();
            AuthorRoleRepo.Delete();
            AuthorGroupRepo.Delete();
            AuthorGroupRoleRepo.Delete();
            AuthorAccountGroupRepo.Delete();
            AuthenBotMetaRepo.Delete();
            MessageRepo.Delete();
            ItemRepo.Delete();
            return this;
        }
    }
}
