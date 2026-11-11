using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class ExternalVendorEntityReferenceRepository : IExternalVendorEntityReferenceRepository
    {
        public ExternalVendorEntityReference GetExternalVendorEntityReference(long Client_Id, byte EntityType_Id, long Entity_Id, byte Service_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Client_Id", Client_Id),
                new SqlParameter("@EntityType_Id", EntityType_Id),
                new SqlParameter("@Entity_Id", Entity_Id),
                new SqlParameter("@Service_Id", Service_Id),
            };
            return SQLHelper.GetObject(new ExternalVendorEntityReference(), "USP_Accounting_GetExternalVendorEntityReference", sp);
        }
        public ExternalVendorEntityReference InsertExternalVendorEntityReference(SqlTransaction Transaction, ExternalVendorEntityReference externalVendorEntityReference)
        {
            SqlParameter[] sp =
            {
                new SqlParameter(){ParameterName="@ExternalEntityReference_Id", Value = externalVendorEntityReference.ExternalEntityReference_Id, Direction=ParameterDirection.InputOutput},
                new SqlParameter("@Client_Id", externalVendorEntityReference.Client_Id),
                new SqlParameter("@EntityType_Id", externalVendorEntityReference.EntityType_Id),
                new SqlParameter("@Entity_Id", externalVendorEntityReference.Entity_Id),
                new SqlParameter("@Service_Id", externalVendorEntityReference.Service_Id),
                new SqlParameter("@ExternalRefId", externalVendorEntityReference.ExternalRefId),
            };
            SQLHelper.ExecuteNonQuery(Transaction, CommandType.StoredProcedure, "USP_Accounting_InsertExternalVendorEntityReference", sp);
            externalVendorEntityReference.ExternalEntityReference_Id = Convert.ToInt64(sp.Where(c => c.ParameterName == "@ExternalEntityReference_Id").FirstOrDefault().Value);
            return externalVendorEntityReference;
        }
    }
}
