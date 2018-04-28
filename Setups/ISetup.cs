using System;
using System.Collections.Generic;
using BeforeOurTime.Repository.Models.Items;
using Microsoft.Extensions.Configuration;

namespace BeforeOurTime.Business.Setups
{
    public interface ISetup
    {
        List<Item> Items(IConfigurationRoot configuration, IServiceProvider serviceProvider);
    }
}
