using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Lookups
{
    public class LU_Clients_AccountingTypes_BillingTypes : BaseEntity
    {
        public int Clients_AccountingTypes_BillingType_Id { get; set; }
        public byte BillingType_Id { get; set; }
        public byte AccountingType_Id { get; set; }
    }
}
