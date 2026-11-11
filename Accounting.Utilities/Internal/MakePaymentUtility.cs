using Accounting.DBUtilities;
using Accounting.DBUtilities.Implementations.Transactions;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transaction;
using Accounting.Entities.DB.Transactions;
using IMSOnline.DBUtilities.DB.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Utilities.Internal
{
    public class MakePaymentUtility
    {
        private readonly IMakePaymentsRepository _IMakePaymentsRepository = new MakePaymentsRepository();
        
      
    }
}
