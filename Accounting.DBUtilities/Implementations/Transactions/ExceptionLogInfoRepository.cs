using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class ExceptionLogInfoRepository : IExceptionLogInfoRepository
    {
        public void InsertExceptionLogInfo(ExceptionLogInfo exceptionLogInfo)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@ErrorLog_Id",exceptionLogInfo.ErrorLog_Id),
                new SqlParameter("@IS_SqlError",exceptionLogInfo.IS_SqlError),
                new SqlParameter("@ErrorStackTrace",exceptionLogInfo.ErrorStackTrace),
                new SqlParameter("@ErrorMessage",exceptionLogInfo.ErrorMessage),
                new SqlParameter("@StoredProcedure",exceptionLogInfo.StoredProcedure),
                new SqlParameter("@UserSessionId",exceptionLogInfo.UserSessionId)
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_InsertExceptionLogInfo", sp);
        }
    }
}
