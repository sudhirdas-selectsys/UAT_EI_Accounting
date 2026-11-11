using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Lookups;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class LU_ClientLevelSageConfigurationsRepository : ILU_ClientLevelSageConfigurationsRepository
    {
        public LU_ClientLevelSageConfigurations GetClientLevelSageConfigurations(long client_Id, byte Action_Id)
        {
            SqlParameter[] sqlParameters = new SqlParameter[]
           {
                new SqlParameter("@Client_Id",client_Id),
                new SqlParameter("@Action_Id",Action_Id)
           };
            return SQLHelper.GetObject(new LU_ClientLevelSageConfigurations(), "USP_Accounting_GetClientLevelSageConfigurations", sqlParameters);
        }
    }
}
