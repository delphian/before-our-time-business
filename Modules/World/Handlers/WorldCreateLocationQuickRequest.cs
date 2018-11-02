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
using BeforeOurTime.Models.Modules.Core.Models.Items;

namespace BeforeOurTime.Business.Modules.World.Managers
{
    public partial class LocationItemManager
    {
        /// <summary>
        /// Create location
        /// </summary>
        /// <param name="message"></param>
        /// <param name="origin">Item that initiated request</param>
        /// <param name="mm">Module manager</param>
        /// <param name="response"></param>
        public IResponse HandleCreateLocationQuickRequest(
            IMessage message,
            Item origin,
            IModuleManager mm,
            IResponse response)
        {
            var request = message.GetMessageAsType<WorldCreateLocationQuickRequest>();
            response = HandleRequestWrapper<WorldCreateLocationQuickResponse>(request, res =>
            {
                var itemManager = mm.GetManager<IItemManager>();
                var location = itemManager.Read(origin.ParentId.Value);
                var createFromLocationItemId = request.FromLocationItemId ??
                                               origin.ParentId.Value;
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
