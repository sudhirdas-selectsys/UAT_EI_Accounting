using Accounting.SchedulerJobs.Models;

namespace Accounting.SchedulerJobs.Abstract
{
    public interface IEmailAttachmentService
    {
        Task<List<EmailAttachmentItem>> BuildAttachmentsAsync<T>(List<SchedulerJobEmailAttachments> attachmentConfigs, T data, byte[] clientLogo = null);
        Task<object> BuildInvoiceNoticeAttachmentDataAsync(long clientId, string agentAddress, byte[] clientLogo);
    }
}
