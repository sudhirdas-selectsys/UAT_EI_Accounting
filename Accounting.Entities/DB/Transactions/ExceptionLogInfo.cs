using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class ExceptionLogInfo : Exception
    {
        public long ErrorLog_Id { get; set; }
        public bool IS_SqlError {  get; set; }
        public string ErrorStackTrace { get; set; }
        public string ErrorMessage { get; set; }
        public string StoredProcedure {  get; set; }
        public long UserSessionId { get; set; }
        //
        public string ErrorName { get; set; }
        public string ErrorDescription { get; set; }
    }
}
