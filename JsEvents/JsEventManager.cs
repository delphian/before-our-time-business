using BeforeOurTime.Repository.Models.Items;
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
        /// Required Js Event handlers an item of TYPE must implement
        /// </summary>
        protected Dictionary<ItemType, List<JsEventRegistration>> RequiredJsHandlers = new Dictionary<ItemType, List<JsEventRegistration>>();
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
                    .ForEach(delegate (object jsEventClass)
                    {
                        var eventRegistration = ((IJsHandler)jsEventClass).Register();
                        JsEventRegistrations.Add(eventRegistration);
                        ((IJsHandler)jsEventClass).RequiredOn().ForEach(delegate (ItemType itemType)
                        {
                            if (!RequiredJsHandlers.ContainsKey(itemType))
                            {
                                RequiredJsHandlers[itemType] = new List<JsEventRegistration>();
                            }
                            RequiredJsHandlers[itemType].Add(eventRegistration);
                        });
                    });
            }
            return JsEventRegistrations;
        }
        /// <summary>
        /// Required Js Event handlers an item of TYPE must implement
        /// </summary>
        /// <returns></returns>
        public Dictionary<ItemType, List<JsEventRegistration>> GetRequiredJsHandlers()
        {
            if (RequiredJsHandlers.Count == 0)
            {
                GetJsEventRegistrations();
            }
            return RequiredJsHandlers;
        }
    }
}
