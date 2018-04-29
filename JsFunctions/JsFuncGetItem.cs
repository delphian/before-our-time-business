using BeforeOurTime.Repository.Models.Items;
using Jint;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeforeOurTime.Business.JsFunctions
{    
    public class JsFuncGetItem : IJsFunc
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
            var itemRepo = serviceProvider.GetService<IItemRepo<Item>>();
            // Box some repository functionality into safe limited javascript functions
            Func<string, Item> getItem = delegate (string uuid)
            {
                return itemRepo.ReadUuid(new List<Guid>() { new Guid(uuid) }).FirstOrDefault();
            };
            jsEngine.SetValue("getItem", getItem);
        }
    }
}
