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

namespace BeforeOurTime.Business.Modules.World.Managers
{
    public partial class ExitItemManager
    {
        /// <summary>
        /// Use an exit
        /// </summary>
        /// <param name="api"></param>
        /// <param name="message"></param>
        /// <param name="terminal"></param>
        /// <param name="response"></param>
        public IResponse HandleUseItemRequest(IMessage message, IApi api, ITerminal terminal, IResponse response)
        {
            var request = message.GetMessageAsType<CoreUseItemRequest>();
            response = HandleRequestWrapper<CoreUseItemResponse>(request, res =>
            {
                var exit = api.GetItemManager().Read(request.ItemId.Value).GetAsItem<ExitItem>();
                var destinationLocation = api.GetItemManager().Read(Guid.Parse(exit.Exit.DestinationId));
                var player = api.GetItemManager().Read(terminal.GetPlayerId().Value);
                api.GetItemManager().Move(player, destinationLocation, exit);
                res = api.GetModuleManager().GetManager<ILocationItemManager>()
                    .HandleReadLocationSummaryRequest(new WorldReadLocationSummaryRequest()
                    {
                    }, api, terminal, response);
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
