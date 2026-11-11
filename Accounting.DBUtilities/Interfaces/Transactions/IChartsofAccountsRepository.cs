using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DBUtilities.Interfaces.Transactions
{
    public interface IChartofAccountsRepository
    {
        ChartofAccounts InsertChartofAccounts(ChartofAccounts chartofAccounts);

        List<ChartofAccounts> GetAllChartofAccounts();
        List<ChartofAccounts> ChartofAccountsSuggestions(byte Service_Id, long Client_Id, string SearchValue);
        List<ChartofAccounts> GetChartOfAccountsByService_Id(byte Service_Id, long Client_Id);
        int InactiveChartOfAccounts(long ChartOfAccounts_Id);
        List<ChartofAccounts> ChartofAccountsList(long Client_ID, int Service_Id, int Source_Id);
        void InsertUpdateChartOfAccountsUsage(ChartOfAccountsUsage ChartOfAccountsUsage);
    }

}
