using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class ReportFileDownloadSettings : BaseEntity
    {
        public long Report_Id { get; set; }
        public byte FileType_Id { get; set; }


    }
}
