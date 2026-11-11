using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class JournalEntryLogRepository : IJournalEntryLogRepository
    {
        public List<JournalEntriesLog> GetJournalEntriesLogByJournalEntry_Id(long JournalEntry_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@JournalEntry_Id", JournalEntry_Id)
            };
            return SQLHelper.GetObjectList(new JournalEntriesLog(), "USP_Accounting_GetJournalEntriesLogByJournalEntry_Id", parameters);
        }
        public JournalEntries InsertJournalEntries(SqlTransaction PaymentsTransaction, JournalEntries journalEntries)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter(){ParameterName="@JournalEntry_Id",Value=journalEntries.JournalEntry_Id, Direction=ParameterDirection.InputOutput },
                new SqlParameter("@SourceType_Id",journalEntries.SourceType_Id),
                new SqlParameter("@Source_Id",journalEntries.Source_Id),
                new SqlParameter("@ExternalJournalEntryIdentifier",journalEntries.ExternalJournalEntryIdentifier),
                new SqlParameter("@JournalIndex",journalEntries.JournalIndex),
                new SqlParameter("@JournalEntryName",journalEntries.JournalEntryName)
            };
            SQLHelper.ExecuteNonQuery(PaymentsTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertJournalEntries", parameters);
            journalEntries.JournalEntry_Id = Convert.ToInt64(parameters.Where(c => c.ParameterName == "@JournalEntry_Id").FirstOrDefault().Value);
            return journalEntries;

        }
        public JournalEntries GetJournalEntry(byte SourceType_Id, long Source_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@SourceType_Id",SourceType_Id),
                new SqlParameter("@Source_Id",Source_Id),
            };
            return SQLHelper.GetObject(new JournalEntries(), "USP_Accounting_GetJournalEntry", parameters);
        }
        public List<JournalEntries> GetJournalEntryByPayment(byte SourceType_Id, long Source_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@SourceType_Id",SourceType_Id),
                new SqlParameter("@Source_Id",Source_Id),
            };
            return SQLHelper.GetObjectList(new JournalEntries(), "USP_Accounting_GetJournalEntry", parameters);
        }
        public JournalEntries GetJournalEntryWithTransaction(SqlTransaction PaymentsTransaction, byte SourceType_Id, long Source_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@SourceType_Id",SourceType_Id),
                new SqlParameter("@Source_Id",Source_Id),
            };
            return SQLHelper.GetObjectWithTransaction(PaymentsTransaction, CommandType.StoredProcedure, new JournalEntries(), "USP_Accounting_GetJournalEntry", parameters);
        }
        public void DeleteJournalEntry(byte SourceType_Id, long Source_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@SourceType_Id",SourceType_Id),
                new SqlParameter("@Source_Id",Source_Id)
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_DeleteJournalEntry", parameters);

        }
        public JournalEntriesLog InsertJournalEntriesLog(SqlTransaction PaymentsTransaction, JournalEntriesLog journalEntriesLog)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter(){ParameterName="@JournalEntryLog_Id",Value=journalEntriesLog.JournalEntryLog_Id, Direction=ParameterDirection.InputOutput },
                new SqlParameter("@JournalEntry_Id", journalEntriesLog.JournalEntry_Id),
                new SqlParameter("@DebitChartOfAccount_Id", journalEntriesLog.DebitChartOfAccount_Id),
                new SqlParameter("@CreditChartOfAccount_Id", journalEntriesLog.CreditChartOfAccount_Id),
                new SqlParameter("@Description", journalEntriesLog.Description),
                new SqlParameter("@Amount", journalEntriesLog.Amount),
                new SqlParameter("@JournalEntryStatus_Id", journalEntriesLog.JournalEntryStatus_Id),
                new SqlParameter("@User_Id", journalEntriesLog.CreatedBy),
            };
            SQLHelper.ExecuteNonQuery(PaymentsTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertJournalEntriesLog", parameters);
            journalEntriesLog.JournalEntryLog_Id = Convert.ToInt64(parameters.Where(c => c.ParameterName == "@JournalEntryLog_Id").FirstOrDefault().Value);
            return journalEntriesLog;
        }
        public void InactiveJournalEntryLogsByJournalEntry_Id(SqlTransaction PaymentsTransaction, long JournalEntry_Id)
        {
            SqlParameter[] parameters =
           {
                new SqlParameter("@JournalEntry_Id",JournalEntry_Id),
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_InactiveJournalEntryLogsByJournalEntry_Id", parameters);
        }
        public JournalEntries InsertJournalEntries(SqlTransaction Transaction, byte SourceType_Id, long Source_Id, string JournalEntryId, int? JournalIndex, string JournalEntryName)
        {
            JournalEntries journalEntries = new JournalEntries();
            journalEntries.SourceType_Id = SourceType_Id;
            journalEntries.Source_Id = Source_Id;
            journalEntries.ExternalJournalEntryIdentifier = JournalEntryId;
            journalEntries.JournalIndex = JournalIndex;
            journalEntries.JournalEntryName = JournalEntryName;
            return InsertJournalEntries(Transaction, journalEntries);
        }
        public int UpdateExternalId(SqlTransaction transaction, long journalEntryId, int externalId)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@JournalEntry_Id", journalEntryId),
                new SqlParameter("@ExternalJournalEntryIdentifier", externalId)
            };

            return SQLHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, "USP_Accounting_UpdateJournalEntryExternalId", parameters.ToArray());
        }
    }
}
