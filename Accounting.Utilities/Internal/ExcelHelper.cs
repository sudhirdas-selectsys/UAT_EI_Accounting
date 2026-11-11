using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Utilities.Internal
{
    public static class ExcelHelper
    {
        public static List<string> GetColumns(Stream excelStream)
        {
            using var workbook = new XLWorkbook(excelStream);
            var worksheet = workbook.Worksheets.First();
            var headerRow = worksheet.FirstRowUsed();
            if (headerRow == null)
                return new List<string>();

            return headerRow.Cells().Select(c => c.GetString().Trim()).Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
        }

        public static List<Dictionary<string, string>> ReadAsDictionaryList(Stream excelStream)
        {
            var rows = new List<Dictionary<string, string>>();

            using var workbook = new XLWorkbook(excelStream);
            var worksheet = workbook.Worksheets.First();

            var headerRow = worksheet.FirstRowUsed();
            if (headerRow == null)
                return rows;

            var headers = headerRow.Cells().Select((c, i) => new { Name = c.GetString().Trim(), Index = i + 1 }).Where(h => !string.IsNullOrWhiteSpace(h.Name)).ToList();

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var header in headers)
                {
                    dict[header.Name] = row.Cell(header.Index).GetValue<string>();
                }

                rows.Add(dict);
            }

            return rows;
        }
    }
}
