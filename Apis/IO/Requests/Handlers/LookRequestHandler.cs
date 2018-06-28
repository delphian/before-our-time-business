using BeforeOurTime.Business.Apis.Items.Attributes.Interfaces;
using BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Items.Attributes;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Requests.List;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Messages.Responses.List;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Requests.Handlers
{
    public class LookRequestHandler : IRequestHandler
    {
        public LookRequestHandler()
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
                var player = api.GetItemManager().Read(terminal.PlayerId);
                var location = api.GetItemManager().ReadWithChildren(player.ParentId.Value);
                var ioLocationUpdate = new ListLocationResponse()
                {
                    ResponseSuccess = true,
                    Item = location,
                    Exits = new List<ListExitResponse>()
                };
                // Add exits
                location.Children
                    .Where(x => x.HasAttribute(typeof(AttributeExit))).ToList()
                    .ForEach(delegate (Item item)
                    {
                        var attribute = item.GetAttribute<AttributeExit>();
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
                    .Where(x => x.HasAttribute(typeof(AttributePhysical))).ToList()
                    .ForEach(delegate (Item item)
                    {
                        var attribute = item.GetAttribute<AttributePhysical>();
                        ioLocationUpdate.Adendums.Add($"A {attribute.Name} is here");
                    });
                // Add player items
                location.Children
                    .Where(x => x.HasAttribute(typeof(AttributePlayer))).ToList()
                    .ForEach(delegate (Item item)
                    {
                        var attribute = item.GetAttribute<AttributePlayer>();
                        ioLocationUpdate.Adendums.Add($"{attribute.Name} is standing here");
                    });
                response = ioLocationUpdate;
            }
            return response;
        }
    }
}
