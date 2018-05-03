using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Messages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text;
using BeforeOurTime.Repository.Models;
using BeforeOurTime.Business.JsEvents;

namespace BeforeOurTime.Business.Apis
{
    /// <summary>
    /// Interface into the game
    /// </summary>
    public partial class Api
    {
        /// <summary>
        /// Relocate an item
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="newParent"></param>
        /// <param name="child"></param>
        public void ItemMove(Item Source, Item newParent, Item child)
        {
            // Construct the message
            var message = new Message()
            {
                Version = ItemVersion.Alpha,
                From = Source,
                Type = MessageType.EventItemMove,
                Value = JsonConvert.SerializeObject(new OnItemMove()
                {
                    Type = MessageType.EventItemMove,
                    From = child.Parent,
                    To = newParent,
                    Item = child
                })
            };
            // Move the item
            var oldParent = child.Parent;
            oldParent?.Children.Remove(child);
            newParent.Children.Add(child);
            child.Parent = newParent;
            var updateItems = new List<Item>() { newParent, child };
            if (oldParent != null)
            {
                updateItems.Add(oldParent);
            }
            ItemRepo.Update(updateItems);
            // Distribute message
            var recipients = new List<Item>() { child, newParent };
            if (oldParent != null)
            {
                recipients.Add(oldParent);
            }
            SendMessage(message, recipients);
        }
    }
}
