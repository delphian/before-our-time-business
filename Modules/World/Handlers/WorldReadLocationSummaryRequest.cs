using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Messages.Responses.List;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Modules.World.Messages.Location.ReadLocationSummary;
using BeforeOurTime.Models.Modules.World.Models.Data;
using BeforeOurTime.Models.Modules.World.Models.Items;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.Core.Models.Items;

namespace BeforeOurTime.Business.Modules.World.Managers
{
    public partial class LocationItemManager
    {
        /// <summary>
        /// Read location summary
        /// </summary>
        /// <param name="message"></param>
        /// <param name="origin">Item that initiated request</param>
        /// <param name="mm">Module manager</param>
        /// <param name="response"></param>
        public IResponse HandleReadLocationSummaryRequest(
            IMessage message,
            Item origin,
            IModuleManager mm,
            IResponse response)
        {
            var request = message.GetMessageAsType<WorldReadLocationSummaryRequest>();
            response = HandleRequestWrapper<WorldReadLocationSummaryResponse>(request, res =>
            {
                var itemManager = mm.GetManager<IItemManager>();
                var location = itemManager.Read(origin.ParentId.Value).GetAsItem<LocationItem>();
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
