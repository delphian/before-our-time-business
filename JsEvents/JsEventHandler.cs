using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.JsEvents
{
    /// <summary>
    /// Map a message type to the javascript 'on event' function name
    /// and the c# class that should be passed as the function parameter
    /// </summary>
    public class JsEventHandler
    {
        /// <summary>
        /// Name of the javascript function to invoke
        /// </summary>
        public string Function { set; get; }
        /// <summary>
        /// C# Class type to pass as js function parameter
        /// </summary>
        public Type Type { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="function">Name of the javascript function to invoke</param>
        /// <param name="type">C# Class type to pass as js function parameter</param>
        public JsEventHandler (string function, Type type) {
            Function = function;
            Type = type;
        }
    }
}
