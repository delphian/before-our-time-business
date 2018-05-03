using BeforeOurTime.Repository.Models.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.JsEvents
{
    /// <summary>
    /// Message type mapping to js onEvent function callback and argument type
    /// </summary>
    public class JsEventManager : IJsEventManager
    {
        protected List<JsEventRegistration> JsEventRegistrations = new List<JsEventRegistration>();
        /// <summary>
        /// Constructor
        /// </summary>
        public JsEventManager()
        {
        }
        /// <summary>
        /// Message type mapping to js onEvent function and argument
        /// </summary>
        public List<JsEventRegistration> GetJsEventRegistrations()
        {
            if (JsEventRegistrations.Count == 0)
            {
                var interfaceType = typeof(IJsHandler);
                var jsEventClasses = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(x => x.GetTypes())
                    .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Select(x => Activator.CreateInstance(x))
                    .ToList();
                jsEventClasses
                    .ForEach(x => JsEventRegistrations.Add(((IJsHandler)x).Register()));
            }
            return JsEventRegistrations;
        }
    }
}
