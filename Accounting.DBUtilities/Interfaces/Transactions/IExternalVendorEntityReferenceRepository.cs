using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface IExternalVendorEntityReferenceRepository
    {
        ExternalVendorEntityReference GetExternalVendorEntityReference(long Client_Id, byte EntityType_Id, long Entity_Id, byte Service_Id);
        ExternalVendorEntityReference InsertExternalVendorEntityReference(SqlTransaction Transaction, ExternalVendorEntityReference externalVendorEntityReference);
    }
}
