using BeforeOurTime.Business.Apis;
using BeforeOurTime.Business.Apis.Scripts.Engines;
using Jint;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.Scripts.Libraries
{
    public class ScriptLib
    {
        protected IConfigurationRoot Configuration { set; get; }
        protected IServiceProvider ServiceProvider { set; get; }
        protected IApi Api { set; get; }
        protected IScriptEngine Engine { set; get; }
        /// <summary>
        /// Add a javascript function to the engine for scripts to call
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="serviceProvider"></param>
        /// <aparam name="iApi"></aparam>
        /// <param name="jsEngine"></param>
        public ScriptLib(
            IConfigurationRoot configuration,
            IServiceProvider serviceProvider,
            IApi api,
            IScriptEngine engine)
        {
            Configuration = configuration;
            ServiceProvider = serviceProvider;
            Api = api;
            Engine = engine;
        }
    }
}
