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
                res.SetSuccess(false).SetMessage("Not implemented");
            });
            return response;
        }
    }
}
