using BeforeOurTime.Business.Apis.Items.Attributes;
using BeforeOurTime.Business.Apis.Items.Attributes.Games;
using BeforeOurTime.Business.Apis.Items.Attributes.Locations;
using BeforeOurTime.Business.Apis.Messages.RequestEndpoints;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.ItemAttributes;
using BeforeOurTime.Models.ItemAttributes.Characters;
using BeforeOurTime.Models.ItemAttributes.Players;
using BeforeOurTime.Models.Items.Games;
using BeforeOurTime.Models.ItemProperties.Visible;
using BeforeOurTime.Models.Messages.CRUD.Items.CreateItem;
using BeforeOurTime.Models.Messages.CRUD.Items.ReadItem;
using BeforeOurTime.Models.Messages.CRUD.Items.ReadItemGraph;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Requests.List;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Messages.Responses.List;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeforeOurTime.Business.Apis.Modules.Game;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Locations.RequestEndpoints
{
    public class ReadItemGraphEndpoint : IRequestEndpoint
    {
        public ReadItemGraphEndpoint()
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
                ReadItemGraphRequest._Id
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
            if (request.GetType() == typeof(ReadItemGraphRequest))
            {
                response = new ReadItemGraphResponse()
                {
                    _requestInstanceId = request.GetRequestInstanceId(),
                };
                try
                {
                    var readItemGraphRequest = request.GetMessageAsType<ReadItemGraphRequest>();
                    var player = api.GetItemManager().Read(terminal.PlayerId.Value);
                    var itemId = readItemGraphRequest.ItemId ??
                        api.GetModuleManager().GetModule<IGameModule>().GetDefaultGame().Id;
                    var item = api.GetItemManager().Read(itemId).GetAsItem<GameItem>();
                    var itemGraph = new ItemGraph()
                    {
                        Id = item.Id,
                        Name = item.Visible?.Name ?? "N/A"
                    };
                    BuildItemGraph(api.GetItemManager(), itemGraph);
                    ((ReadItemGraphResponse)response)._responseSuccess = true;
                    ((ReadItemGraphResponse)response).ReadItemGraphEvent = new ReadItemGraphEvent()
                    {
                        ItemGraph = itemGraph
                    };
                }
                catch (Exception e)
                {
                    var traverseExceptions = e;
                    while (traverseExceptions != null)
                    {
                        ((ReadItemGraphResponse)response)._responseMessage += traverseExceptions.Message + ". ";
                        traverseExceptions = traverseExceptions.InnerException;
                    }
                    api.GetLogger().Log(LogLevel.Error, ((ReadItemGraphResponse)response)._responseMessage);
                }
            }
            return response;
        }
        /// <summary>
        /// Build an item graph
        /// </summary>
        /// <param name="itemManager"></param>
        /// <param name="itemGraph"></param>
        private void BuildItemGraph(IItemManager itemManager, ItemGraph itemGraph)
        {
            var parentItem = itemManager.Read(itemGraph.Id);
            parentItem.ChildrenIds?.ForEach((itemId) =>
            {
                var item = itemManager.Read(itemId);
                var name = item.GetProperty<VisibleProperty>("Visible")?.Name ?? "N/A";
                var childGraph = new ItemGraph()
                {
                    Id = itemId,
                    Name = name
                };
                BuildItemGraph(itemManager, childGraph);
                itemGraph.Children.Add(childGraph);
            });
        }
    }
}
