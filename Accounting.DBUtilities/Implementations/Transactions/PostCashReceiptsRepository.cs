using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class PostCashReceiptsRepository : IPostCashReceiptsRepository
    {
        public PostCashReceipts_Batches InsertPostCashReceipts_Batches(PostCashReceipts_Batches postCashReceipts_Batches)
        {
            SqlParameter[] sp =
            {
                new SqlParameter(){Direction = ParameterDirection.InputOutput, Value = postCashReceipts_Batches.Batch_Id, ParameterName = "@Batch_Id"},
                new SqlParameter("@IncomingRequest_Id",postCashReceipts_Batches.IncomingRequest_Id),
                new SqlParameter("@BatchReference_Id",postCashReceipts_Batches.BatchReference_Id),
                new SqlParameter("@Service_Id",postCashReceipts_Batches.Service_Id),
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_InsertPostCashReceipts_Batches", sp);
            postCashReceipts_Batches.Batch_Id = Convert.ToInt64(sp.FirstOrDefault(z => z.ParameterName == "@Batch_Id").Value);
            return postCashReceipts_Batches;
        }
        public PostCashReceipts_PaymentTransactions InsertPostCashReceipts_PaymentTransactions(PostCashReceipts_PaymentTransactions postCashReceipts_PaymentTransactions)
        {
            SqlParameter[] sqlParameters =
            {
                new SqlParameter(){Direction = ParameterDirection.InputOutput, Value = postCashReceipts_PaymentTransactions.PaymentTransaction_Id, ParameterName = "@PaymentTransaction_Id"},
                new SqlParameter("@Batch_Id",postCashReceipts_PaymentTransactions.Batch_Id),
                new SqlParameter("@TransactionReference_Id",postCashReceipts_PaymentTransactions.TransactionReference_Id),
                new SqlParameter("@ProcessorReference_Id",postCashReceipts_PaymentTransactions.ProcessorReference_Id),
                new SqlParameter("@TransactionDate",postCashReceipts_PaymentTransactions.TransactionDate),
                new SqlParameter("@AccountNumber",postCashReceipts_PaymentTransactions.AccountNumber),
                new SqlParameter("@TransactionAmount",postCashReceipts_PaymentTransactions.TransactionAmount),
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_InsertPostCashReceipts_PaymentTransactions", sqlParameters);
            postCashReceipts_PaymentTransactions.PaymentTransaction_Id = Convert.ToInt64(sqlParameters.FirstOrDefault(z => z.ParameterName == "@PaymentTransaction_Id").Value);
            return postCashReceipts_PaymentTransactions;
        }
        public PostCashReceipts_PaidInvoices InsertPostCashReceipts_PaidInvoices(PostCashReceipts_PaidInvoices postCashReceipts_PaidInvoices)
        {
            SqlParameter[] sqlParameters =
            {
                new SqlParameter(){Direction = ParameterDirection.InputOutput, Value = postCashReceipts_PaidInvoices.PaidInvoice_Id, ParameterName = "@PaidInvoice_Id" },
                new SqlParameter("@PaymentTransaction_Id",postCashReceipts_PaidInvoices.PaymentTransaction_Id),
                new SqlParameter("@Invoice_Id",postCashReceipts_PaidInvoices.Invoice_Id),
                new SqlParameter("@PaidAmount",postCashReceipts_PaidInvoices.PaidAmount),
                new SqlParameter("@Comment",postCashReceipts_PaidInvoices.Comment),
                new SqlParameter("@Division",postCashReceipts_PaidInvoices.Division),
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_InsertPostCashReceipts_PaidInvoices", sqlParameters);
            postCashReceipts_PaidInvoices.PaidInvoice_Id = Convert.ToInt64(sqlParameters.FirstOrDefault(z => z.ParameterName == "@PaidInvoice_Id").Value);
            return postCashReceipts_PaidInvoices;
        }
    }
}
