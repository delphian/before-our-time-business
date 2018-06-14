using BeforeOurTime.Business.Apis.Items.Attributes.Interfaces;
using BeforeOurTime.Business.Apis.Scripts.Delegates.OnTerminalInput;
using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Attributes;
using BeforeOurTime.Repository.Models.Items.Attributes.Exits;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Requests;
using BeforeOurTime.Repository.Models.Messages.Requests.Look;
using BeforeOurTime.Repository.Models.Messages.Responses;
using BeforeOurTime.Repository.Models.Messages.Responses.Enumerate;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Requests.Handlers
{
    public class IOLookRequestHandler : IIORequestHandler
    {
        public IOLookRequestHandler()
        {
        }
        /// <summary>
        /// Register to handle a specific set of IO requests
        /// </summary>
        /// <returns></returns>
        public List<string> RegisterForIORequests()
        {
            return new List<string>()
            {
                typeof(LookRequest).ToString()
            };
        }
        /// <summary>
        /// Handle terminal request
        /// </summary>
        /// <param name="api"></param>
        /// <param name="terminal"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        public IResponse HandleIORequest(IApi api, Terminal terminal, IRequest request, IResponse response)
        {
            if (request.GetType() == typeof(LookRequest))
            {
                var playerAttribute = api.GetAttributeManager<IAttributePlayerManager>().Read(terminal.PlayerId);
                var player = api.GetItemManager().Read(playerAttribute.ItemId);
                var location = api.GetItemManager().ReadWithChildren(player.ParentId.Value);
                AttributeLocation locationAttributes = location.GetAttribute<AttributeLocation>();
                var ioLocationUpdate = new LocationResponse()
                {
                    LocationId = locationAttributes.Id,
                    Name = locationAttributes.Name,
                    Description = locationAttributes.Description,
                    Exits = new List<ExitResponse>()
                };
                // Add exits
                location.Children
                    .Where(x => x.HasAttribute(typeof(AttributeExit))).ToList()
                    .ForEach(delegate (Item item)
                    {
                        var attribute = item.GetAttribute<AttributeExit>();
                        ioLocationUpdate.Exits.Add(new ExitResponse()
                        {
                            ExitId = attribute.Id,
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
