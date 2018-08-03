using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Business.Apis.Messages.RequestEndpoints;
using BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Items.Attributes;
using BeforeOurTime.Models.Items.Attributes.Characters;
using BeforeOurTime.Models.Items.Attributes.Exits;
using BeforeOurTime.Models.Items.Attributes.Physicals;
using BeforeOurTime.Models.Items.Attributes.Players;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Requests.List;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Messages.Responses.List;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Locations.RequestEndpoints
{
    public class ExamineLocationEndpoint : IRequestEndpoint
    {
        public ExamineLocationEndpoint()
        {
        }
        /// <summary>
        /// Register to handle a specific set of IO requests
        /// </summary>
        /// <returns></returns>
        public List<Guid> RegisterForRequests()
        {
            return new List<Guid>()
            {
                ListLocationRequest._Id
            };
        }
        /// <summary>
        /// Handle terminal request
        /// </summary>
        /// <param name="api"></param>
        /// <param name="terminal"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        public IResponse HandleRequest(IApi api, Terminal terminal, IRequest request, IResponse response)
        {
            if (request.GetType() == typeof(ListLocationRequest))
            {
                var player = api.GetItemManager().Read(terminal.PlayerId.Value);
                var location = api.GetItemManager().ReadWithChildren(player.ParentId.Value);
                var ioLocationUpdate = new ListLocationResponse()
                {
                    ResponseSuccess = true,
                    Item = location,
                    Exits = new List<ListExitResponse>()
                };
                // Add exits
                location.Children
                    .Where(x => x.HasAttribute(typeof(ExitAttribute))).ToList()
                    .ForEach(delegate (Item item)
                    {
                        var attribute = item.GetAttribute<ExitAttribute>();
                        ioLocationUpdate.Exits.Add(new ListExitResponse()
                        {
                            ResponseSuccess = true,
                            Item = item,
                            Name = attribute.Name,
                            Description = attribute.Description
                        });
                    });
                // Add physical items
                location.Children
                    .Where(x => x.HasAttribute<PhysicalAttribute>() && !x.HasAttribute<CharacterAttribute>()).ToList()
                    .ForEach(delegate (Item item)
                    {
                        var attribute = item.GetAttribute<PhysicalAttribute>();
                        ioLocationUpdate.Adendums.Add($"A {attribute.Name} is here");
                        ioLocationUpdate.Objects.Add(item);
                    });
                // Add character items
                location.Children
                    .Where(x => x.HasAttribute<CharacterAttribute>() || x.HasAttribute<PlayerAttribute>()).ToList()
                    .ForEach(delegate (Item item)
                    {
                        var attribute = item.GetAttribute<PlayerAttribute>();
                        ioLocationUpdate.Adendums.Add($"{attribute.Name} is standing here");
                        ioLocationUpdate.Characters.Add(item);
                    });
                response = ioLocationUpdate;
            }
            return response;
        }
    }
}
