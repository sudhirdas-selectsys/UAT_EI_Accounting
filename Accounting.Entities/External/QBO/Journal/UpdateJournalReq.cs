using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.QBO.Journal
{
    public class AccountRef
    {
        public string value { get; set; }
        public string name { get; set; }
    }

    public class JournalEntryLineDetail
    {
        public string PostingType { get; set; }
        public AccountRef AccountRef { get; set; }
    }



    public class TxnTaxDetail
    {
        // Empty object — add properties here if needed in the future
    }

    public class UpdateJournalReq
    {
        public bool Adjustment { get; set; }
        public decimal TotalAmt { get; set; }
        public string domain { get; set; }
        public bool sparse { get; set; }
        public string Id { get; set; }
        public string SyncToken { get; set; }
        public MetaData MetaData { get; set; }
        public string DocNumber { get; set; }
        public string TxnDate { get; set; }
        public CurrencyRef CurrencyRef { get; set; }
        public decimal ExchangeRate { get; set; }
        public List<JournalEntryLine> Line { get; set; }
        public TxnTaxDetail TxnTaxDetail { get; set; }
        public List<LinkedTxn> LinkedTxn { get; set; } // Optional; might be null
        public Fault Fault { get; set; }
    }
    
}
