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
    public  class ClientServiceDefaultOperatingBankRepository: IClientServiceDefaultOperatingBankRepository
    {
        public ClientServiceDefaultOperatingBank GetClientServiceDefaultOperatingBank(long Client_Id, byte ServiceProviderType_Id, byte Service_Id)
        {
            SqlParameter[] sp =
            {  
                new SqlParameter("@Client_Id",Client_Id),
                new SqlParameter("@ServiceProviderType_Id",ServiceProviderType_Id),
                new SqlParameter("@Service_Id",Service_Id),
            };
            return SQLHelper.GetObject(new ClientServiceDefaultOperatingBank(), "USP_Accounting_GetClientServiceDefaultOperatingBank", sp);
        }
        public ClientServiceDefaultOperatingBank insertUpdateClientServiceDefaultOperatingBank(ClientServiceDefaultOperatingBank defaultOperatingBank)
        {
            SqlParameter[] sp =
            {
                new SqlParameter(){ParameterName="@ClientServiceDefaultOperatingBank_Id",Value = defaultOperatingBank.ClientServiceDefaultOperatingBank_Id,Direction=ParameterDirection.InputOutput},
                new SqlParameter("@Client_Id",defaultOperatingBank.Client_Id),
                new SqlParameter("@ServiceProviderType_Id",defaultOperatingBank.ServiceProviderType_Id),
                new SqlParameter("@Service_Id",defaultOperatingBank.Service_Id),
                new SqlParameter("@OperatingBank_Id",defaultOperatingBank.OperatingBank_Id),
            };
            SQLHelper.GetObject(new ClientServiceDefaultOperatingBank(), "USP_Accounting_InsertUpdateClientServiceDefaultOperatingBank", sp);
            defaultOperatingBank.ClientServiceDefaultOperatingBank_Id = Convert.ToInt64(sp.Where(c => c.ParameterName == "@ClientServiceDefaultOperatingBank_Id").FirstOrDefault().Value);
            return defaultOperatingBank;
        }
    }
}
