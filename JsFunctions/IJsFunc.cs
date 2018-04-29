using Jint;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.JsFunctions
{
    public interface IJsFunc
    {
        /// <summary>
        /// Add a javascript function to the engine for scripts to call
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="jsEngine"></param>
        void AddFunctions(
            IConfigurationRoot configuration, 
            IServiceProvider serviceProvider, 
            Engine jsEngine);
    }
}
