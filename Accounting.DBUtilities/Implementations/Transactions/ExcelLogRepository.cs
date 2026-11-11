using Microsoft.Data.SqlClient;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class ExcelLogRepository : IExcelLogRepository
    {
        public InvoiceFilterFileUploads GetExcelFileById(long id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@InvoiceFilterFileUploads_Id",id)
            };
            var invoiceFilterFileUploads = SQLHelper.GetObject(new InvoiceFilterFileUploads(), "USP_Accounting_GetExcelFileById", sp);
            return invoiceFilterFileUploads;
        }
        public InvoiceFilterFileUploads InsertInvoiceFilterFileUploads(InvoiceFilterFileUploads invoiceFilterFileUploads)
        {
            SqlParameter[] sp =
            {
                new SqlParameter(){ParameterName="@InvoiceFilterFileUploads_Id",Value=invoiceFilterFileUploads.InvoiceFilterFileUploads_Id, Direction=System.Data.ParameterDirection.InputOutput},
                new SqlParameter("@Document",invoiceFilterFileUploads.Document),
                new SqlParameter("@Filename",invoiceFilterFileUploads.Filename),
                new SqlParameter("@CreatedBy",invoiceFilterFileUploads.CreatedBy)
            };
            SQLHelper.ExecuteNonQuery(System.Data.CommandType.StoredProcedure,"USP_Accounting_InsertInvoiceFilterFileUploads",sp);
            invoiceFilterFileUploads.InvoiceFilterFileUploads_Id = Convert.ToInt64(sp.Where(p => p.ParameterName == "@InvoiceFilterFileUploads_Id").FirstOrDefault().Value);
             return invoiceFilterFileUploads;
        }
    }
}
