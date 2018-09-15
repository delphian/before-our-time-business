using BeforeOurTime.Business.Dbs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Modules
{
    public interface IModule
    {
        /// <summary>
        /// Get repositories declared by the module
        /// </summary>
        /// <returns></returns>
        List<ICrudDataRepository> GetRepositories();
        /// <summary>
        /// Initialize module
        /// </summary>
        /// <param name="repositories"></param>
        void Initialize(List<ICrudDataRepository> repositories);
    }
}
