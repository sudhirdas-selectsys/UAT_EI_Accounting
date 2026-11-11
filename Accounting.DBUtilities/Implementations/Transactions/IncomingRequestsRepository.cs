using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class IncomingRequestsRepository : IIncomingRequestsRepository
    {
        public IncomingRequests InsertIncomingRequests(IncomingRequests incomingRequests)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter() { Direction = ParameterDirection.InputOutput, Value = incomingRequests.IncomingRequest_Id, ParameterName = "@IncomingRequest_Id" },
                new SqlParameter("@Service_Id", incomingRequests.Service_Id),
                new SqlParameter("@RequestBody", incomingRequests.RequestBody),
                new SqlParameter("@CreatedBy", incomingRequests.CreatedBy)
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_InsertIncomingRequests", parameters);
            incomingRequests.IncomingRequest_Id = Convert.ToInt64(parameters.FirstOrDefault(z => z.ParameterName == "@IncomingRequest_Id").Value);
            return incomingRequests;
        }
    }
}
