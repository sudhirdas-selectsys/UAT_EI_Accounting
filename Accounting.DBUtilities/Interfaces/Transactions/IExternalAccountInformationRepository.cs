using Accounting.Entities.DB.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface IExternalAccountInformationRepository
    {
        ExternalAccountInformation InsertUpdateExternalAccountInformation(ExternalAccountInformation externalAccountInformation);
        ExternalAccountInformation GetExternalAccountInformation(long Entity_Id, byte EntityType_Id, long Client_Id);
        ExternalAccountInformation GetExternalAccountInformationByAccountNumber(string AccountNumber, long Client_Id);
    }
}
