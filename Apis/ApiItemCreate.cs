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
        /// Create a new item
        /// </summary>
        /// <param name="Source">Item responsible for doing the creating</param>
        /// <param name="newParent">Item which will become the parent</param>
        /// <param name="child">Item which is the new child being created</param>
        public bool ItemCreate(Item source, Item parent, Item child)
        {
            var created = false;
            List<JsEventRegistration> missingHandlers = JsEventManager.MissingEvent(child);
            if (missingHandlers.Count == 0)
            {
                ItemMove(source, parent, child);
                created = true;
            } else
            {
                Console.WriteLine(child.Id + " is missing event handlers for : " + missingHandlers.ToString());
            }
            return created;
        }
    }
}
