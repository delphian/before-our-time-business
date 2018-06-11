using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Messages;
using BeforeOurTime.Repository.Models.Messages.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Messages
{
    /// <summary>
    /// Central environment interface for all things message related
    /// </summary>
    public class MessageManager : IMessageManager
    {
        private IMessageRepo MessageRepo { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageRepo"></param>
        public MessageManager(IMessageRepo messageRepo)
        {
            MessageRepo = messageRepo;
        }
        /// <summary>
        /// Send a message to multiple recipient items
        /// </summary>
        /// <param name="message"></param>
        /// <param name="recipients"></param>
        public void SendMessage(SavedMessage message, List<Guid> recipientIds)
        {
            if (message.SenderId != null)
            {
                var messages = new List<SavedMessage>();
                recipientIds.ForEach(delegate (Guid recipientId)
                {
                    var messageCopy = (SavedMessage)message.Clone();
                    messageCopy.RecipientId = recipientId;
                    messages.Add(messageCopy);
                });
                MessageRepo.Create(messages);
            }
            else
            {
                Console.WriteLine("Refusing to send anonymous message");
            }
        }
    }
}
