using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.QBO.Journal
{
    public class GetAllJournalEntrieRes
    {
        public QueryResponse QueryResponse { get; set; }
        public DateTime time { get; set; }

    }
    public class JournalEntry
    {
        public bool Adjustment { get; set; }
        public string domain { get; set; }
        public bool sparse { get; set; }
        public string Id { get; set; }
        public string SyncToken { get; set; }
        public MetaData MetaData { get; set; }
        public string DocNumber { get; set; }
        public string TxnDate { get; set; }
        public CurrencyRef CurrencyRef { get; set; }
        public List<JournalEntryLine> Line { get; set; }
    }
    public class Entity
    {
        public string Type { get; set; }
        public EntityRef EntityRef { get; set; }
    }

    public class EntityRef
    {
        public string value { get; set; }
        public string name { get; set; }
    }


    public class LinkedTxn
    {
        public string TxnId { get; set; }
        public string TxnType { get; set; }
    }
}
