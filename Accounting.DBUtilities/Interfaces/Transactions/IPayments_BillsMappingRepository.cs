using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface IPayments_BillsMappingRepository
    {
        Payments_BillsMapping GetPayments_BillsMapping(long Payment_Id);
        Payments_BillsMapping InsertPayments_BillsMapping(SqlTransaction Transaction, Payments_BillsMapping Payments_BillsMapping);
        ExternalBillReferenceLineItems GetExternalReferenceLineItems(long Entity_Id, byte EntityType_Id, long Invoice_Id, long InvoiceLineItem_Id, long LineItem_Id, byte Direction_Id, long Client_Id);
    }
}
