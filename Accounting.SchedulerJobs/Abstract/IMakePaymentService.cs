using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;
using Accounting.SchedulerJobs.Models.MakePayments;

namespace Accounting.SchedulerJobs.Abstract
{
    public interface IMakePaymentService
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
        Task<ServiceResponse> CheckDuplicateMapping(long client_Id, byte direction_Id, List<SelectedInvoice> selectedInvoices, Bill billDetail);
        Task<ServiceResponse> GenerateAndUploadCommissionDocuments(long report_Id, byte fileType_Id, long client_Id, List<SelectedInvoice> selectedInvoices, Bill billDetail, HttpClient httpClient);
        Task<ServiceResponse> CreateBillAndPayment(long client_Id, byte direction_Id, List<SelectedInvoice> selectedInvoices, Bill billDetail, long billProcess_Id);
        Task<ServiceResponse> ApplyPayment(long client_Id, byte direction_Id, List<SelectedInvoice> selectedInvoices, Bill billDetail, ServiceResponse createBillAndPaymentResponse);
        Task<ServiceResponse> CreateVendorAndExternalBill(long client_Id, string User_Id, long Direction_Id, Bill billDetail, ServiceResponse createBillAndPaymentResponse, ServiceResponse applyPaymentResponse, HttpClient httpClient);
        Task<ServiceResponse> UnapplyPayment(long paymentId, long Client_Id, long User_Id,  HttpClient httpClient);
    }
}
