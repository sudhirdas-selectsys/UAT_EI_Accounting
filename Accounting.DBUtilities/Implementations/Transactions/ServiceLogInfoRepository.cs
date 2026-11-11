using Azure;
using Accounting.Entities.DB.Transactions;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Accounting.DBUtilities.Interfaces.Transactions;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class ServiceLogInfoRepository : IServiceLogInfoRepository
    {
        public ServiceLogInfo InsertServiceLogInfo(ServiceLogInfo serviceLogInfo)
        {
            SqlParameter[] sp =
            {
                new SqlParameter(){ParameterName="@ServiceLogInfo_Id",Value=serviceLogInfo.ServiceLogInfo_Id,Direction=ParameterDirection.InputOutput },
                new SqlParameter("@Service_Id", serviceLogInfo.Service_Id),
                new SqlParameter("@Client_Id", serviceLogInfo.Client_Id),
                new SqlParameter("@ServiceRequestType_Id", serviceLogInfo.ServiceRequestType_Id),
                new SqlParameter("@Request", serviceLogInfo.Request),
                new SqlParameter("@Response", serviceLogInfo.Response),
                new SqlParameter("@Is_Success", serviceLogInfo.Is_Success),
                new SqlParameter("@ResponseTime_In_Sec", serviceLogInfo.ResponseTime_In_Sec),
                new SqlParameter("@Message", serviceLogInfo.Message),
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_InsertServiceLogInfo", sp);
            serviceLogInfo.ServiceLogInfo_Id = Convert.ToInt64(sp.Where(c => c.ParameterName == "@ServiceLogInfo_Id")?.FirstOrDefault()?.Value);
            return serviceLogInfo;
        }
        public ServiceLogInfo GetServiceLogInfo(byte Service_Id, long Client_Id, byte ServiceRequestType_Id)
        {
            SqlParameter[] sp =
            {
              new SqlParameter("@Service_Id", Service_Id),
              new SqlParameter("@Client_Id", Client_Id),
              new SqlParameter("@ServiceRequestType_Id", ServiceRequestType_Id),
            };
            return SQLHelper.GetObject(new ServiceLogInfo(), "USP_Accounting_GetServiceLogInfo", sp);
        }


    }
}
