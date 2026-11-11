using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface ITransactionHistoryRepository
    {
        List<LookupEntity> GetAllPayorOrPayeeDetailsByName(string SearchText);
        List<LookupEntity> GetAllPolicyNumbersByNumber(string SearchText, long Client_Id);
        List<LookupEntity> GetAllInvoiceNumbersByNumber(string SearchText, long Client_Id);
        List<TransactionHistory> GetAllTransactionHistory(TransactionHistorySearch search, int PageSize, int PageNumber, ref long TotalRecords);
        TransactionHistory GetTransactionHistoryDetails(long PaymentApplicationLineItem_Id);
    }
}
