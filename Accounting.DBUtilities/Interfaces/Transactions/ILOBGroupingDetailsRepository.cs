using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface ILOBGroupingDetailsRepository
    {
        LOBGroupingDetails InsertLOBGrouping(SqlTransaction sqlTransaction, LOBGroupingDetails lobgrouping);
    }
}
