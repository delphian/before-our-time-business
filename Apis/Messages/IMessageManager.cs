using BeforeOurTime.Business.Apis.Terminals;
using BeforeOurTime.Models.Items;
using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Messages.Requests;
using BeforeOurTime.Models.Messages.Responses;
using BeforeOurTime.Repository.Models.Messages.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Messages
{
    /// <summary>
    /// Central environment interface for all things message related
    /// </summary>
    public interface IMessageManager
    {
        /// <summary>
        /// Send a message to multiple recipient items
        /// </summary>
        /// <param name="message"></param>
        /// <param name="recipients"></param>
        /// <param name="senderId"></param>
        void SendMessage(IMessage message, List<Item> recipients, Guid senderId);
        /// <summary>
        /// Distribute a message to all items at a location
        /// </summary>
        /// <param name="message">Message to be delivered</param>
        /// <param name="location">Location item, including children, where message is to be delivered</param>
        /// <param name="actorId">Initiator of the message</param>
        void SendMessageToLocation(IMessage message, Item location, Guid actorId);
        /// <summary>
        /// Distribute message to all items at a location of an item's arrival
        /// </summary>
        /// <param name="item">Item that has arrived</param>
        /// <param name="location">Location item, including children, where the item has arrived</param>
        /// <param name="actorId">Initiator of the movement</param>
        void SendArrivalEvent(Item item, Item location, Guid actorId);
        /// <summary>
        /// Distribute message to all items at a location of an item's departure
        /// </summary>
        /// <param name="item">Item that has departed</param>
        /// <param name="location">Location item, including children, where the item has departed</param>
        /// <param name="actorId">Initiator of the movement</param>
        void SendDepartureEvent(Item item, Item location, Guid actorId);
        /// <summary>
        /// Get all messages awaiting deliver and prompty delete from data store
        /// </summary>
        /// <returns></returns>
        List<SavedMessage> CullAllMessages();
        /// <summary>
        /// Forward message requests to the appropriate endpoints
        /// </summary>
        /// <param name="api"></param>
        /// <param name="terminal">Single generic connection used by the environment to communicate with clients</param>
        /// <param name="request">A request from a terminal</param>
        /// <param name="response">A response to the terminal</param>
        IResponse HandleRequest(IApi api, Terminal terminal, IRequest request);
    }
}
