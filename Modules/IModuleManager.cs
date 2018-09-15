using BeforeOurTime.Business.Dbs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Modules
{
    /// <summary>
    /// Register through reflection and manage all modules
    /// </summary>
    public interface IModuleManager
    {
        /// <summary>
        /// Get API module that implements interface
        /// </summary>
        /// <typeparam name="T">Interface that API module must implement</typeparam>
        /// <returns>API module if found, otherwise null</returns>
        T GetModule<T>() where T : IModule;
        /// <summary>
        /// Get repository that implements interface
        /// </summary>
        /// <typeparam name="T">Interface that repository must implement</typeparam>
        /// <returns></returns>
        T GetRepository<T>() where T : ICrudDataRepository;
    }
}
