using BeforeOurTime.Business.Apis.Items;
using BeforeOurTime.Models.Apis;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules.Core;
using BeforeOurTime.Models.Modules.Core.Messages.ItemGraph;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Core.Models.Properties;
using BeforeOurTime.Models.Terminals;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Modules.Core
{
    public partial class CoreModule
    {
        /// <summary>
        /// Handle a message
        /// </summary>
        /// <param name="api"></param>
        /// <param name="message"></param>
        /// <param name="terminal"></param>
        /// <param name="response"></param>
        private IResponse HandleCoreReadItemGraphRequest(IMessage message, IApi api, ITerminal terminal, IResponse response)
        {
            var request = message.GetMessageAsType<CoreReadItemGraphRequest>();
            response = HandleRequestWrapper<CoreReadItemGraphResponse>(request, res =>
            {
                var itemId = request.ItemId ??
                    api.GetModuleManager().GetModule<ICoreModule>().GetDefaultGame().Id;
                var item = api.GetItemManager().Read(itemId).GetAsItem<GameItem>();
                var itemGraph = new ItemGraph()
                {
                    Id = item.Id,
                    Name = item.Visible?.Name ?? "N/A"
                };
                BuildItemGraph(api.GetItemManager(), itemGraph);
                ((CoreReadItemGraphResponse)res).CoreReadItemGraphEvent = new CoreReadItemGraphEvent()
                {
                    ItemGraph = itemGraph
                };
                res.SetSuccess(true);
            });
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
