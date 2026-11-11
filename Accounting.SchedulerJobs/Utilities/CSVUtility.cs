using Accounting.SchedulerJobs.Models;
using System.Data;
using System.Text;

namespace Accounting.SchedulerJobs.Utilities
{
    public static class CSVUtility
    {
        public static List<EmailAttachmentItem> DownloadCSV(DataSet dataSet, string fileName)
        {
            List<EmailAttachmentItem> emailAttachmentItems = new();

            try
            {
                FileLogger.LogInformation($"CSVUtility: DownloadCSV Started");
                if (dataSet?.Tables?.Count > 0)
                {
                    foreach (DataTable table in dataSet.Tables)
                    {
                        StringBuilder sb = new();
                        IEnumerable<string> columnNames = table.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
                        sb.AppendLine(string.Join(",", columnNames));

                        foreach (DataRow row in table.Rows)
                        {
                            IEnumerable<string> fields = row.ItemArray.Select(field => $"\"{Convert.ToString(field)?.Replace("\"", "\"\"")}\"");
                            sb.AppendLine(string.Join(",", fields));
                        }

                        emailAttachmentItems.Add(new EmailAttachmentItem
                        {
                            FileContent = Encoding.ASCII.GetBytes(sb.ToString()),
                            FileName = $"{fileName}.csv"
                        });
                    }
                }

                return emailAttachmentItems;
            }
            catch(Exception ex)
            {
                FileLogger.LogError($"CSVUtility: DownloadCSV Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }
    }
}
