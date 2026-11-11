using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;
using Microsoft.Data.SqlClient;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface IInstallmentPlansRepository
    {
        List<CarrierDetails> GetAllCarriers();
        List<CarrierDetails> GetAllCarriersByClient_Id(long Client_Id, bool hasUniqueCarrierNames = false);
        LU_PaymentPlans GetPaymentPlanBy_PaymentCode(string PaymentCode, long Client_Id,long CarrierDetail_Id, DateTime EndorsementDate, string TransactionType, long OriginalSubmission_Id, long PreviousSubmission_Id, string policyNumber);
        LU_PaymentPlans GetPaymentPlanBy_PaymentCode(string PaymentCode, long Client_Id);
        List<PolicyPaymentSchedule> InsertUpdateGeneratedInvoices(SqlTransaction sqlTransaction, List<PolicyPaymentSchedule> scheduledInvoices, long client_Id);
        List<SelectedInvoiceDetailDTO> GetPendingCancellationInvoiceDetails(long Client_Id, DateTime CancellationDate, long OriginalSubmission_Id, double cancellationAmount);
    }
}
