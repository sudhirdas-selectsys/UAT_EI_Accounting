using Accounting.Entities.DB.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface IExcelLogRepository
    {
        public InvoiceFilterFileUploads GetExcelFileById(long id);
        public InvoiceFilterFileUploads InsertInvoiceFilterFileUploads(InvoiceFilterFileUploads invoiceFilterFileUploads);
    }
}
