using Accounting.SchedulerJobs.Models;

namespace Accounting.SchedulerJobs.Abstract
{
    public interface IInvoiceNoticeRepositoryService
    {
        Task<List<InvoiceNoticeLog>> GetInvoiceNoticeLogs(long client_Id, int noticeType_Id);
        Task InsertInvoiceNoticeLog(InvoiceNoticeLog invoiceNoticeLog);
    }
}
