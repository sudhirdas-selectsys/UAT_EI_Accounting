using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class JournalEntriesLog: BaseEntity
    {
        public long JournalEntryLog_Id { get; set; }
        public long JournalEntry_Id { get; set; }
        public int? DebitChartOfAccount_Id { get; set; }
        public int? CreditChartOfAccount_Id { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public DateTime EntryDate { get; set; }
        public byte JournalEntryStatus_Id { get; set; }
        public long CreatedBy { get; set; }
        public string JournalEntryStatusName { get; set; }
        public string CreatedByName { get; set; }
        public string DebitCOAName { get; set; }
        public string CreditCOAName { get; set; }
        public string ApplicationNumber { get; set; }
    }
}
