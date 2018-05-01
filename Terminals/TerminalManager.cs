using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace BeforeOurTime.Business.Terminals
{
    public class TerminalManager
    {
        public List<Terminal> Terminals { set; get; }
        public TerminalManager()
        {

        }
        /// <summary>
        /// Send a message to a specific terminal
        /// </summary>
        /// <param name="terminalId">Unique terminal identifier</param>
        /// <param name="message">Raw message</param>
        public void SendToTerminalId(Guid terminalId, string message)
        {
            Terminals.FirstOrDefault(x => x.Id == terminalId).SendToTerminal(message);
        }
    }
}
