using BeforeOurTime.Business.Apis;
using BeforeOurTime.Business.Apis.Scripts.Engines;
using Jint;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeforeOurTime.Business.Apis.Scripts.Libraries
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
        public JsFunctionManager(
            IConfigurationRoot configuration,
            IServiceProvider serviceProvider)
        {
            Configuration = configuration;
            ServiceProvider = serviceProvider;
            Api = ServiceProvider.GetService<IApi>();
        }
        public void AddJsFunctions(IScriptEngine engine)
        {
            var interfaceType = typeof(IScriptLib);
            var jsFuncClasses = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => Activator.CreateInstance(x, Configuration, ServiceProvider, Api, engine))
                .ToList();
            jsFuncClasses
                .ForEach(x => ((IScriptLib)x).AddFunctions());
        }
    }
}
