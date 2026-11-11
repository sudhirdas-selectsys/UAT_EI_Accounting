using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface ICommissionRulesRepository
    {
        List<CommissionRules> GetCommissionRules(int entityType_Id, long entity_Id, long? client_Id = null);
        public List<UnderWriterDetails> GetAllUnderWriters(long clientId);
        List<CommissionRules> GetCommissionRules(int PageNumber, int PageSize, ref int TotalRecords, string? Searchvalue, long Client_Id, byte entityType_Id);
        CommissionRules GetCommissionRule(long CommissionRule_Id);
        CommissionRules InsertUpdateCommissionRule(SqlTransaction sqlTransaction, CommissionRules commissionRules);
        int DeleteCommissionRule(long CommissionRule_Id);
    }
}
