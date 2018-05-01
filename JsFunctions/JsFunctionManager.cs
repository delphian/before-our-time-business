using BeforeOurTime.Business.Apis;
using Jint;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.JsFunctions
{
    class JsFunctionManager
    {
        protected IConfigurationRoot Configuration { set; get; }
        protected IServiceProvider ServiceProvider { set; get; }
        protected IApi Api { set; get; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="api"></aparam>
        /// <param name="jsEngine"></param>
        public JsFunctionManager(
            IConfigurationRoot configuration,
            IServiceProvider serviceProvider,
            IApi api)
        {
            Configuration = configuration;
            ServiceProvider = serviceProvider;
            Api = api;
        }
        public void AddJsFunctions(Engine jsEngine)
        {
            var interfaceType = typeof(IJsFunc);
            var jsFuncClasses = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => Activator.CreateInstance(x, Configuration, ServiceProvider, Api, jsEngine))
                .ToList();
            jsFuncClasses
                .ForEach(x => ((IJsFunc)x).AddFunctions());
        }
    }
}
