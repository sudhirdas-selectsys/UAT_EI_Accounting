using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class ClientsRepository : IClientsRepository
    {
        public Clients GetClients(long Client_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Client_Id",Client_Id)
            };
            return SQLHelper.GetObject(new Clients(), "USP_Accounting_GetClients", parameters);
        }
        public List<Clients> GetAllClientsAssociatedToUser_Id(long User_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@User_Id",User_Id)
            };
            return SQLHelper.GetObjectList(new Clients(), "USP_Accounting_GetAllClientsAssociatedToUser_Id_V1", parameters);
        }
        public void UpdateImage(long Client_Id, byte[] logo)
        {
            SqlParameter[] parameters =
            {
        new SqlParameter("@Client_Id",Client_Id),
        new SqlParameter("@logo",logo)
    };
            string Query = "update Accounting_Clients set Logo = @logo where Client_Id = @Client_Id and Status = 1";
            SQLHelper.ExecuteNonQuery(CommandType.Text, Query, parameters);
        }
        public EI_InstanceNames GetClientIdByEI_InstanceNames(string EI_InstanceName)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Name",EI_InstanceName)
            };
            return SQLHelper.GetObject(new EI_InstanceNames(), "USP_Accounting_GetClientIdByEI_InstanceNames", sp);
        }
    }
}
