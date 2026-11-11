using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;
using Microsoft.Data.SqlClient;
using Accounting.Entities.External.QBO.Journal;
using Accounting.Entities.External.ChecKeeper;


namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface IPaymentsRepository
    {
        List<Payments> GetPaymentsSearchResults(Payments PaymentsSearch);
        List<Payments> GetPaymentsSearchResults(Payments PaymentsSearch, int PageSize, int PageNumber, string SortOrder,  ref long TotalRecords);
        Payments InsertUpdatePayments(SqlTransaction sqlTransaction, Payments payments);
        Payments GetPaymentDetails(long Payments_Id);
        List<Payments> GetAllPayments(long Client_Id, int PageSize, int PageNumber, ref long TotalRecords);
        void DeletePaymentDetails(long Payments_Id);
        PaymentApplications InsertUpdatePaymentApplications(SqlTransaction sqlTransaction, PaymentApplications paymentApplications);
        PaymentApplicationLineItems InsertUpdatePaymentApplicationLineItems(SqlTransaction sqlTransaction, PaymentApplicationLineItems paymentApplicationLineItems);
        PaymentApplications GetPaymentApplicationsByInvoice_Id(long Invoice_Id);
        PaymentApplicationLineItems GetPaymentApplicationLineItemsByInvoiceLineItem_Id(long InvoiceLineItem_Id);
        List<PaymentApplications> GetPaymentApplicationsByPayment_Id(long Payment_Id);
        List<Payments> GetPaymentsByEntity_Id(long Entity_Id, byte Entitytype_Id);
        void UpdatePaymentRemainingAmount(SqlTransaction sqlTransaction, long Payments_Id, double RemainingAmount);
        List<PaymentApplicationLineItems> GetPaymentApplicationLineItemsByPayment_Id(long Payment_Id);
        List<PaymentApplicationLineItems> GetPaymentApplicationLineItemsForReceiptByPayment_Id(long Payment_Id);
        PaymentApplicationLineItems GetWriteoffPaymentApplicationLineItemsByPayment_Id(long Payment_Id);
        List<PaymentApplicationLineItems> GetPaymentApplicationLineItemsByInvoice_Id(long Invoice_Id, byte Direction_Id);
        List<PaymentApplicationLineItems> GetPaymentApplicationLineItemsByInvoiceDetail_Id(long InvoiceDetail_Id);
        PaymentApplications GetPaymentApplicationsByPaymentApplication_Id(long PaymentApplication_Id);
        void InactivePaymentApplicationLineItems(SqlTransaction sqlTransaction, long PaymentApplicationLineItem_Id);
        List<InvoicePaymentDetails> GetPaymentDetailsByInvoice_Id(long Invoice_Id);
        List<PaymentApplicationLineItems> GetAllPaymentApplicationLineItemsByInvoice_Id(long Invoice_Id);
        List<Payments> GetDeletedPayments(long Client_Id);
        List<PaymentApplicationLineItems> GetAllPaymentApplicationLineItemsByPolicyDetail_Id(long PolicyDetails_Id);
        Clients GetClientByPaymentId(long Payment_Id);
        List<PaymentApplicationLineItems> GetPaymentApplicationLineItemsByPaymentApplication_Id(long PaymentApplication_Id);
        int UpdatePaymentStatus(SqlTransaction sqlTransaction, long PaymentId, byte PaymentStatus_Id, long client_Id);
        List<PaymentApplicationLineItems> ApplyPayments(long Client_Id, string InvoiceDetailsJson, long Payment_Id, byte Direction_Id, bool IsNetNegative);
        List<Error> ValidateInvoiceCOAConfiguration(long Client_Id, string InvoiceDetailIds, byte Service_Id, long Payment_Id);
        string UnApplyPayments(long Client_Id, byte Direction_Id, string PaymentApplicationLineitemJson);
        List<PrintCheckDetails> GetPrintCheckDetails(long paymentId);
        int UpdateCheckPaymentPrintStatus(long Payment_Id);
        List<BillProcessQueueWeb> GetBillProcessQueues(long Client_Id);
        List<BillProcessQueueEventsWeb> GetBillProcessQueuesView(long Client_Id, long BillProcess_Id, DateTime date);
        string RetryBillBatch(long client_Id, long user_Id, long payment_Id, DateTime transactionDate);
        List<Payments> GetAllPaymentsForPrintCheck(long Client_Id, int PageSize, int PageNumber, ref long TotalRecords);
        List<Payments> GetCheckPaymentsSearchResults(Payments PaymentsSearch, int PageSize, int PageNumber, string SortOrder, ref long TotalRecords);
        InvoicePaymentDetails GetInvoiceAppliedPaymentDetails(long Invoice_Id, long Client_Id);
    }
}
