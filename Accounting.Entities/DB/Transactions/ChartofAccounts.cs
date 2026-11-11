using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;

namespace Accounting.Entities.DB.Transactions
{
    public class ChartofAccounts : BaseEntity
    {
        public int ChartOfAccounts_Id { get; set; }
        public string Code { get; set; }
        public string AccountType { get; set; }
        public string AccountSubType { get; set; }
        public string Classicfication { get; set; }
        public string ReferenceID { get; set; }
        public byte Service_Id { get; set; }
        public bool Is_UsedByEI { get; set; }

    }
}
