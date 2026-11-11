using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class ExternalAccountInformationRepository : IExternalAccountInformationRepository
    {
        public ExternalAccountInformation InsertUpdateExternalAccountInformation(ExternalAccountInformation externalAccountInformation)
        {
            SqlParameter[] sp =
            {
                new SqlParameter(){ParameterName="@Id", Value =externalAccountInformation.Id, Direction=ParameterDirection.InputOutput},
                new SqlParameter("@EntityType_Id",externalAccountInformation.EntityType_Id),
                new SqlParameter("@Entity_Id",externalAccountInformation.Entity_Id),
                new SqlParameter("@AccountNumber",externalAccountInformation.AccountNumber),
                new SqlParameter("@Client_Id",externalAccountInformation.Client_Id)
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_InsertUpdateExternalAccountInformation", sp);
            externalAccountInformation.Id = Convert.ToInt64(sp.Where(c => c.ParameterName == "@Id").FirstOrDefault().Value);
            return externalAccountInformation;
        }
        public ExternalAccountInformation GetExternalAccountInformation(long Entity_Id, byte EntityType_Id, long Client_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Client_Id",Client_Id),
                new SqlParameter("@EntityType_Id",EntityType_Id),
                new SqlParameter("@Entity_Id",Entity_Id)
            };
            return SQLHelper.GetObject(new ExternalAccountInformation(), "USP_Accounting_GetExternalAccountInformation", sp);
        }

        public ExternalAccountInformation GetExternalAccountInformationByAccountNumber(string AccountNumber, long Client_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@AccountNumber",AccountNumber),
                new SqlParameter("@Client_Id",Client_Id),
            };
            return SQLHelper.GetObject(new ExternalAccountInformation(), "USP_Accounting_GetExternalAccountInformationByAccountNumber_V1", sp);
        }
    }
}
