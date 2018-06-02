using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO.Updates.Models
{
    /// <summary>
    /// Communicate the existance of a potentail location exit
    /// </summary>
    public class IOExitUpdate : IOUpdate
    {
        /// <summary>
        /// Unique exit attribute identifier
        /// </summary>
        public Guid ExitId { set; get; }
        /// <summary>
        /// Name of the exit
        /// </summary>
        public string Name { set; get; }
        /// <summary>
        /// Description of exit
        /// </summary>
        public string Description { set; get; }
    }
}
