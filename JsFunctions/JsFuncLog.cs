using BeforeOurTime.Repository.Models.Items;
using Jint;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeforeOurTime.Business.JsFunctions
{    
    public class JsFuncLog : IJsFunc
    {
        /// <summary>
        /// Add a javascript function to the engine for scripts to call
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="jsEngine"></param>
        public void AddFunctions(
            IConfigurationRoot configuration, 
            IServiceProvider serviceProvider, 
            Engine jsEngine)
        {
            jsEngine.SetValue("log", new Action<object>(Console.Write));
        }
    }
}
