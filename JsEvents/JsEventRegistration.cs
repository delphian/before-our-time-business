using BeforeOurTime.Repository.Models.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.JsEvents
{
    /// <summary>
    /// Message type to javascript event handler and handler argument
    /// </summary>
    public class JsEventRegistration
    {
        /// <summary>
        /// Item message type that initiates js event handler
        /// </summary>
        public MessageType MessageType { set; get; }
        /// <summary>
        /// Name of the javascript function to invoke
        /// </summary>
        public string JsFunction { set; get; }
        /// <summary>
        /// C# Class type to pass as js function argument
        /// </summary>
        public Type JsArgument { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="jsFunction">Name of the javascript function to invoke</param>
        /// <param name="jsArgument">C# Class type to pass as js function argument</param>
        public JsEventRegistration (MessageType messageType, string jsFunction, Type jsArgument) {
            MessageType = messageType;
            JsFunction = jsFunction;
            JsArgument = jsArgument;
        }
    }
}
