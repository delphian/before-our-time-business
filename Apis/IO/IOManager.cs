using BeforeOurTime.Business.Apis.IO.Requests.Handlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO
{
    /// <summary>
    /// Manage all terminal input request handlers
    /// </summary>
    public class IOManager : IIOManager
    {
        /// <summary>
        /// List of handlers to process terminal input requests
        /// </summary>
        public List<IIORequestHandler> IORequestHandlers = new List<IIORequestHandler>();
    }
}
