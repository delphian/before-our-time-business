using BeforeOurTime.Business.Apis.IO.Updates.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Terminals.Telnet.TranslateIOUpdates
{
    public interface ITranslateIOUpdate
    {
        /// <summary>
        /// Translate structured data from the environment to pure text
        /// </summary>
        /// <param name="environmentUpdate">Update from the environment</param>
        void Translate(IIOUpdate environmentUpdate);
    }
}
