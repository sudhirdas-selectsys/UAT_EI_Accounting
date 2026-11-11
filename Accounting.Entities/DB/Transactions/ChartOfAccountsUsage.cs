using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class ChartOfAccountsUsage 
    {
        public long Accounting_ChartOfAccountsUsage_Id { get; set; }
        public int ChartOfAccounts_Id { get; set; }
        public byte Source_Id { get; set; }
        public bool Status {  get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
