using BeforeOurTime.Business.Apis;
using Jint;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.JsFunctions
{
    public class JsFunc
    {
        protected IConfigurationRoot Configuration { set; get; }
        protected IServiceProvider ServiceProvider { set; get; }
        protected IApi Api { set; get; }
        protected Engine JsEngine { set; get; }
        /// <summary>
        /// Add a javascript function to the engine for scripts to call
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="serviceProvider"></param>
        /// <aparam name="iApi"></aparam>
        /// <param name="jsEngine"></param>
        public JsFunc(
            IConfigurationRoot configuration,
            IServiceProvider serviceProvider,
            IApi api,
            Engine jsEngine)
        {
            Configuration = configuration;
            ServiceProvider = serviceProvider;
            Api = api;
            JsEngine = jsEngine;
        }
    }
}
