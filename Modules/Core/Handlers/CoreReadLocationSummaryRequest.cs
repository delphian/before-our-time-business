using BeforeOurTime.Models;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Messages.Responses.List;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Terminals;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Modules.Account.Messages.Location.ReadLocationSummary;
using BeforeOurTime.Models.Modules.Core.Messages.Location.ReadLocationSummary;
using BeforeOurTime.Models.Modules.Core.Models.Data;

namespace BeforeOurTime.Business.Modules.Core.Managers
{
    public partial class LocationItemManager
    {
        /// <summary>
        /// Read location summary
        /// </summary>
        /// <param name="api"></param>
        /// <param name="message"></param>
        /// <param name="terminal"></param>
        /// <param name="response"></param>
        public IResponse HandleReadLocationSummaryRequest(IMessage message, IApi api, ITerminal terminal, IResponse response)
        {
            var request = message.GetMessageAsType<CoreReadLocationSummaryRequest>();
            response = HandleRequestWrapper<CoreReadLocationSummaryResponse>(request, res =>
            {
                var player = api.GetItemManager().Read(
                    terminal.GetPlayerId().Value,
                    new TransactionOptions() { NoTracking = true });
                var location = api.GetItemManager().Read(player.ParentId.Value).GetAsItem<LocationItem>();
                ((CoreReadLocationSummaryResponse)res).Item = location;
                ((CoreReadLocationSummaryResponse)res).Exits = new List<ListExitResponse>();
                // Add exits
                location.Children
                    .Where(x => x.HasData(typeof(ExitData)))
                    .Select(x => x.GetAsItem<ExitItem>())
                    .ToList()
                    .ForEach(delegate (ExitItem item)
                    {
                        var data = item.GetData<ExitData>();
                        ((CoreReadLocationSummaryResponse)res).Exits.Add(new ListExitResponse()
                        {
                            _requestInstanceId = request.GetRequestInstanceId(),
                            _responseSuccess = true,
                            Item = item,
                            Name = data.Name,
                            Description = data.Description
                        });
                    });
                // Add character items
                location.Children
                    .Where(x => x.HasData<CharacterData>())
                    .Select(x => x.GetAsItem<CharacterItem>())
                    .ToList()
                    .ForEach(item =>
                    {
                        ((CoreReadLocationSummaryResponse)res).Adendums.Add($"{item.Visible.Name} is standing here");
                        ((CoreReadLocationSummaryResponse)res).Characters.Add(item);
                    });
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
