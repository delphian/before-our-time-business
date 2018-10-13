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
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Modules.Core.Models.Data;
using BeforeOurTime.Models.Modules.World.Messages.Location.ReadLocationSummary;
using BeforeOurTime.Models.Modules.World.Models.Data;
using BeforeOurTime.Models.Modules.World.Models.Items;

namespace BeforeOurTime.Business.Modules.World.Managers
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
            var request = message.GetMessageAsType<WorldReadLocationSummaryRequest>();
            response = HandleRequestWrapper<WorldReadLocationSummaryResponse>(request, res =>
            {
                var player = api.GetItemManager().Read(
                    terminal.GetPlayerId().Value,
                    new TransactionOptions() { NoTracking = true });
                var location = api.GetItemManager().Read(player.ParentId.Value).GetAsItem<LocationItem>();
                ((WorldReadLocationSummaryResponse)res).Item = location;
                ((WorldReadLocationSummaryResponse)res).Exits = new List<ListExitResponse>();
                // Add exits
                location.Children
                    .Where(x => x.HasData(typeof(ExitData)))
                    .Select(x => x.GetAsItem<ExitItem>())
                    .ToList()
                    .ForEach(delegate (ExitItem item)
                    {
                        var data = item.GetData<ExitData>();
                        ((WorldReadLocationSummaryResponse)res).Exits.Add(new ListExitResponse()
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
                        ((WorldReadLocationSummaryResponse)res).Adendums.Add($"{item.Visible.Name} is standing here");
                        ((WorldReadLocationSummaryResponse)res).Characters.Add(item);
                    });
                res.SetSuccess(true);
            });
            return response;
        }
    }
}
