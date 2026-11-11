using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface ICarrierGroupingDetailsRepository
    {
        CarrierGroupingDetails InsertCarrierGrouping(SqlTransaction SqlTransaction, CarrierGroupingDetails carriergrouping);
    }
}
