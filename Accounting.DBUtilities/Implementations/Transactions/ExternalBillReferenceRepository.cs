using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;
using System.Data;
namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class ExternalBillReferenceRepository : IExternalBillReferenceRepository
    {
        public ExternalBillReference GetExternalBillReference(long Client_Id, byte Service_Id, byte SourceType_Id, long Source_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Client_Id", Client_Id),
                new SqlParameter("@Service_Id", Service_Id),
                new SqlParameter("@SourceType_Id", SourceType_Id),
                new SqlParameter("@Source_Id", Source_Id),
            };
            return SQLHelper.GetObject(new ExternalBillReference(), "USP_Accounting_GetExternalBillReference", sp);
        }
        public ExternalBillReference InsertExternalBillReference(SqlTransaction Transaction, ExternalBillReference externalBillReference)
        {
            SqlParameter[] sp =
            {
                new SqlParameter(){ParameterName="@ExternalBillReference_Id", Value = externalBillReference.ExternalBillReference_Id, Direction=ParameterDirection.InputOutput},
                new SqlParameter("@Client_Id", externalBillReference.Client_Id),
                new SqlParameter("@Service_Id", externalBillReference.Service_Id),
                new SqlParameter("@SourceType_Id", externalBillReference.SourceType_Id),
                new SqlParameter("@Source_Id", externalBillReference.Source_Id),
                new SqlParameter("@ExternalBillIdentifier", externalBillReference.ExternalBillIdentifier),
            };
            SQLHelper.ExecuteNonQuery(Transaction, CommandType.StoredProcedure, "USP_Accounting_InsertExternalBillReference", sp);
            externalBillReference.ExternalBillReference_Id = Convert.ToInt64(sp.Where(c => c.ParameterName == "@ExternalBillReference_Id").FirstOrDefault().Value);
            return externalBillReference;
        }
        public void InactivePaymentBill(long Payment_Id, ExternalBillReference externalBillReference)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Payment_Id",Payment_Id),
                new SqlParameter("@Bill_Id",externalBillReference.Source_Id),
                new SqlParameter("@SourceType_Id",externalBillReference.SourceType_Id),
                new SqlParameter("@Service_Id",externalBillReference.Service_Id),
                new SqlParameter("@Client_Id",externalBillReference.Client_Id)
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_InactiveBill", sp);
        }

    }
}
