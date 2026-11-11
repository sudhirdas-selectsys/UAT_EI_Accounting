using Accounting.DBUtilities.Implementations.Lookups;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using System.Data;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class InstallmentPlansRepository : IInstallmentPlansRepository
    {
        public List<CarrierDetails> GetAllCarriers()
        {
            return SQLHelper.GetObjectList(new CarrierDetails(), "USP_Accounting_GetAllCarrier");
        }
        public List<CarrierDetails> GetAllCarriersByClient_Id(long Client_Id, bool hasUniqueCarrierNames = false)
        {
            SqlParameter[] spParams =
                      [
                    new("@Client_Id",Client_Id),
                ];

            return hasUniqueCarrierNames ? SQLHelper.GetObjectList(new CarrierDetails(), "USP_Accounting_GetAllCarrierByClient_Id_V1", spParams) :
                SQLHelper.GetObjectList(new CarrierDetails(), "USP_Accounting_GetAllCarrierByClient_Id", spParams);
        }
        public LU_PaymentPlans GetPaymentPlanBy_PaymentCode(string PaymentCode,long Client_Id,long CarrierDetail_Id, DateTime EndorsementDate, string TransactionType, long OriginalSubmission_Id, long PreviousSubmission_Id, string policyNumber)
        {
            SqlParameter[] spParams =
                      [
                    new("@PaymentCode",PaymentCode),
                    new("@Client_Id",Client_Id),
                    new("@CarrierDetail_Id",CarrierDetail_Id),
                    new("@EndorsementDate", EndorsementDate),
                    new("@TransactionType", TransactionType),
                    new("@OriginalSubmission_Id", OriginalSubmission_Id),
                    new("@PreviousSubmission_Id", PreviousSubmission_Id),
                    new("@PolicyNumber", policyNumber)
                ];
            return SQLHelper.GetObject(new LU_PaymentPlans(), "USP_Accounting_GetPaymentPlanByPaymentCode", spParams);
        }
        public LU_PaymentPlans GetPaymentPlanBy_PaymentCode(string PaymentCode,long Client_Id)
        {
            SqlParameter[] spParams =
                      [
                    new("@PaymentCode",PaymentCode),
                    new("@Client_Id",Client_Id),
                ];
            return SQLHelper.GetObject(new LU_PaymentPlans(), "USP_Accounting_GetPaymentPlanByCode", spParams);
        }

        public List<PolicyPaymentSchedule> InsertUpdateGeneratedInvoices(SqlTransaction sqlTransaction, List<PolicyPaymentSchedule> scheduledInvoices, long client_Id)
        {
            try
            {
                long policyDetailId = scheduledInvoices.Select(x => x.PolicyDetails_Id).First();


                string checkQuery = "SELECT 1 FROM Accounting_PolicyPaymentSchedule WHERE PolicyDetail_Id = @PolicyDetail_Id";
                object result = SQLHelper.ExecuteScalar(sqlTransaction,CommandType.Text,checkQuery,new SqlParameter("@PolicyDetail_Id", policyDetailId));


                if (result != null)
                {
                    SQLHelper.ExecuteNonQuery(sqlTransaction,CommandType.StoredProcedure,"USP_Accounting_DeleteScheduledInvoicesByPolicyDetailId",
                        new SqlParameter("@PolicyDetails_Id", policyDetailId));
                }


                foreach (var invoice in scheduledInvoices)
                {
                    SqlParameter[] spParams =
                    [
                        new SqlParameter{
                        ParameterName = "@PolicyPaymentSchedule_Id",
                        SqlDbType = SqlDbType.BigInt,
                        Direction = ParameterDirection.Output
                        },
                        new SqlParameter(){ParameterName="@InvoiceCreatedOn",Value= DBNull.Value,Direction=ParameterDirection.Output,SqlDbType=SqlDbType.DateTime },
                        new SqlParameter("@PolicyDetail_Id", invoice.PolicyDetails_Id),
                        new SqlParameter("@PaymentPlan_Id", (object)invoice.PaymentPlan_Id ?? DBNull.Value),
                        new SqlParameter("@InstallmentNo", (object)invoice.InstallmentNo ?? DBNull.Value),
                        new SqlParameter("@IsDownPayment", invoice.IsDownPayment),
                        new SqlParameter("@DueDate",invoice.DueDate ),
                        new SqlParameter("@BillDate", invoice.BillDate),
                        new SqlParameter("@CreatedOn",invoice.BillDate),
                        new SqlParameter("@Amount", invoice.Amount),
                        new SqlParameter("@Notes", (object)invoice.Notes ?? DBNull.Value),
                        new SqlParameter("@Paid", invoice.Paid),
                        new SqlParameter("@PaidOn", (object)invoice.PaidOn ?? DBNull.Value),
                        new SqlParameter("@Sequence", invoice.Sequence),
                        new SqlParameter("@Client_Id", client_Id)
                    ];

                    SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertUpdateScheduledInvoices", spParams);
                    invoice.PolicyPaymentSchedule_Id = Convert.ToInt64(spParams.Where(c => c.ParameterName == "@PolicyPaymentSchedule_Id").FirstOrDefault().Value);
                    invoice.CreatedOn = Convert.ToDateTime(spParams.Where(c => c.ParameterName == "@InvoiceCreatedOn").FirstOrDefault().Value);

                }
                return scheduledInvoices;
            }
            catch (Exception ex)
            {
                sqlTransaction.Rollback();
                return [];
            }
        }

        public List<SelectedInvoiceDetailDTO> GetPendingCancellationInvoiceDetails(long Client_Id, DateTime CancellationDate, long OriginalSubmission_Id, double cancellationAmount)
        {
            SqlParameter[] spParams =
            [
               new("@Client_Id", Client_Id),
               new("@CancellationDate", CancellationDate),
               new("@OriginalSubmission_Id", OriginalSubmission_Id),
               new("@CancellationAmount", cancellationAmount)
            ];
            return SQLHelper.GetObjectList(new SelectedInvoiceDetailDTO(), "USP_Accounting_GetPendingCancellationInvoiceDetails", spParams);

        }
    }
}
