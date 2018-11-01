using BeforeOurTime.Models.Messages.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Terminals;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Modules.Core.Messages.UseItem;
using BeforeOurTime.Models.Modules.World.Models.Items;
using BeforeOurTime.Models.Modules.World.Managers;
using BeforeOurTime.Models.Modules.World.Messages.Location.ReadLocationSummary;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Managers;

namespace BeforeOurTime.Business.Modules.Core
{
    public partial class CoreModule
    {
        /// <summary>
        /// Use an exit
        /// </summary>
        /// <param name="message"></param>
        /// <param name="mm">Module manager</param>
        /// <param name="terminal">Terminal that initiated request</param>
        /// <param name="response"></param>
        private IResponse HandleCoreUseItemRequest(
            IMessage message,
            IModuleManager mm,
            ITerminal terminal,
            IResponse response)
        {
            var request = message.GetMessageAsType<CoreUseItemRequest>();
            var user = mm.GetManager<IItemManager>().Read(terminal.GetPlayerId().Value);
            response = mm.UseItem(request, user, terminal, response);
            return response;
        }
    }
}
