using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accounting.Entities.DB.Transactions;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface IClientServiceDefaultOperatingBankRepository
    {
        ClientServiceDefaultOperatingBank GetClientServiceDefaultOperatingBank(long Client_Id, byte ServiceProviderType_Id, byte Service_Id);
        ClientServiceDefaultOperatingBank insertUpdateClientServiceDefaultOperatingBank(ClientServiceDefaultOperatingBank DefaultOperatingBank);
    }
}
