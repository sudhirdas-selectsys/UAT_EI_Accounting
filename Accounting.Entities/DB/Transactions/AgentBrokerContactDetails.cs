using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class AgentBrokerContactDetails :BaseEntity
    {
        public long AgentBrokerContactDetail_Id { get; set; }
        public long AgentBrokerContactReference_Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
       
    }
}
