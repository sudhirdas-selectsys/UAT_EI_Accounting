using Accounting.Entities.External.QBO.Journal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.QBO
{
    public class AccountsResponse
    {
        public QueryResponse QueryResponse { get; set; }
        public DateTime Time { get; set; }
    }

    public class QueryResponse
    {
        public List<JournalEntry> JournalEntry { get; set; }
        public List<Account> Account { get; set; }
        public int StartPosition { get; set; }
        public int MaxResults { get; set; }
        public int totalCount { get; set; }
    }

    public class Account
    {
        public string Name { get; set; }
        public bool SubAccount { get; set; }
        public string FullyQualifiedName { get; set; }
        public bool Active { get; set; }
        public string Classification { get; set; }
        public string AccountType { get; set; }
        public string AccountSubType { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal CurrentBalanceWithSubAccounts { get; set; }
        public CurrencyRef CurrencyRef { get; set; }
        public string Domain { get; set; }
        public bool Sparse { get; set; }
        public string Id { get; set; }
        public string SyncToken { get; set; }
        public MetaData MetaData { get; set; }
    }

    
    public class MetaData
    {
        public DateTime CreateTime { get; set; }
        public DateTime LastUpdatedTime { get; set; }
    }
}
