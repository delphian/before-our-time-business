﻿using BeforeOurTime.Business.Apis;
using BeforeOurTime.Business.Apis.Scripts.Engines;
using BeforeOurTime.Business.Terminals;
using BeforeOurTime.Repository.Models.Items;
using BeforeOurTime.Repository.Models.Messages;
using Jint;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeforeOurTime.Business.JsFunctions
{    
    public class JsFuncTerminalMessage : JsFunc, IJsFunc
    {
        public JsFuncTerminalMessage(IConfigurationRoot config, IServiceProvider provider, IApi api, IScriptEngine engine)
            : base(config, provider, api, engine) { }
        /// <summary>
        /// Add a javascript function to the engine for scripts to call
        /// </summary>
        public void AddFunctions()
        {
            var terminalManager = ServiceProvider.GetService<ITerminalManager>();
            // Box some repository functionality into safe limited javascript functions
            Func<Item, string, object, bool> terminalMessage = delegate (
                Item me,
                string terminalId,
                object msgBody)
            {
                var terminal = terminalManager.GetTerminals().Where(x => x.Id.ToString() == terminalId).FirstOrDefault();
                terminal.SendToClient(msgBody.ToString());
                return true;
            };
            Engine
                .SetValue("_terminalMessage", terminalMessage)
                .Execute("var terminalMessage = function(terminalId, msgBody){ return _terminalMessage(me, terminalId.ToString(), msgBody) };");
        }
    }
}
