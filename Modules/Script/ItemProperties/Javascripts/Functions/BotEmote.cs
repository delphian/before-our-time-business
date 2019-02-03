using BeforeOurTime.Models.Messages;
using BeforeOurTime.Models.Modules;
using BeforeOurTime.Models.Modules.Core.Managers;
using BeforeOurTime.Models.Modules.Core.Models.Items;
using BeforeOurTime.Models.Modules.Script.ItemProperties.Javascripts;
using BeforeOurTime.Models.Modules.World.Messages.Emotes;
using Jint;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Modules.Script.ItemProperties.Javascripts.Functions
{
    public class BotEmote : IJavascriptFunction
    {
        /// <summary>
        /// Manage all modules
        /// </summary>
        private IModuleManager ModuleManager { set; get; }
        /// <summary>
        /// Description of this function
        /// </summary>
        private JavascriptFunctionDefinition Definition { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="moduleManager"></param>
        public BotEmote(IModuleManager moduleManager)
        {
            ModuleManager = moduleManager;
            Definition = new JavascriptFunctionDefinition()
            {
                Global = true,
                Prototype = "void botEmote(int emoteType, string message = null)",
                Description = "Send out emote from current item. 100 = Smile, 200 = Frown, 300 = Speak, 400 = Raw.",
                Example = @"botEmote(300, ""Hello World"");"
            };
            ModuleManager.ModuleManagerReadyEvent += () =>
            {
                ModuleManager.GetManager<IJavascriptItemDataManager>().AddFunctionDefinition(Definition);
                var jsEngine = ModuleManager.GetManager<IJavascriptItemDataManager>().GetJSEngine();
                Action<int, string> botEmote = (type, parameter) =>
                {
                    Item item = (Item)jsEngine.GetValue("me").ToObject();
                    IMessage emote = new WorldEmoteEvent()
                    {
                        Origin = item,
                        EmoteType = (WorldEmoteType)type,
                        Parameter = parameter
                    };
                    ModuleManager.GetManager<IMessageManager>().SendMessageToSiblings(new List<IMessage>() { emote }, item, item);
                };
                jsEngine.SetValue("botEmote", botEmote);
            };
        }
    }
}
