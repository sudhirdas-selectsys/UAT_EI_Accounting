using Accounting.SchedulerJobs.Abstract;
using Accounting.SchedulerJobs.Models;
using Accounting.SchedulerJobs.Utilities;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace Accounting.SchedulerJobs.RepositoryServices
{
    public class InvoiceNoticeRepositoryService : IInvoiceNoticeRepositoryService
    {
        public async Task<List<InvoiceNoticeLog>> GetInvoiceNoticeLogs(long client_Id, int noticeType_Id)
        {
            try
            {
                FileLogger.LogInformation($"InvoiceNoticeRepositoryService: GetInvoiceNoticeLogs Started");
                SqlParameter[] parameters =
                {
                    new SqlParameter("@Client_Id", client_Id),
                    new SqlParameter("@NoticeType_Id", noticeType_Id),
                };

                return await SQLHelper.GetObjectList(new InvoiceNoticeLog(), "USP_GetClientSchedulerInvoiceNoticeLogs", parameters);
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"InvoiceNoticeRepositoryService: GetInvoiceNoticeLogs Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }

        public async Task InsertInvoiceNoticeLog(InvoiceNoticeLog invoiceNoticeLog)
        {
            try
            {
                FileLogger.LogInformation($"InvoiceNoticeRepositoryService: InsertInvoiceNoticeLog Started");
                SqlParameter[] parameters =
                {
                    new SqlParameter("@CreatedOrUpdatedBy", invoiceNoticeLog.CreatedOrUpdatedBy),
                    new SqlParameter("@Job_Id", invoiceNoticeLog.Job_Id),
                    new SqlParameter("@SchedulerClientJob_Id", invoiceNoticeLog.SchedulerClientJob_Id),
                    new SqlParameter("@NoticeType_Id", invoiceNoticeLog.NoticeType_Id),
                    new SqlParameter("@Client_Id", invoiceNoticeLog.Client_Id),
                    new SqlParameter("@PolicyDetails_Id", invoiceNoticeLog.PolicyDetails_Id),
                    new SqlParameter("@Invoice_Id", invoiceNoticeLog.Invoice_Id),
                    new SqlParameter("@InvoiceNo", invoiceNoticeLog.InvoiceNo),
                    new SqlParameter("@SentOn", invoiceNoticeLog.SentOn),
                    new SqlParameter("@IsEmailSent", invoiceNoticeLog.IsEmailSent),
                    new SqlParameter("@EI_Document_Id", invoiceNoticeLog.EI_Document_Id),
                    new SqlParameter("@ErrorMessage", (object?)invoiceNoticeLog.ErrorMessage ?? DBNull.Value)
                };
                string query = "USP_InsertSchedulerInvoiceNoticeLogs";
                await SQLHelper.ExecuteNonQuery(CommandType.StoredProcedure, query, parameters);
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"InvoiceNoticeRepositoryService: InsertInvoiceNoticeLog Exception: {ex.ToString()}  Message: {ex.Message} InsertInvoiceNoticeLog: {JsonConvert.SerializeObject(invoiceNoticeLog)}");
                throw;
            }
        }
    }
}
