using BeforeOurTime.Business.Terminals;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Apis.IO
{
    /// <summary>
    /// Parse input from terminals
    /// </summary>
    public interface IIOInputParser
    {
        /// <summary>
        /// Parse input from terminals
        /// </summary>
        /// <param name="terminalInput"></param>
        void ParseInput(Terminal terminal, string terminalInput);
    }
}
