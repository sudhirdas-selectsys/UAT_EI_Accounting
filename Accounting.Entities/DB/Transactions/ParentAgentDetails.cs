using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class ParentAgentDetails : BaseEntity
    {
        public long ParentAgentDetail_Id { get; set; }
        public string AgentReference_Id { get; set; }
        public string Email { get; set; }
        public string FEIN { get; set; }
        public string EI_Accounting_Id { get; set; }
        public string? AgentAccountNumber { get; set; }
        public string? AgentRoutingNumber { get; set; }
        public string? SalesRep { get; set; }
        public long AssignedSalesRep_Id { get; set; }
    }
}
