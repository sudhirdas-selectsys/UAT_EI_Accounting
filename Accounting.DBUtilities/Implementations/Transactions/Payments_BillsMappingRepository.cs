using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class Payments_BillsMappingRepository : IPayments_BillsMappingRepository
    {
        public Payments_BillsMapping GetPayments_BillsMapping(long Payment_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Payment_Id", Payment_Id),
            };
            return SQLHelper.GetObject(new Payments_BillsMapping(), "USP_Accounting_GetPayments_BillsMapping", sp);
        }
        public Payments_BillsMapping InsertPayments_BillsMapping(SqlTransaction Transaction, Payments_BillsMapping Payments_BillsMapping)
        {
            SqlParameter[] sp =
            {
                new SqlParameter(){ParameterName="@PaymentBill_Id", Value = Payments_BillsMapping.PaymentBill_Id, Direction=ParameterDirection.InputOutput},
                new SqlParameter("@Payment_Id", Payments_BillsMapping.Payment_Id),
                new SqlParameter("@Bill_Id", Payments_BillsMapping.Bill_Id),
            };
            SQLHelper.ExecuteNonQuery(Transaction, CommandType.StoredProcedure, "USP_Accounting_InsertPayments_BillsMapping", sp);
            Payments_BillsMapping.PaymentBill_Id = Convert.ToInt64(sp.Where(c => c.ParameterName == "@PaymentBill_Id").FirstOrDefault().Value);
            return Payments_BillsMapping;
        }
        public ExternalBillReferenceLineItems GetExternalReferenceLineItems(long Entity_Id, byte EntityType_Id, long Invoice_Id, long InvoiceLineItem_Id, long LineItem_Id, byte Direction_Id, long Client_Id)
        {
            SqlParameter[] sp =
            {          new SqlParameter("@Entity_Id", Entity_Id),
                       new SqlParameter("@EntityType_Id", EntityType_Id),
                       new SqlParameter("@Invoice_Id", Invoice_Id),
                       new SqlParameter("@InvoiceLineItem_Id", InvoiceLineItem_Id),
                       new SqlParameter("@LineItem_Id", LineItem_Id),
                       new SqlParameter("@Direction_Id", Direction_Id),
                       new SqlParameter("@Client_Id", Client_Id)
                       //,new SqlParameter("@SourceType_Id", SourceType_Id)
             };
            return SQLHelper.GetObject(new ExternalBillReferenceLineItems(), "USP_Accounting_IsExternalBillExists", sp);
        }
    }
}
