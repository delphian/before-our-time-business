using BeforeOurTime.Models.Messages.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Models.Modules.World.Managers;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.Terminal.Models;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.World.Messages.Emotes.PerformEmote;
using BeforeOurTime.Models.Modules.World.Messages.Emotes;

namespace BeforeOurTime.Business.Modules.World
{
    public partial class WorldModule
    {
        /// <summary>
        /// Perform an emote
        /// </summary>
        /// <param name="message"></param>
        /// <param name="origin">Item that initiated request</param>
        /// <param name="mm">Module manager</param>
        /// <param name="response"></param>
        public IResponse HandlePerformEmoteRequest(
            IMessage message,
            Item origin,
            IModuleManager mm,
            IResponse response)
        {
            var request = message.GetMessageAsType<WorldPerformEmoteRequest>();
            response = HandleRequestWrapper<WorldPerformEmoteResponse>(request, res =>
            {
                var messageManager = mm.GetManager<IMessageManager>();
                var itemManager = mm.GetManager<IItemManager>();
                var location = itemManager.Read(origin.ParentId.Value);
                var emoteEvent = new WorldEmoteEvent()
                {
                    Origin = origin,
                    EmoteType = request.EmoteType,
                    Destination = null
                };
                messageManager.SendMessage(emoteEvent, location.Children, origin);
                ((WorldPerformEmoteResponse)res).EmoteEvent = emoteEvent;
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
