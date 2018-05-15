using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Messages;
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
        public void SendMessage(Message message, List<Item> recipients)
        {
            if (message.SenderId != null)
            {
                var messages = new List<Message>();
                recipients.ForEach(delegate (Item recipient)
                {
                    var messageCopy = (Message)message.Clone();
                    messageCopy.RecipientId = recipient.Id;
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
