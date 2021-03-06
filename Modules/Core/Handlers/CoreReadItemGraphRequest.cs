﻿using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.ItemProperties.Visibles;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.Core.Messages.ItemGraph;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.World;
using BeforeOurTime.Models.Modules.World.ItemProperties.Games;
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
        /// <param name="origin">Item that initiated request</param>
        /// <param name="mm">Module manager</param>
        /// <param name="response"></param>
        private IResponse HandleCoreReadItemGraphRequest(
            IMessage message,
            Item origin,
            IModuleManager mm, 
            IResponse response)
        {
            var request = message.GetMessageAsType<CoreReadItemGraphRequest>();
            response = HandleRequestWrapper<CoreReadItemGraphResponse>(request, res =>
            {
                var itemId = request.ItemId ??
                    mm.GetModule<IWorldModule>().GetDefaultGame().Id;
                var item = mm.GetManager<IItemManager>().Read(itemId);
                var itemGraph = new ItemGraph()
                {
                    Id = item.Id,
                    Name = item.GetProperty<VisibleItemProperty>()?.Name ?? "N/A"
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
                var name = item.GetProperty<VisibleItemProperty>()?.Name ?? "N/A";
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
