using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;

namespace Accounting.DBUtilities.Implementations.Transactions
{
    public class ChartofAccountsRepository : IChartofAccountsRepository
    {
        public List<ChartofAccounts> GetAllChartofAccounts()
        {
            return SQLHelper.GetObjectList(new ChartofAccounts(), "USP_Accounting_GetChartofAccounts");
        }

        public ChartofAccounts InsertChartofAccounts(ChartofAccounts chartofAccounts)
        {
            SqlParameter[] sp =
            {
                new SqlParameter(){ParameterName="@Id",Value=chartofAccounts.Id,Direction=ParameterDirection.InputOutput },
                new SqlParameter("@Name", chartofAccounts.Name),
                new SqlParameter("@Code", chartofAccounts.Code),
                new SqlParameter("@AccountType", chartofAccounts.AccountType),
                new SqlParameter("@AccountSubType", chartofAccounts.AccountSubType),
                new SqlParameter("@classicfication", chartofAccounts.Classicfication),
                new SqlParameter("@ReferenceID", chartofAccounts.ReferenceID),
                new SqlParameter("@Client_Id", chartofAccounts.Client_Id),
                new SqlParameter("@Service_Id", chartofAccounts.Service_Id),
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_InsertChartsOfAccounts", sp);
            chartofAccounts.Id = Convert.ToInt64(sp.Where(c => c.ParameterName == "@Id")?.FirstOrDefault()?.Value);
            return chartofAccounts;
        }


        public List<ChartofAccounts> ChartofAccountsSuggestions(byte Service_Id, long Client_Id, string SearchValue)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Service_Id", Service_Id),
                new SqlParameter("@Client_Id", Client_Id),
                new SqlParameter("@SearchValue", SearchValue),
            };
            return SQLHelper.GetObjectList(new ChartofAccounts(), "USP_Accounting_ChartofAccountsSuggestions", sp);
        }
        public int InactiveChartOfAccounts(long ChartOfAccounts_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@ChartOfAccounts_Id", ChartOfAccounts_Id),
            };
            return SQLHelper.ExecuteNonQuery("USP_Accounting_InactiveChartOfAccounts", sp);
        }
        public List<ChartofAccounts> GetChartOfAccountsByService_Id(byte Service_Id, long Client_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Service_Id", Service_Id),
                new SqlParameter("@Client_Id", Client_Id),
            };
            return SQLHelper.GetObjectList(new ChartofAccounts(), "USP_Accounting_GetChartOfAccountsByService_Id", sp);
        }
        public List<ChartofAccounts> ChartofAccountsList(long Client_ID, int Service_Id, int Source_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Client_ID", Client_ID),
                new SqlParameter("@Service_Id", Service_Id),
                new SqlParameter("@Source_Id", Source_Id),
            };
            return SQLHelper.GetObjectList(new ChartofAccounts(), "USP_Accounting_GetChartofAccountsByClient_IdAndService_Id", sp);
        }
        public void InsertUpdateChartOfAccountsUsage(ChartOfAccountsUsage ChartOfAccountsUsage)
        {
            SqlParameter[] spParams =
            {
                new SqlParameter("@ChartOfAccounts_Id", ChartOfAccountsUsage.ChartOfAccounts_Id),
                new SqlParameter("@Source_Id", ChartOfAccountsUsage.Source_Id),
                new SqlParameter("@Status", ChartOfAccountsUsage.Status),
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_InsertUpdateChartOfAccountsUsage", spParams);
        }
    }
}
