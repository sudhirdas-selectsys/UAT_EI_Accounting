using Accounting.Entities.DB.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface IServiceLogInfoRepository
    {
        ServiceLogInfo InsertServiceLogInfo(ServiceLogInfo serviceLogInfo);
        ServiceLogInfo GetServiceLogInfo(byte Service_Id, long Client_Id, byte ServiceRequestType_Id);
    }
}
