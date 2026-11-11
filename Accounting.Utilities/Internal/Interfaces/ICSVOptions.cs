using Accounting.Entities.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Utilities.Internal.Interfaces
{
    public interface ICSVOptions
    {
        public List<DocumentFileInfo> DownloadCSV(DataSet dataSet);
    }
}
