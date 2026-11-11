using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class JournalEntries :BaseEntity
    {
        public long JournalEntry_Id { get; set; }
        public byte SourceType_Id { get; set; }
        public long Source_Id { get; set; }
        public string ExternalJournalEntryIdentifier { get; set; }
        public int? JournalIndex { get; set; }
        public string JournalEntryName { get; set; }
        public string ReferenceNumber { get; set; }
    }
}
