using Accounting.Entities.Internal;
using Accounting.Utilities.Internal.Interfaces;
using System.Data;
using System.Text;

namespace Accounting.Utilities.Internal.Implementations
{
    public class CSVUtility : ICSVOptions
    {
        public List<DocumentFileInfo> DownloadCSV(DataSet dataSet)
        {
            List<DocumentFileInfo> documentFileInfoList = new List<DocumentFileInfo>();
            StringBuilder sb;
            try
            {
                if (dataSet?.Tables?.Count > 0)
                {
                    foreach (DataTable table in dataSet.Tables)
                    {
                        byte[] bytes = null;
                        sb = new StringBuilder();
                        IEnumerable<string> columnNames = table.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
                        sb.AppendLine(string.Join(",", columnNames));
                        foreach (DataRow row in table.Rows)
                        {
                            IEnumerable<string> fields = row.ItemArray.Select(field =>
                              string.Concat("\"", Convert.ToString(field).Replace("\"", "\"\""), "\""));
                            sb.AppendLine(string.Join(",", fields));
                        }
                        bytes = ASCIIEncoding.ASCII.GetBytes(sb.ToString());
                        DocumentFileInfo fileInfo = new DocumentFileInfo();
                        fileInfo.Bytes = bytes;
                        fileInfo.Name = $"{table.TableName}.csv";
                        documentFileInfoList.Add(fileInfo);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return documentFileInfoList;
        }
    }
}
