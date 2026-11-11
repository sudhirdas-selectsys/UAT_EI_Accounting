using Accounting.Entities.DB.Transactions;
using Accounting.Entities.External.QBO.Journal;
using Microsoft.Data.SqlClient;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface IJournalEntryLogRepository
    {
        List<JournalEntriesLog> GetJournalEntriesLogByJournalEntry_Id(long JournalEntry_Id);
        JournalEntries InsertJournalEntries(SqlTransaction PaymentsTransaction, JournalEntries journalEntries);
        JournalEntries GetJournalEntry(byte SourceType_Id, long Source_Id);
        JournalEntries GetJournalEntryWithTransaction(SqlTransaction PaymentsTransaction, byte SourceType_Id, long Source_Id);
        void DeleteJournalEntry(byte SourceType_Id, long Source_Id);
        JournalEntriesLog InsertJournalEntriesLog(SqlTransaction PaymentsTransaction, JournalEntriesLog journalEntriesLog);
        void InactiveJournalEntryLogsByJournalEntry_Id(SqlTransaction PaymentsTransaction, long JournalEntry_Id);
        List<JournalEntries> GetJournalEntryByPayment(byte SourceType_Id, long Source_Id);
        int UpdateExternalId(SqlTransaction transaction, long journalEntryId, int externalId);
    }
}
