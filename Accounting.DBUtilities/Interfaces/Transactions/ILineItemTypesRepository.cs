using Accounting.Entities.DB.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface ILineItemTypesRepository
    {
        LineItemTypes GetAllInvoicesBasedOnLineItemId(long Invoice_Id, int LineItemType_Id);

       
    }
}
