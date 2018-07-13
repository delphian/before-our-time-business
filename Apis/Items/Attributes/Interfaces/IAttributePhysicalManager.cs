﻿using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Items.Attributes;
using BeforeOurTime.Models.Primitives.Images;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Items.Attributes.Interfaces
{
    public interface IAttributePhysicalManager : IAttributeManager<AttributePhysical>, IAttributeManager
    {
        /// <summary>
        /// Attach new physical attributes to an existing item
        /// </summary>
        /// <param name="item">Existing item that has already been saved</param>
        /// <param name="name">One, two, or three word short description of item</param>
        /// <param name="description">A long description of the item. Include many sensory experiences</param>
        /// <param name="height">Volume</param>
        /// <param name="weight">Weight</param>
        AttributePhysical Attach(
            Item item,
            string name,
            string description,
            int height,
            int weight);
        /// <summary>
        /// Create new item with new physical attributes
        /// </summary>
        /// <param name="parent">Parent item</param>
        /// <param name="name">One, two, or three word short description of item</param>
        /// <param name="description">A long description of the item. Include many sensory experiences</param>
        /// <param name="volume">Volume</param>
        /// <param name="weight">Weight</param>
        AttributePhysical Create(
            Item parent,
            string name,
            string description,
            int volume,
            int weight);
        /// <summary>
        /// Update the physical name
        /// </summary>
        /// <param name="id">Unique phsyical attribute identifier</param>
        /// <param name="description">New name of the physical</param>
        /// <returns></returns>
        AttributePhysical UpdateName(
            Guid id, 
            string name);
        /// <summary>
        /// Update the physical description
        /// </summary>
        /// <param name="id">Unique phsyical attribute identifier</param>
        /// <param name="description">New description of the physical</param>
        /// <returns></returns>
        AttributePhysical UpdateDescription(
            Guid id, 
            string description);
        /// <summary>
        /// Update the physical attribute's image icon
        /// </summary>
        /// <param name="id">Unique phsyical attribute identifier</param>
        /// <param name="image">Wrapper for image that allows meta data</param>
        /// <returns></returns>
        AttributePhysical UpdateImageIcon(
            Guid id,
            Image image);
        /// <summary>
        /// Update the physical volume
        /// </summary>
        /// <param name="id">Unique phsyical attribute identifier</param>
        /// <param name="height">New height of the physical</param>
        /// <returns></returns>
        AttributePhysical UpdateHeight(
            Guid id, 
            int height);
        /// <summary>
        /// Update the physical weight
        /// </summary>
        /// <param name="id">Unique phsyical attribute identifier</param>
        /// <param name="weight">New weight of the physical</param>
        /// <returns></returns>
        AttributePhysical UpdateWeight(
            Guid id,
            int weight);
    }
}
