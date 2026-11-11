using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface IExternalBillReferenceRepository
    {
        ExternalBillReference GetExternalBillReference(long Client_Id, byte Service_Id, byte SourceType_Id, long Source_Id);
        ExternalBillReference InsertExternalBillReference(SqlTransaction Transaction, ExternalBillReference externalBillReference); 
        void InactivePaymentBill(long Payment_Id, ExternalBillReference externalBillReference);
    }
}
