﻿using BeforeOurTime.Business.Apis.Scripts;
using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Items.Attributes.Players;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Events.Arrivals;
using BeforeOurTime.Models.Messages.Events.Departures;
using BeforeOurTime.Repository.Models.Messages.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeforeOurTime.Business.Apis.Messages
{
    /// <summary>
    /// Central environment interface for all things message related
    /// </summary>
    public class MessageManager : IMessageManager
    {
        private IMessageRepo MessageRepo { set; get; }
        private IScriptManager ScriptManager { set; get; }
        private ITerminalManager TerminalManager { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageRepo"></param>
        public MessageManager(
            IMessageRepo messageRepo,
            IScriptManager scriptManager,
            ITerminalManager terminalManager)
        {
            MessageRepo = messageRepo;
            ScriptManager = scriptManager;
            TerminalManager = terminalManager;
        }
        /// <summary>
        /// Send a message to multiple recipient items
        /// </summary>
        /// <param name="message"></param>
        /// <param name="recipients"></param>
        /// <param name="senderId"></param>
        public void SendMessage(IMessage message, List<Item> recipients, Guid senderId)
        {
            var savedMessage = new SavedMessage()
            {
                SenderId = senderId,
                DelegateId = ScriptManager.GetDelegateDefinition("onArrival").GetId(),
                Package = JsonConvert.SerializeObject(message)
            };
            recipients.ForEach(delegate (Item recipient)
            {
                var playerAttribute = recipient.GetAttribute<PlayerAttribute>();
                if (playerAttribute != null)
                {
                    var terminalId = TerminalManager.GetTerminals()
                        .Where(x => x.PlayerId == recipient.Id)
                        .Select(x => x.Id)
                        .FirstOrDefault();
                    if (terminalId != Guid.Empty)
                    {
                        TerminalManager.SendToTerminalId(terminalId, message);
                    }
                } else
                {
                    var messageCopy = (SavedMessage)savedMessage.Clone();
                    messageCopy.RecipientId = recipient.Id;
                    MessageRepo.Create(messageCopy);
                }
            });
        }
        /// <summary>
        /// Distribute a message to all items at a location
        /// </summary>
        /// <param name="message">Message to be delivered</param>
        /// <param name="location">Location item, including children, where message is to be delivered</param>
        /// <param name="actorId">Initiator of the message</param>
        public void SendMessageToLocation(IMessage message, Item location, Guid actorId)
        {
            var recipients = new List<Item>() { location };
            recipients.AddRange(location.Children);
            SendMessage(message, recipients, actorId);
        }
        /// <summary>
        /// Distribute message to all items at a location of an item's arrival
        /// </summary>
        /// <param name="item">Item that has arrived</param>
        /// <param name="location">Location item, including children, where the item has arrived</param>
        /// <param name="actorId">Initiator of the movement</param>
        public void SendArrivalEvent(Item item, Item location, Guid actorId)
        {
            var name = item.Name;
            if (item.HasAttribute<PlayerAttribute>())
            {
                name = item.GetAttribute<PlayerAttribute>().Name;
            }
            SendMessageToLocation(new ArrivalEvent()
                {
                    Item = item,
                    Name = name
                }
                , location, actorId);
        }
        /// <summary>
        /// Distribute message to all items at a location of an item's departure
        /// </summary>
        /// <param name="item">Item that has departed</param>
        /// <param name="location">Location item, including children, where the item has departed</param>
        /// <param name="actorId">Initiator of the movement</param>
        public void SendDepartureEvent(Item item, Item location, Guid actorId)
        {
            var name = item.Name;
            if (item.HasAttribute<PlayerAttribute>())
            {
                name = item.GetAttribute<PlayerAttribute>().Name;
            }
            SendMessageToLocation(new DepartureEvent()
                {
                    Name = name,
                    Item = item
                }
                , location, actorId);
        }
        /// <summary>
        /// Get all messages awaiting deliver and prompty delete from data store
        /// </summary>
        /// <returns></returns>
        public List<SavedMessage> CullAllMessages()
        {
            List<SavedMessage> messages = MessageRepo.Read();
            MessageRepo.Delete();
            return messages;
        }
    }
}
