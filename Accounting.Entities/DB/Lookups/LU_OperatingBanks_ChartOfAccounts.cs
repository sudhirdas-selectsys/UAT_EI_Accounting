using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Lookups
{
    public class LU_OperatingBanks_ChartOfAccounts : BaseLookupEntity
    {
        public long OperatingBanks_ChartOfAccount_Id { get; set; }
        public int OperatingBank_Id { get; set; }
        public int ChartOfAccounts_Id { get; set; }
        //mot Mapped
        public string BankName { get; set; }

    }
}
