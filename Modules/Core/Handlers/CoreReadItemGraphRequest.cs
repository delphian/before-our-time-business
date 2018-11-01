using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.Core.Messages.ItemGraph;
using BeforeOurTime.Models.Modules.Core.Models.Properties;
using BeforeOurTime.Models.Modules.Terminal.Models;
using BeforeOurTime.Models.Modules.World;
using BeforeOurTime.Models.Modules.World.Models.Items;
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
        /// <param name="message"></param>
        /// <param name="mm">Module manager</param>
        /// <param name="terminal">Terminal that initiated request</param>
        /// <param name="response"></param>
        private IResponse HandleCoreReadItemGraphRequest(
            IMessage message, 
            IModuleManager mm, 
            ITerminal terminal, 
            IResponse response)
        {
            var request = message.GetMessageAsType<CoreReadItemGraphRequest>();
            response = HandleRequestWrapper<CoreReadItemGraphResponse>(request, res =>
            {
                var itemId = request.ItemId ??
                    mm.GetModule<IWorldModule>().GetDefaultGame().Id;
                var item = mm.GetManager<IItemManager>().Read(itemId).GetAsItem<GameItem>();
                var itemGraph = new ItemGraph()
                {
                    Id = item.Id,
                    Name = item.Visible?.Name ?? "N/A"
                };
                BuildItemGraph(mm.GetManager<IItemManager>(), itemGraph);
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
