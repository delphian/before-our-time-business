using BeforeOurTime.Models.Messages.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Terminals;
using BeforeOurTime.Models.Modules.World.Messages.Location.CreateLocation;
using BeforeOurTime.Models.Modules.World.Managers;
using BeforeOurTime.Models.Messages;

namespace BeforeOurTime.Business.Modules.World.Managers
{
    public partial class LocationItemManager
    {
        /// <summary>
        /// Create location
        /// </summary>
        /// <param name="api"></param>
        /// <param name="terminal"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        public IResponse HandleCreateLocationQuickRequest(IMessage message, IApi api, ITerminal terminal, IResponse response)
        {
            var request = message.GetMessageAsType<WorldCreateLocationQuickRequest>();
            response = HandleRequestWrapper<WorldCreateLocationQuickResponse>(request, res =>
            {
                var player = api.GetItemManager().Read(terminal.GetPlayerId().Value);
                var location = api.GetItemManager().Read(player.ParentId.Value);
                var createFromLocationItemId = request.FromLocationItemId ??
                                               player.ParentId.Value;
                var newLocationItem = api
                    .GetModuleManager()
                    .GetManager<ILocationItemManager>()
                    .CreateFromHere(createFromLocationItemId);
                ((WorldCreateLocationQuickResponse)res).CreateLocationEvent = new WorldCreateLocationEvent()
                {
                    Item = newLocationItem
                };
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
