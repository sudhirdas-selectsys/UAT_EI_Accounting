using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.SchedulerJobs.Models.MakePayments;
using Microsoft.Data.SqlClient;

namespace Accounting.SchedulerJobs.Abstract
{
    public interface IMakePaymentRepositoryService
    {
        Task<List<BillProcessQueue>> GetPendingBillProcessQueue();
        Task<BillProcessQueue> GetBillProcessQueue(long billProcess_Id, long client_Id);
        Task<List<BillProcessQueueEvents>> GetPendingBillProcessQueueEvents(long billProcessId, long clientId, DateTime? transactionDate = null);
        Task<List<BillProcessQueueEvents>> GetCompletedBillProcessQueueEvents(long billProcessId, long clientId, DateTime? transactionDate = null);
        Task<string> UpdateBillProcessWorkFlowEvent(long billProcessId, long clientId, byte workFlowStatusId, long workFlowEventId, byte workFlowEventTypeId, string? serviceResponseMessage = null, string? processedBy = null);
        Task<string> MoveToNextWorkFlowEvent(long billProcessId, long clientId, long? payment_Id, string? processedBy = null);
        Task<string> UpdateBillProcessQueue(long billProcessId, long clientId, byte billProcessStatusId, byte currentWorkFlowEventTypeId, string? errorMessage = null, string? processedBy = null);
        Task<List<BillProcessRetryConfiguration>> GetBillProcessRetryConfiguration();
        Task<string> SetBillProcessEventRetry(long billProcessId, long clientId, byte workFlowStatusId, long workFlowEventId, byte workFlowEventTypeId, byte delayMinutes, string? serviceResponseMessage = null, string? processedBy = null);
        Task<string> FailBillProcess(long billProcessId, long clientId, long workFlowEventId, byte workFlowEventTypeId, string? serviceResponseMessage = null, string? processedBy = null);
        Task<Payments_BillsMapping> GetPayments_BillsMapping(long Payment_Id);
        Task<Payments_BillsMapping> InsertPayments_BillsMapping(SqlTransaction Transaction, Payments_BillsMapping Payments_BillsMapping);
        Task<ExternalBillReferenceLineItems> GetExternalReferenceLineItems(long Entity_Id, byte EntityType_Id, long Invoice_Id, long InvoiceLineItem_Id, long LineItem_Id, byte Direction_Id, long Client_Id);
        Task<StateDetails> GetStateDetailsByName(string Name, long Client_Id);
        Task<StateDetails> InsertUpdateStateDetails(SqlTransaction sqlTransaction, StateDetails stateDetails);
        Task<Bill> InsertUpdateBill(SqlTransaction sqlTransaction, Bill bill);
        Task<LineItemBill> InsertUpdateBillLineItems(SqlTransaction sqlTransaction, LineItemBill lineItemBills);
        Task<Payments> InsertUpdatePayments(SqlTransaction sqlTransaction, Payments payments);
        Task<Clients> GetClients(long Client_Id);
        Task<Payments> GetPaymentDetails(long payment_Id);
        Task<Invoices> GetInvoicesByInvoice_Id(long Invoice_Id);
        Task<PolicyDetails> GetPolicyDetails(long Id);
        Task<AgentDetails> GetAgentDetails(long AgentDetail_Id);
        Task<List<LU_BillingTypes>> GetAllBillingTypes();
        Task<LU_Clients_AccountingTypes_BillingTypes> GetClients_AccountingTypes_BillingTypes(long Client_Id, byte BillingType_Id);
        Task<PaymentApplications> InsertUpdatePaymentApplications(SqlTransaction sqlTransaction, PaymentApplications paymentApplications);
        Task<List<InvoiceDetails>> GetInvoiceDetailsByDirection_Id(long Invoice_Id, byte Direction_Id);
        Task<PaymentApplicationLineItems> InsertUpdatePaymentApplicationLineItems(SqlTransaction sqlTransaction, PaymentApplicationLineItems paymentApplicationLineItems);
        Task UpdateBalanceInInvoiceDetails(SqlTransaction sqlTransaction, long InvoiceDetail_Id, double RemainingBalance, bool Paid);
        Task UpdatePaymentRemainingAmount(SqlTransaction sqlTransaction, long Payments_Id, double RemainingAmount);
        Task UpdateBalanceInInvoices(SqlTransaction sqlTransaction, long Invoice_Id, double RemainingBalance, bool Paid);
        Task<InvoiceDetails> GetInvoiceDetailsByInvoiceDetail_Id(long? InvoiceDetail_Id);
        Task UpdateBillDetailsForBillProcessQueue(SqlTransaction sqlTransaction, long Client_Id, long BillProcess_Id, long Bill_Id, string BillNumber);



    }
}
