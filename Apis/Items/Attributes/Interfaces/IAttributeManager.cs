﻿using BeforeOurTime.Business.Apis.Scripts.Libraries;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Items.Attributes;
using BeforeOurTime.Repository.Models;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Items.Attributes;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Interfaces
{
    public interface IAttributeManager
    {
        /// <summary>
        /// Deliver a message to an item
        /// </summary>
        /// <remarks>
        /// Often results in the item's script executing and parsing the message package
        /// </remarks>
        /// <param name="item"></param>
        void DeliverMessage(SavedMessage message, Item item, JsFunctionManager jsFunctionManager);
        /// <summary>
        /// Determine if an item has attributes that may be managed
        /// </summary>
        /// <param name="item">Item that may posses attributes</param>
        bool IsManaging(Item item);
    }
    /// <summary>
    /// Manage details of an item's extended data
    /// </summary>
    public interface IAttributeManager<T> : IAttributeRepository<T> where T: ItemAttribute
    {
        /// <summary>
        /// Create many models with base item
        /// </summary>
        /// <param name="models"></param>
        /// <param name="parentId">Create the base items as children of this parent item</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns></returns>
        List<Item> Create(List<T> models, Guid parentId, TransactionOptions options = null);
        /// <summary>
        /// Create model with base item
        /// </summary>
        /// <param name="model"></param>
        /// <param name="parentId">Create the base items as children of this parent item</param>
        /// <param name="options">Options to customize how data is transacted from datastore</param>
        /// <returns></returns>
        Item Create(T model, Guid parentId, TransactionOptions options = null);
        /// <summary>
        /// Attach new attributes to an existing item
        /// </summary>
        /// <param name="attributes">Unsaved new attributes</param>
        /// <param name="item">Existing item that has already been saved</param>
        /// <returns></returns>
        T Attach(T attributes, Item item);
    }
}
