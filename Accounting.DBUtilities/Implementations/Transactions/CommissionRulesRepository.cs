using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;
using Microsoft.Data.SqlClient;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class CommissionRulesRepository : ICommissionRulesRepository
    {
        public List<CommissionRules> GetCommissionRules(int entityType_Id, long entity_Id, long? client_Id = null)
        {
            var sqlParams = new SqlParameter[]
            {
            new SqlParameter("@EntityType_Id", entityType_Id),
            new SqlParameter("@Entity_Id", entity_Id),
            new SqlParameter("@Client_Id", client_Id == null ? DBNull.Value : client_Id),
            };
            return SQLHelper.GetObjectList(new CommissionRules(), "USP_Accounting_GetCommissionRules", sqlParams);
        }
        public List<UnderWriterDetails> GetAllUnderWriters(long clientId)
        {
            SqlParameter[] sqlParameter =
            {
                  new SqlParameter("@Client_Id", clientId)
            };
            return SQLHelper.GetObjectList(new UnderWriterDetails(), "USP_Accounting_GetAllUnderwriters", sqlParameter);
        }

        public List<CommissionRules> GetCommissionRules(int PageNumber, int PageSize, ref int TotalRecords, string? Searchvalue, long Client_Id, byte entityType_Id)
        {
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@Client_Id",Client_Id),
                new SqlParameter("@EntityType_Id", entityType_Id),
                new SqlParameter("@PageNumber",PageNumber),
                new SqlParameter("@PageSize",PageSize),
                new SqlParameter("@SearchValue",Searchvalue),
                new SqlParameter(){ParameterName="@TotalRecords",Direction=ParameterDirection.Output,Size=100 },
            };
            var _Result = SQLHelper.GetObjectList(new CommissionRules(), "USP_Accounting_GetPagedCommissionRules", sqlParams);
            TotalRecords = Convert.ToInt32(sqlParams.Where(x => x.ParameterName == "@TotalRecords")?.FirstOrDefault()?.Value);
            return _Result;
        }

        public CommissionRules GetCommissionRule(long CommissionRule_Id)
        {
            var sqlParam = new SqlParameter[] {
                new SqlParameter("@CommissionRule_Id",CommissionRule_Id)
            };

            return SQLHelper.GetObject(new CommissionRules(), "USP_Accounting_GetCommissionRule", sqlParam);
        }

        public CommissionRules InsertUpdateCommissionRule(SqlTransaction sqlTransaction, CommissionRules commissionRules)
        {
            SqlParameter[] sp =
            {
                new() { ParameterName = "@CommissionRule_Id", Value = commissionRules.CommissionRule_Id, Direction = ParameterDirection.InputOutput },
                new SqlParameter("@Client_Id",commissionRules.Client_Id),
                new SqlParameter("@EntityType_Id",commissionRules.EntityType_Id),
                new SqlParameter("@Entity_Id",commissionRules.Entity_Id),
                new SqlParameter("@SubmissionType_Ids",string.IsNullOrEmpty(commissionRules.SubmissionTypeIds) ? DBNull.Value : commissionRules.SubmissionTypeIds),
                new SqlParameter("@CommissionType_Id",commissionRules.CommissionType_Id),
                new SqlParameter("@Value",commissionRules.Value),
                new SqlParameter("@ApplyOnAllTransactions",commissionRules.ApplyOnAllTransactions),
                new SqlParameter("@CommissionBasisType_Id",commissionRules.CommissionBasisType_Id),
                new SqlParameter("@Lob_Id",commissionRules.LOB_Id),
                new SqlParameter("@ReferralCommissionSplitPct",commissionRules.Referral_Commission_SplitPct)
            };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertUpdateCommissionRule", sp);
            commissionRules.CommissionRule_Id = Convert.ToInt64(sp.Where(c => c.ParameterName == "@CommissionRule_Id").FirstOrDefault().Value);
            return commissionRules;
        }

        public int DeleteCommissionRule(long CommissionRule_Id)
        {
            var sqlParam = new SqlParameter[] {
                new SqlParameter("@CommissionRule_Id",CommissionRule_Id)
            };

            return SQLHelper.ExecuteNonQuery("USP_Accounting_DeleteCommissionRule", sqlParam);
        }

    }
}
