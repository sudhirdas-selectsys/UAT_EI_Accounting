using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;
using Microsoft.Data.SqlClient;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class TransactionHistoryRepository : ITransactionHistoryRepository
    {
        public List<LookupEntity> GetAllPayorOrPayeeDetailsByName(string SearchText)
        {
            SqlParameter[] sqlParameter =
            {
                new SqlParameter("@Name", SearchText)
            };
            return SQLHelper.GetObjectList(new LookupEntity(), "USP_Accounting_GetAllPayorOrPayeeDetailsByName", sqlParameter);
        }
        public List<LookupEntity> GetAllPolicyNumbersByNumber(string SearchText, long Client_Id)
        {
            SqlParameter[] sqlParameter =
            {
                new SqlParameter("@PolicyNumber", SearchText),
                new SqlParameter("@Client_Id", Client_Id)
            };
            return SQLHelper.GetObjectList(new LookupEntity(), "USP_Accounting_GetAllPolicyNumbersByNumber", sqlParameter);
        }
        public List<LookupEntity> GetAllInvoiceNumbersByNumber(string SearchText, long Client_Id)
        {
            SqlParameter[] sqlParameter =
            {
                new SqlParameter("@InvoiceNumber", SearchText),
                new SqlParameter("@Client_Id", Client_Id)
            };
            return SQLHelper.GetObjectList(new LookupEntity(), "USP_Accounting_GetAllInvoiceNumbersByNumber", sqlParameter);
        }
        public List<TransactionHistory> GetAllTransactionHistory(TransactionHistorySearch TransactionHistorySearch, int PageSize, int PageNumber, ref long TotalRecords)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@Direction_Id", TransactionHistorySearch.Direction_Id == 0 ? null : TransactionHistorySearch.Direction_Id),
                    new SqlParameter("@EntityName", TransactionHistorySearch.EntityName),
                    new SqlParameter("@TransactionDate", TransactionHistorySearch.TransactionDate),
                    new SqlParameter("@PaymentType_Id", TransactionHistorySearch.PaymentType_Id == 0 ? null : TransactionHistorySearch.PaymentType_Id ),
                    new SqlParameter("@Amount_From", TransactionHistorySearch.Amount_From == 0 ? null : TransactionHistorySearch.Amount_From),
                    new SqlParameter("@Amount_To", TransactionHistorySearch.Amount_To == 0 ? null : TransactionHistorySearch.Amount_To),
                    new SqlParameter("@PaymentStatus_Id", TransactionHistorySearch.PaymentStatus_Id == 0 ? null : TransactionHistorySearch.PaymentStatus_Id),
                    new SqlParameter("@PolicyNumber", TransactionHistorySearch.PolicyNumber),
                    new SqlParameter("@InvoiceNumber", TransactionHistorySearch.InvoiceNumber),
                    new SqlParameter("@Client_Id", TransactionHistorySearch.Client_Id),
                    new SqlParameter("@Payment_Id", TransactionHistorySearch.Payment_Id),
                    new SqlParameter("@PageSize",PageSize),
                    new SqlParameter("@PageNumber",PageNumber),
                    new SqlParameter(){ ParameterName="@TotalRecords",Value=TotalRecords,Direction=ParameterDirection.InputOutput}
                };
                var result = SQLHelper.GetObjectList(new TransactionHistory(), "USP_Accounting_GetTransactionHistorySearchResults", parameters);
                TotalRecords = Convert.ToInt64(parameters.Where(c => c.ParameterName == "@TotalRecords")?.FirstOrDefault()?.Value);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public TransactionHistory GetTransactionHistoryDetails(long PaymentApplicationLineItem_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@PaymentApplicationLineItem_Id",PaymentApplicationLineItem_Id)
            };
            return SQLHelper.GetObject(new TransactionHistory(), "USP_Accounting_GetTransactionHistoryDetails", parameters);
        }
    }
}
