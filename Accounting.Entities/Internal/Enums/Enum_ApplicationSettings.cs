using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.Internal.Enums
{
    public class Enum_ApplicationSettings
    {
        public Enum_ApplicationSettings()
        {
            EnableAgentStatementCCEmail = "EnableAgentStatementCCEmail";
            AgentStatementCCEmail = "AgentStatementCCEmail";
            EnableAgentStatementBCCEmail = "EnableAgentStatementBCCEmail";
            AgentStatementBCCEmail = "AgentStatementBCCEmail";
            RedirectAgentStatementToEmails = "RedirectAgentStatementToEmails";
            AgentStatementToEmail = "AgentStatementToEmail";
        }
        public string EnableAgentStatementCCEmail { get; set; }
        public string AgentStatementCCEmail { get; set; }
        public string EnableAgentStatementBCCEmail { get; set; }
        public string AgentStatementBCCEmail { get; set; }
        public string RedirectAgentStatementToEmails { get; set; }
        public string AgentStatementToEmail { get; set; }
    }
}
