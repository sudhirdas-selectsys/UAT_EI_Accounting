using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.QBO.Journal
{
    public class DeleteJournalReq
    {
        public string Id { get; set; }
        public string SyncToken { get; set; }
    }

    public class DeleteJournalResponse
    {
        public JournalEntryInfo JournalEntry { get; set; }
        public DateTime time { get; set; }
        public Fault Fault { get; set; }
    }
    public class JournalEntryInfo
    {
        public string domain { get; set; }
        public string status { get; set; }
        public string Id { get; set; }
    }
    public class Error
    {
        public string Message { get; set; }
        public string Detail { get; set; }
        public string code { get; set; }
    }

    public class Fault
    {
        public List<Error> Error { get; set; }
        public string type { get; set; }
    }

}
