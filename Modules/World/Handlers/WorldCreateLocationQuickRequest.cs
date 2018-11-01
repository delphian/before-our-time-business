using BeforeOurTime.Models.Messages.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Models.Modules.World.Messages.Location.CreateLocation;
using BeforeOurTime.Models.Modules.World.Managers;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.Terminal.Models;

namespace BeforeOurTime.Business.Modules.World.Managers
{
    public partial class LocationItemManager
    {
        /// <summary>
        /// Create location
        /// </summary>
        /// <param name="message"></param>
        /// <param name="mm">Module manager</param>
        /// <param name="terminal">Terminal that initiated request</param>
        /// <param name="response"></param>
        public IResponse HandleCreateLocationQuickRequest(
            IMessage message,
            IModuleManager mm,
            ITerminal terminal,
            IResponse response)
        {
            var request = message.GetMessageAsType<WorldCreateLocationQuickRequest>();
            response = HandleRequestWrapper<WorldCreateLocationQuickResponse>(request, res =>
            {
                var itemManager = mm.GetManager<IItemManager>();
                var player = itemManager.Read(terminal.GetPlayerId().Value);
                var location = itemManager.Read(player.ParentId.Value);
                var createFromLocationItemId = request.FromLocationItemId ??
                                               player.ParentId.Value;
                var newLocationItem = mm.GetManager<ILocationItemManager>()
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
