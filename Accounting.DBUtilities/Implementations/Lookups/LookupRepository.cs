using Accounting.DBUtilities.Interfaces.Lookups;
using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;
using System.Data;
using Accounting.Entities.Internal;

namespace Accounting.DBUtilities.Implementations.Lookups
{
    public class LookupRepository : ILookupRepository
    {

        public List<LU_BillingTypes> GetAllBillingTypes()
        {
            return SQLHelper.GetObjectList(new LU_BillingTypes(), "USP_Accounting_GetAllBillingTypes");
        }
        public List<LU_PaymentPlans> GetAllPaymentPlans()
        {
            return SQLHelper.GetObjectList(new LU_PaymentPlans(), "USP_Accounting_GetAllPaymentPlans");
        }
        public List<LU_Roles> GetAllRoles()
        {
            return SQLHelper.GetObjectList(new LU_Roles(), "USP_Accounting_GetAllRoles");
        }
        public List<LU_Sources> GetAllSources()
        {
            return SQLHelper.GetObjectList(new LU_Sources(), "USP_Accounting_GetAllSources");
        }
        public List<LU_States> GetAllStates()
        {
            return SQLHelper.GetObjectList(new LU_States(), "USP_Accounting_GetAllStates");
        }
        public List<LU_OperatingBanks> GetAllOperatingBanks()
        {
            return SQLHelper.GetObjectList(new LU_OperatingBanks(), "USP_Accounting_LU_GetAllOperatingBanks");
        }
        public List<LU_PaymentTypes> GetAllPaymentTypes()
        {
            return SQLHelper.GetObjectList(new LU_PaymentTypes(), "USP_Accounting_LU_GetAllPaymentTypes");
        }
        public List<LU_PaymentStatus> GetAllPaymentStatus()
        {
            return SQLHelper.GetObjectList(new LU_PaymentStatus(), "USP_Accounting_LU_GetAllPaymentStatus");
        }
        public List<LU_LOBs> GetAllLOBs()
        {
            return SQLHelper.GetObjectList(new LU_LOBs(), "USP_Accounting_GetAllLOBs");
        }


        public List<LU_Directions> GetAllDirections()
        {
            return SQLHelper.GetObjectList(new LU_Directions(), "USP_Accounting_LU_GetAllDirections");
        }
        public List<LU_PaymentDirections> GetAllPaymentDirections()
        {
            return SQLHelper.GetObjectList(new LU_PaymentDirections(), "USP_Accounting_GetAllPaymentDirections");
        }
        public List<LU_LineItemTypes> GetAllLineItemTypes()
        {
            return SQLHelper.GetObjectList(new LU_LineItemTypes(), "USP_Accounting_GetAllLineItemTypes");
        }
        public List<LU_LineItems> GetAllLineItems()
        {
            return SQLHelper.GetObjectList(new LU_LineItems(), "USP_Accounting_GetAllLineItems");
        }
        public List<LU_LineItems_Directions> GetAllLineItems_Directions()
        {
            return SQLHelper.GetObjectList(new LU_LineItems_Directions(), "USP_Accounting_GetAllLineItems_Directions");
        }
        public List<LU_BillingType_LineItems_Directions> GetAllBillingType_LineItems_Directions()
        {
            return SQLHelper.GetObjectList(new LU_BillingType_LineItems_Directions(), "USP_Accounting_GetAllBillingType_LineItems_Directions");
        }

        public List<LU_EntityTypes> GetAllEntityTypes()
        {
            return SQLHelper.GetObjectList(new LU_EntityTypes(), "USP_Accounting_GetAllEntityTypes");
        }
        public List<LU_AnchorTypes> GetAllAnchorTypes()
        {
            return SQLHelper.GetObjectList(new LU_AnchorTypes(), "USP_Accounting_GetAllAnchorTypes");
        }
        public List<LU_InstallmentBasisTypes> GetAllInstallmentBasisTypes()
        {
            return SQLHelper.GetObjectList(new LU_InstallmentBasisTypes(), "USP_Accounting_GetAllInstallmentBasisTypes");
        }
        public List<LU_IntervalUnitTypes> GetAllIntervalUnitTypes()
        {
            return SQLHelper.GetObjectList(new LU_IntervalUnitTypes(), "USP_Accounting_GetAllIntervalUnitTypes");
        }



        //public void InsertLineItems_Directions(LU_LineItems_Directions lU_LineItems_Directions)
        //{
        //    SqlParameter[] sp =
        //     {
        //        new SqlParameter("@LineItems_Directions_Id", lU_LineItems_Directions.LineItems_Directions_Id),
        //        new SqlParameter("@LineItem_Id", lU_LineItems_Directions.LineItem_Id),
        //        new SqlParameter("@Direction_Id", lU_LineItems_Directions.Direction_Id),
        //        //new SqlParameter("@ChartofAccount_Id", lU_LineItems_Directions.ChartofAccount_Id),
        //    };
        //    SQLHelper.ExecuteNonQuery("USP_Update_LU_LineItems_Directions", sp);
        //}
        public List<LU_ClientLevelParameters> GetAllClientLevelParameters(long client_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Client_Id", client_Id),
            };
            return SQLHelper.GetObjectList(new LU_ClientLevelParameters(), "USP_Accounting_LU_GetAllClientLevelParameters", sp);
        }
        public List<LU_Services> GetAllServices()
        {
            return SQLHelper.GetObjectList(new LU_Services(), "USP_Accounting_LU_Services");
        }
        public List<LU_Parameters> GetAllParameters()
        {
            return SQLHelper.GetObjectList(new LU_Parameters(), "USP_Accounting_LU_Parameters");
        }
        public List<LU_ServiceLevelParameters> GetAllServiceLevelParameters()
        {
            return SQLHelper.GetObjectList(new LU_ServiceLevelParameters(), "USP_Accounting_LU_ServiceLevelParameters");
        }
        public List<LU_ServiceRequestTypes> GetAllServiceRequestTypes()
        {
            return SQLHelper.GetObjectList(new LU_ServiceRequestTypes(), "USP_Accounting_LU_ServiceRequestTypes");
        }
        public List<LU_PredefinedParameters> GetAllPredefinedParameters()
        {
            return SQLHelper.GetObjectList(new LU_PredefinedParameters(), "USP_Accounting_LU_PredefinedParameters");
        }
        public List<LU_ServiceProviderTypes> GetAllServiceProviderTypes()
        {
            return SQLHelper.GetObjectList(new LU_ServiceProviderTypes(), "USP_Accounting_LU_ServiceProviderTypes");
        }
        public List<LU_ServiceProviders> GetAllServiceProviders()
        {
            return SQLHelper.GetObjectList(new LU_ServiceProviders(), "USP_Accounting_LU_ServiceProviders");
        }
        public List<LU_LineItems_ChartOfAccounts> GetAllLineItems_ChartOfAccounts(byte Service_Id, long Client_Id)
        {
            SqlParameter[] sp =
             {
                new SqlParameter("@Service_Id", Service_Id),
                new SqlParameter("@Client_Id", Client_Id),
            };
            return SQLHelper.GetObjectList(new LU_LineItems_ChartOfAccounts(), "USP_Accounting_LU_LineItems_ChartOfAccounts", sp);
        }
        public List<LU_Datatypes> GetAllDataTypes()
        {
            return SQLHelper.GetObjectList(new LU_Datatypes(), "USP_Accounting_LU_DataTypes");
        }
        public List<LU_OperatingBanks_ChartOfAccounts> GetAllOperatingBanksChartOfAccounts(byte Service_Id, long Client_Id)
        {
            SqlParameter[] sp =
           {
                  new SqlParameter("@Service_Id", Service_Id),
                new SqlParameter("@Client_Id", Client_Id),
            };
            return SQLHelper.GetObjectList(new LU_OperatingBanks_ChartOfAccounts(), "USP_Accounting_GetOperatingBanks_ChartOfAccounts", sp);
        }
        public void Insert_LU_LineItems_ChartOfAccounts(LU_LineItems_ChartOfAccounts lU_LineItems_ChartOfAccounts)
        {
            SqlParameter[] sp =
             {
                new SqlParameter("@Client_Id", lU_LineItems_ChartOfAccounts.Client_Id),
                new SqlParameter("@Service_Id", lU_LineItems_ChartOfAccounts.Service_Id),
                new SqlParameter("@BillingType_LineItems_Directions_Id", lU_LineItems_ChartOfAccounts.BillingType_LineItems_Directions_Id),
                new SqlParameter("@ChartOfAccount_Id", lU_LineItems_ChartOfAccounts.ChartOfAccount_Id),
                new SqlParameter("@LOB_Carrier_Grouping_Id", lU_LineItems_ChartOfAccounts.LOB_Carrier_Grouping_Id),
                new SqlParameter("@ChartOfAccountsClassificationType_Id", lU_LineItems_ChartOfAccounts.ChartOfAccountsClassificationType_Id),
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_Insert_LU_LineItems_ChartOfAccounts", sp);
        }
        public List<LU_PaymentDirections_ChartOfAccounts> GetAllPaymentDirectionsChartOfAccounts(byte Service_Id, long Client_Id)
        {
            SqlParameter[] sp =
           {
                  new SqlParameter("@Service_Id", Service_Id),
                new SqlParameter("@Client_Id", Client_Id),
            };
            return SQLHelper.GetObjectList(new LU_PaymentDirections_ChartOfAccounts(), "USP_Accounting_GetPaymentDirections_ChartOfAccounts", sp);
        }

        public void Insert_Update_OperatingBanksChartOfAccounts(LU_OperatingBanks_ChartOfAccounts OperatingBanks_ChartOfAccounts)
        {
            SqlParameter[] sp =
           {
                new SqlParameter("@Client_Id", OperatingBanks_ChartOfAccounts.Client_Id),
                new SqlParameter("@Service_Id", OperatingBanks_ChartOfAccounts.Service_Id),
                new SqlParameter("@OperatingBank_Id", OperatingBanks_ChartOfAccounts.OperatingBank_Id),
                new SqlParameter("@ChartOfAccounts_Id", OperatingBanks_ChartOfAccounts.ChartOfAccounts_Id),
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_Insert_LU_OperatingBanks_ChartOfAccounts", sp);
        }

        public void Insert_Update_PaymentDirectionsChartOfAccounts(LU_PaymentDirections_ChartOfAccounts PaymentDirections_ChartOfAccounts)
        {
            SqlParameter[] sp =
                     {
                new SqlParameter("@Client_Id", PaymentDirections_ChartOfAccounts.Client_Id),
                new SqlParameter("@Service_Id", PaymentDirections_ChartOfAccounts.Service_Id),
                new SqlParameter("@PaymentDirection_Id", PaymentDirections_ChartOfAccounts.PaymentDirection_Id),
                new SqlParameter("@ChartOfAccounts_Id", PaymentDirections_ChartOfAccounts.ChartOfAccounts_Id),
                new SqlParameter("@PaymentDirectionsSubType_Id", PaymentDirections_ChartOfAccounts.PaymentDirectionsSubType_Id),
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_Insert_LU_PaymentDirections_ChartOfAccounts", sp);
        }

        public List<LU_JournalEntryStatus> GetAllJournalEntryStatus()
        {
            return SQLHelper.GetObjectList(new LU_JournalEntryStatus(), "USP_Accounting_GetAllJournalEntryStatus");
        }

        public List<LU_AccountingTypes> GetAllAccountingTypes()
        {
            return SQLHelper.GetObjectList(new LU_AccountingTypes(), "USP_Accounting_GetAllAccountingTypes");
        }
        public List<LU_ChartOfAccountsClassificationTypes> GetAllChartOfAccountsClassificationTypes()
        {
            return SQLHelper.GetObjectList(new LU_ChartOfAccountsClassificationTypes(), "USP_Accounting_GetAllChartOfAccountsClassificationTypes");
        }
        public List<LU_PaymentDirectionsSubTypes> GetAllPaymentDirectionsSubTypes()
        {
            return SQLHelper.GetObjectList(new LU_PaymentDirectionsSubTypes(), "USP_Accounting_GetAllPaymentDirectionsSubTypes");
        }
        public List<LU_BillingType_LineItems_Directions> GetAllLineItems_ByChartOfAccounts_ARAP(long Client_Id)
        {
            SqlParameter[] sqlparams =
            {
                new SqlParameter("@Client_Id",Client_Id)
            };
            return SQLHelper.GetObjectList(new LU_BillingType_LineItems_Directions(), "USP_GetLineItems_ByChartOfAccounts_ARAP", sqlparams);
        }
        public List<LU_BillingType_LineItems_Directions> GetAllLineItems_ByChartOfAccounts_FeesTaxes(long Client_Id)
        {
            SqlParameter[] sqlparams =
            {
                new SqlParameter("@Client_Id",Client_Id)
            };
            return SQLHelper.GetObjectList(new LU_BillingType_LineItems_Directions(), "USP_GetLineItems_ByChartOfAccounts_FeesTaxes", sqlparams);
        }
        public LU_LineItems_ChartOfAccounts GetLineItems_ChartOfAccounts(int BillingType_LineItems_Directions_Id, byte Service_Id, long Client_Id, int LOB_Carrier_Grouping_Id, byte ChartOfAccountsClassificationType_Id)
        {
            SqlParameter[] sp =
             {
                new SqlParameter("@BillingType_LineItems_Directions_Id", BillingType_LineItems_Directions_Id),
                new SqlParameter("@Service_Id", Service_Id),
                new SqlParameter("@Client_Id", Client_Id),
                new SqlParameter("@LOB_Carrier_Grouping_Id", LOB_Carrier_Grouping_Id),
                new SqlParameter("@ChartOfAccountsClassificationType_Id", ChartOfAccountsClassificationType_Id),
            };
            return SQLHelper.GetObject(new LU_LineItems_ChartOfAccounts(), "USP_GetLineItems_ChartOfAccounts", sp);
        }
        #region OperatingBank Module
        public List<LU_OperatingBanks> GetAllOperatingBanks(int PageNumber, int PageSize, ref int TotalRecords, string? Searchvalue, long Client_Id)
        {
            SqlParameter[] sqlparams =
            {
                new SqlParameter("@PageNumber",PageNumber),
                new SqlParameter("@PageSize",PageSize),
                new SqlParameter("@SearchValue",Searchvalue),
                new SqlParameter("@Client_Id",Client_Id),
                new SqlParameter(){ParameterName="@TotalRecords",Direction=ParameterDirection.Output,Size=100 },
            };
            var _Result = SQLHelper.GetObjectList(new LU_OperatingBanks(), "USP_Accounting_GetAllOperatingBanks", sqlparams);
            TotalRecords = Convert.ToInt32(sqlparams.Where(x => x.ParameterName == "@TotalRecords")?.FirstOrDefault()?.Value);
            return _Result;
        }
        public LU_OperatingBanks GetOperatingBank(int OperatingBank_Id, long Client_Id)
        {
            SqlParameter[] sqlparams =
            {
                new SqlParameter("@OperatingBank_Id",OperatingBank_Id),
                new SqlParameter("@Client_Id",Client_Id)
            };
            return SQLHelper.GetObject(new LU_OperatingBanks(), "USP_Accounting_GetOperatingBank", sqlparams);
        }
        public LU_OperatingBanks InsertUpdateOperatingBanks(LU_OperatingBanks operatingbanks)
        {
            SqlParameter[] sqlparams =
            {
                new SqlParameter (){ParameterName="@OperatingBank_Id",Value=operatingbanks.OperatingBank_Id,Direction=ParameterDirection.InputOutput},
                new SqlParameter("@Name",operatingbanks.Name),
                new SqlParameter("@Code",operatingbanks.Code),
                new SqlParameter("@Client_Id",operatingbanks.Client_Id),
                new SqlParameter("@AccountNumber",operatingbanks.AccountNumber),
                new SqlParameter("@RoutingNumber",operatingbanks.RoutingNumber),
                new SqlParameter("@SignerValue",operatingbanks.SignerValue),
                new SqlParameter("@SignerType",operatingbanks.SignerType),
                new SqlParameter("@Check_CompanyName",operatingbanks.Check_CompanyName),
                new SqlParameter("@Check_BankName",operatingbanks.Check_BankName),
                new SqlParameter("@Check_BankAddress",operatingbanks.Check_BankAddress),
                new SqlParameter("@cityName",operatingbanks.cityName),
                new SqlParameter("@ZipCode",operatingbanks.zipCode)
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_InsertUpdateOperatingBanks", sqlparams);
            operatingbanks.OperatingBank_Id = Convert.ToInt32(sqlparams.Where(x => x.ParameterName == "@OperatingBank_Id").FirstOrDefault()?.Value);
            return operatingbanks;
        }
        public void DeleteOperatingBank(int OperatingBank_Id, long Client_Id)
        {
            SqlParameter[] sqlparams =
            {
                new SqlParameter("@OperatingBank_Id",OperatingBank_Id),
                new SqlParameter("@Client_Id",Client_Id)
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_DeleteOperatingBank", sqlparams);
        }
        #endregion

        #region Installment Plan Module
        public List<LU_PaymentPlans> GetAllInstallmentPlan(long Client_Id, int PageNumber, int PageSize, ref int TotalRecords, string? Name, string? Code, string? NoOfPayments, string? Carrier, bool HasuniqueCarrierNames = false)
        {
            try
            {
                SqlParameter[] sqlparams =
                [
                    new("@Client_Id",Client_Id),
                    new("@PageNumber",PageNumber),
                    new("@PageSize",PageSize),
                    new("@Name",Name),
                    new("@Code",Code),
                    new("@NoOfPayments",NoOfPayments),
                    new("@Carrier",Carrier),
                    new(){ParameterName="@TotalRecords",Direction=ParameterDirection.Output,Size=100 },
                ];
                var _Result = HasuniqueCarrierNames ? SQLHelper.GetObjectList(new LU_PaymentPlans(), "USP_Accounting_GetAllPaymentPlanDetails_V1", sqlparams)
                    : SQLHelper.GetObjectList(new LU_PaymentPlans(), "USP_Accounting_GetAllPaymentPlanDetails", sqlparams);
                TotalRecords = Convert.ToInt32(sqlparams.Where(x => x.ParameterName == "@TotalRecords")?.FirstOrDefault()?.Value);
                return _Result;

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public LU_PaymentPlans GetInstallmentPlan(byte PaymentPlan_Id)
        {
            SqlParameter[] sqlparams =
            [
                new("@PaymentPlan_Id",PaymentPlan_Id)
            ];
            return SQLHelper.GetObject(new LU_PaymentPlans(), "USP_Accounting_GetPaymentPlans", sqlparams);
        }
        public LU_PaymentPlans InsertUpdateInstallmentPlan(LU_PaymentPlans installmentPlan)
        {

            SqlParameter[] sqlparams =
            [
                new(){ParameterName="@PaymentPlan_Id",Value=installmentPlan.PaymentPlan_Id,Direction=ParameterDirection.InputOutput},

                new("@Client_Id",installmentPlan.Client_Id),
                new("@Carrier_Id",installmentPlan.Carrier_Id),
                new("@Name",installmentPlan.Name),
                new("@Code",installmentPlan.Code),
                new("@DownPaymentPercent",installmentPlan.DownPaymentPercent),
                new("@DownPaymentMinimum",installmentPlan.DownPaymentMinimum),
                new("@NumberOfPayments",installmentPlan.NumberOfPayments),
                new("@DownPaymentTermDays",installmentPlan.DownPaymentTermDays),
                new("@PaymentAnchorType_Id",installmentPlan.PaymentAnchorType_Id),
                new("@InstallmentBasisType_Id",installmentPlan.InstallmentBasisType_Id),
                new("@IntervalUnitType_Id",installmentPlan.IntervalUnitType_Id),
                new("@IntervalValue",installmentPlan.IntervalValue),
                new("@FirstPaymentOffsetDays",installmentPlan.FirstPaymentOffsetDays),
                new("@FixedDayOfMonth",installmentPlan.FixedDayOfMonth),
                new("@IsSetBillDateEqualDue",installmentPlan.IsSetBillDateEqualDue),
                new("@IsInstallmentFeeSetup",installmentPlan.IsInstallmentFeeSetup),
                new("@LeadTime",installmentPlan.LeadTime),
                new("@IsAutoSendInvoices",installmentPlan.IsAutoSendInvoices),
                new("@IsActive",installmentPlan.IsActive),
                new("@SequenceNo",installmentPlan.SequenceNo),
            ];
            SQLHelper.ExecuteNonQuery("USP_Accounting_InsertUpdatePaymentPlans", sqlparams);
            installmentPlan.PaymentPlan_Id = Convert.ToByte(sqlparams.Where(x => x.ParameterName == "@PaymentPlan_Id").FirstOrDefault()?.Value);
            return installmentPlan;
        }
        public void DeleteInstallmentPlan(byte PaymentPlan_Id)
        {
            SqlParameter[] sqlparams =
            [
                new SqlParameter("@PaymentPlan_Id",PaymentPlan_Id)
            ];
            SQLHelper.ExecuteNonQuery("USP_Accounting_DeletePaymentPlan", sqlparams);
        }
        #endregion

        #region LineofBusiness Module
        public List<LU_LOBs> GetAllLineofBusiness(int PageNumber, int PageSize, ref int TotalRecords, string? Searchvalue)
        {
            SqlParameter[] sqlparams =
            {
                new SqlParameter("@PageNumber",PageNumber),
                new SqlParameter("@PageSize",PageSize),
                new SqlParameter("@SearchValue",Searchvalue),
                new SqlParameter(){ParameterName="@TotalRecords",Direction=ParameterDirection.Output,Size=100 },
            };
            var _Result = SQLHelper.GetObjectList(new LU_LOBs(), "USP_Accounting_GetAllLineofBusiness", sqlparams);
            TotalRecords = Convert.ToInt32(sqlparams.Where(x => x.ParameterName == "@TotalRecords")?.FirstOrDefault()?.Value);
            return _Result;
        }
        public LU_LOBs GetLineofBusiness(byte LOB_Id)
        {
            SqlParameter[] sqlparams =
            {
                new SqlParameter("@LOB_Id",LOB_Id)
            };
            return SQLHelper.GetObject(new LU_LOBs(), "USP_Accounting_GetLineofBusiness", sqlparams);
        }
        public LU_LOBs InsertUpdateLineofBusiness(LU_LOBs lineofbusiness)
        {
            SqlParameter[] sqlparams =
            {
                new SqlParameter (){ParameterName="@LOB_Id",Value=lineofbusiness.LOB_Id,Direction=ParameterDirection.InputOutput},
                new SqlParameter("@Name",lineofbusiness.Name),
                new SqlParameter("@Code",lineofbusiness.Code),
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_InsertUpdateLineofBusiness", sqlparams);
            lineofbusiness.LOB_Id = Convert.ToByte(sqlparams.Where(x => x.ParameterName == "@LOB_Id").FirstOrDefault()?.Value);
            return lineofbusiness;
        }
        public void DeleteLineofBusiness(byte LOB_Id)
        {
            SqlParameter[] sqlparams =
            {
                new SqlParameter("@LOB_Id",LOB_Id)
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_DeleteLineofBusiness", sqlparams);
        }
        #endregion

        #region Reports
        public List<LU_DataDictionaryTypes> GetDataDictionaryTypes()
        {
            return SQLHelper.GetObjectList(new LU_DataDictionaryTypes(), CommandType.Text, "Select * from Accounting_LU_DataDictionaryTypes where status=1");
        }
        public List<LU_FileTypes> GetFileTypes()
        {
            return SQLHelper.GetObjectList(new LU_FileTypes(), CommandType.Text, "Select * from Accounting_LU_FileTypes where status=1");
        }
        public List<LU_DataDictionary> GetDataDictionaries()
        {
            return SQLHelper.GetObjectList(new LU_DataDictionary(), CommandType.Text, "Select * from Accounting_LU_DataDictionary where status=1");
        }
        public List<LU_HTMLControlTypes> GetHTMLControlTypes()
        {
            return SQLHelper.GetObjectList(new LU_HTMLControlTypes(), CommandType.Text, "Select * from Accounting_LU_HTMLControlTypes where status=1");
        }
        public List<LU_ReportTemplates> GetAllReportTemplates()
        {
            return SQLHelper.GetObjectList(new LU_ReportTemplates(), "USP_Accounting_GetAllReportTemplates");
        }
        #endregion

        public List<LU_LineItems_Clients> GetAllLineItems_Clients()
        {
            return SQLHelper.GetObjectList(new LU_LineItems_Clients(), "USP_Accounting_GetAllLineItems_Clients");
        }
        #region LineItems (Fee/Tax)
        public List<LU_LineItems> GetPagedLineItemsList(int PageNumber, int PageSize, ref int TotalRecords, string? Searchvalue)
        {
            SqlParameter[] sqlparams =
            {
                new SqlParameter("@PageNumber",PageNumber),
                new SqlParameter("@PageSize",PageSize),
                new SqlParameter("@SearchValue",Searchvalue),
                new SqlParameter(){ParameterName="@TotalRecords",Direction=ParameterDirection.Output,Size=100 },
            };
            var _Result = SQLHelper.GetObjectList(new LU_LineItems(), "USP_Accounting_GetPagedLineItemsList", sqlparams);
            TotalRecords = Convert.ToInt32(sqlparams.Where(x => x.ParameterName == "@TotalRecords")?.FirstOrDefault()?.Value);
            return _Result;
        }
        public LU_LineItems GetLineItem(long LineItem_Id)
        {
            SqlParameter[] sqlparams =
            {
                new SqlParameter("@LineItem_Id",LineItem_Id)
            };
            return SQLHelper.GetObject(new LU_LineItems(), "USP_Accounting_GetLineItem", sqlparams);
        }
        public LU_LineItems InsertUpdateLineItem(LU_LineItems lineitem)
        {
            SqlParameter[] sqlparams =
            {
                new SqlParameter (){ParameterName="@LineItem_Id",Value=lineitem.LineItem_Id,Direction=ParameterDirection.InputOutput},
                new SqlParameter("@LineItemType_Id",lineitem.LineItemType_Id),
                new SqlParameter("@Name",lineitem.Name),
                new SqlParameter("@Code",lineitem.Code),
                new SqlParameter("@Description",lineitem.Description),
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_InsertUpdateLineItem", sqlparams);
            lineitem.LineItem_Id = Convert.ToInt64(sqlparams.Where(x => x.ParameterName == "@LineItem_Id").FirstOrDefault()?.Value);
            return lineitem;
        }
        public void DeleteLineItem(long LineItem_Id)
        {
            SqlParameter[] sqlparams =
            {
                new SqlParameter("@LineItem_Id",LineItem_Id)
            };
            SQLHelper.ExecuteNonQuery("USP_Accounting_DeleteLineItem", sqlparams);
        }
        #endregion
        public LU_LineItems_Directions InsertUpdateLineItems_Directions(SqlTransaction sqlTransaction, LU_LineItems_Directions lU_LineItems_Directions)
        {
            SqlParameter[] sp =
             {
                new SqlParameter(){ParameterName ="@LineItems_Directions_Id",Value=lU_LineItems_Directions.LineItems_Directions_Id,Direction=ParameterDirection.InputOutput},
                new SqlParameter("@LineItems_Clients_Id", lU_LineItems_Directions.LineItems_Clients_Id),
                new SqlParameter("@Direction_Id", lU_LineItems_Directions.Direction_Id),
                new SqlParameter("@EntityType_Id", lU_LineItems_Directions.EntityType_Id == 0 ? null : lU_LineItems_Directions.EntityType_Id),
            };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertUpdateLineItems_Directions", sp);
            lU_LineItems_Directions.LineItems_Directions_Id = Convert.ToInt32(sp.Where(l => l.ParameterName == "@LineItems_Directions_Id").FirstOrDefault()?.Value);
            return lU_LineItems_Directions;
        }
        public LU_BillingType_LineItems_Directions InsertBillingType_LineItems_Directions(SqlTransaction sqlTransaction, LU_BillingType_LineItems_Directions lU_BillingType_LineItems_Directions)
        {
            SqlParameter[] sql =
            {
                new SqlParameter(){ParameterName = "@BillingType_LineItems_Directions_Id",Value=lU_BillingType_LineItems_Directions.BillingType_LineItems_Directions_Id,Direction =ParameterDirection.InputOutput},
                new SqlParameter("@BillingType_Id",lU_BillingType_LineItems_Directions.BillingType_Id),
                new SqlParameter("@LineItems_Directions_Id",lU_BillingType_LineItems_Directions.LineItems_Directions_Id),
            };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertBillingType_LineItems_Directions", sql);
            lU_BillingType_LineItems_Directions.BillingType_LineItems_Directions_Id = Convert.ToInt32(sql.Where(l => l.ParameterName == "@BillingType_LineItems_Directions_Id").FirstOrDefault()?.Value);
            return lU_BillingType_LineItems_Directions;
        }
        public void InactiveLineItems_Directions(SqlTransaction sqlTransaction, long LineItems_Clients_Id)
        {
            SqlParameter[] sql =
            {
                new SqlParameter("@LineItems_Clients_Id", LineItems_Clients_Id),
            };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InactiveLineItems_Directions", sql);
        }
        public void InactiveBillingType_LineItems_Directions(SqlTransaction sqlTransaction, int BillingType_LineItems_Directions_Id)
        {
            SqlParameter[] sql =
            {
                new SqlParameter("@BillingType_LineItems_Directions_Id", BillingType_LineItems_Directions_Id),
            };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "US_Accounting_InactiveBillingType_LineItems_Directions", sql);
        }
        public List<LU_LineItems_Clients> GetLineItems_ClientsByBillingType_Id(long Client_Id, byte BillingType_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Client_Id", Client_Id),
                new SqlParameter("@BillingType_Id", BillingType_Id),
            };
            return SQLHelper.GetObjectList(new LU_LineItems_Clients(), "USP_Accounting_GetLineItems_ClientsByBillingType_Id", parameters);
        }
        public List<LU_LineItems_Clients> GetLineItems_ClientsByClient_Id(long Client_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Client_Id", Client_Id),
            };
            return SQLHelper.GetObjectList(new LU_LineItems_Clients(), "USP_Accounting_GetLineItems_ClientsByClient_Id", parameters);
        }
        public LU_LineItems_Clients GetLineItems_ClientsByLineItem_Id(long Client_Id, long LineItem_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Client_Id", Client_Id),
                new SqlParameter("@LineItem_Id", LineItem_Id),
            };
            return SQLHelper.GetObject(new LU_LineItems_Clients(), "USP_Accounting_GetLineItems_ClientsByLineItem_Id", parameters);
        }
        public LU_LineItems_Clients InsertUpdateLineItems_Clients(SqlTransaction sqlTransaction, LU_LineItems_Clients lineItems_Clients)
        {
            SqlParameter[] parameters =
            {
                 new SqlParameter(){ParameterName = "@LineItems_Clients_Id",Value=lineItems_Clients.LineItems_Clients_Id,Direction =ParameterDirection.InputOutput},
                new SqlParameter("@Client_Id", lineItems_Clients.Client_Id),
                new SqlParameter("@LineItem_Id", lineItems_Clients.LineItem_Id),
                new SqlParameter("@Is_Passthrough", lineItems_Clients.Is_Passthrough),
                new SqlParameter("@Is_Excluded", lineItems_Clients.Is_Excluded),
                new SqlParameter("@Is_Commissionable", lineItems_Clients.Is_Commissionable),
            };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InsertUpdateLineItems_Clients", parameters);
            lineItems_Clients.LineItems_Clients_Id = Convert.ToInt32(parameters.Where(l => l.ParameterName == "@LineItems_Clients_Id").FirstOrDefault()?.Value);
            return lineItems_Clients;
        }
        public List<LU_LineItems_Clients> GetLineItemsDirections(long Lineitems_Clients_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Lineitems_Clients_Id", Lineitems_Clients_Id),
            };
            return SQLHelper.GetObjectList(new LU_LineItems_Clients(), "USP_Accounting_GetLineItemsDirections_V1", parameters);
        }
        public LU_LineItems_Directions GetLineItems_Directions(SqlTransaction sqlTransaction, long LineItems_Clients_Id, byte Direction_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@LineItems_Clients_Id", LineItems_Clients_Id),
                new SqlParameter("@Direction_Id", Direction_Id),
            };
            return SQLHelper.GetObjectWithTransaction(sqlTransaction, CommandType.StoredProcedure, new LU_LineItems_Directions(), "USP_Accounting_GetLineItems_Directions", parameters);
        }
        public LU_BillingType_LineItems_Directions GetBillingType_LineItems_Directions(long LineItems_Directions_Id, byte BillingType_Id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@LineItems_Directions_Id", LineItems_Directions_Id),
                new SqlParameter("@BillingType_Id", BillingType_Id),
            };
            return SQLHelper.GetObject(new LU_BillingType_LineItems_Directions(), "USP_Accounting_GetBillingType_LineItems_Directions", parameters);
        }
        public void Insert_LU_LineItems_ChartOfAccountsWithTransaction(SqlTransaction sqltransaction, LU_LineItems_ChartOfAccounts lU_LineItems_ChartOfAccounts)
        {
            SqlParameter[] sp =
             {
                new SqlParameter("@Client_Id", lU_LineItems_ChartOfAccounts.Client_Id),
                new SqlParameter("@Service_Id", lU_LineItems_ChartOfAccounts.Service_Id),
                new SqlParameter("@BillingType_LineItems_Directions_Id", lU_LineItems_ChartOfAccounts.BillingType_LineItems_Directions_Id),
                new SqlParameter("@ChartOfAccount_Id", lU_LineItems_ChartOfAccounts.ChartOfAccount_Id),
                new SqlParameter("@LOB_Carrier_Grouping_Id", lU_LineItems_ChartOfAccounts.LOB_Carrier_Grouping_Id),
                new SqlParameter("@ChartOfAccountsClassificationType_Id", lU_LineItems_ChartOfAccounts.ChartOfAccountsClassificationType_Id),
            };
            SQLHelper.ExecuteNonQuery(sqltransaction, CommandType.StoredProcedure, "USP_Accounting_Insert_LU_LineItems_ChartOfAccounts", sp);
        }
        public void InactiveLineitems_Clients(SqlTransaction sqlTransaction, long LineItems_Clients_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@LineItems_Clients_Id",LineItems_Clients_Id)
            };
            SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InactivateLineitems_Clients", sp);
        }
        public LU_LineItems_Clients GetLineItemsMapping(LU_LineItems_Clients items)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Client_Id",items.Client_Id),
                new SqlParameter("@BillTypeList",items.BillTypeList == null ? items.BillingType_Id.ToString(): items.BillTypeList),
                new SqlParameter("@DirectionList",items.DirectionList == null? items.Direction_Id.ToString(): items.DirectionList),
                new SqlParameter("@LineItem_Id",items.LineItem_Id),
                new SqlParameter("@EntityType_Id",items.EntityType_Id == 0 ? null : items.EntityType_Id),
                new SqlParameter("@Is_Passthrough",items.Is_Passthrough),
                new SqlParameter("@Is_Excluded",items.Is_Excluded),
                new SqlParameter("@Is_Commissionable", items.Is_Commissionable)
            };
            return SQLHelper.GetObject(new LU_LineItems_Clients(), "USP_Accounting_GetExistingLineitemMapping", sp);
        }
        public List<LU_BillingType_LineItems_Directions> GetExistingBillTypesByLineItems_Clients_Id(long LineItems_Clients_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@LineItems_Clients_Id",LineItems_Clients_Id)
            };
            return SQLHelper.GetObjectList(new LU_BillingType_LineItems_Directions(), "USP_Accounting_GetExistingBillTypesByLineItems_Clients_Id", sp);
        }
        public List<LU_LineItems_Directions> GetExistingDirectionsByLineItems_Clients_Id(long LineItems_Clients_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@LineItems_Clients_Id",LineItems_Clients_Id)
            };
            return SQLHelper.GetObjectList(new LU_LineItems_Directions(), "USP_Accounting_GetExistingDirectionsByLineItems_Clients_Id", sp);
        }
        public int InactiveExistingBillTypesByBillingType_LineItems_Directions_Id(SqlTransaction sqlTransaction, int BillingType_LineItems_Directions_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@BillingType_LineItems_Directions_Id",BillingType_LineItems_Directions_Id)
            };
            return SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InactiveExistingBillTypesByBillingType_LineItems_Directions_Id", sp);
        }
        public int InactiveExistingDirectionsByLineItems_Directions_Id(SqlTransaction sqlTransaction, int LineItems_Directions_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@LineItems_Directions_Id",LineItems_Directions_Id)
            };
            return SQLHelper.ExecuteNonQuery(sqlTransaction, CommandType.StoredProcedure, "USP_Accounting_InactiveExistingDirectionsByLineItems_Directions_Id", sp);
        }
        public LU_Clients_AccountingTypes_BillingTypes GetClients_AccountingTypes_BillingTypes(long Client_Id, byte BillingType_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Client_Id",Client_Id),
                new SqlParameter("@BillingType_Id",BillingType_Id)
            };
            return SQLHelper.GetObject(new LU_Clients_AccountingTypes_BillingTypes(), "USP_Accounting_GetClients_AccountingTypes_BillingTypes", sp);
        }
        public List<LU_AmountDueBasedOnTypes> GetAmountDueBasedOnTypes()
        {
            return SQLHelper.GetObjectList(new LU_AmountDueBasedOnTypes(), "USP_Accounting_GetAllAmountDueBasedOnTypes");
        }
        public List<LU_CommissionTypes> GetAllCommissionTypes()
        {
            return SQLHelper.GetObjectList(new LU_CommissionTypes(), "USP_Accounting_GetAllCommissionTypes");
        }
        public List<LU_CommissionBasisTypes> GetAllCommissionBasisTypes()
        {
            return SQLHelper.GetObjectList(new LU_CommissionBasisTypes(), "USP_Accounting_GetAllCommissionBasisTypes");
        }
        public List<ChartofAccounts> GetChartOfAccountsByChartOfAccount_Id(int? ChartOfAccount_Id, long Client_Id)
        {
            SqlParameter[] sp =
            {
                    new SqlParameter("@ChartOfAccount_Id", ChartOfAccount_Id),
                    new SqlParameter("@Client_Id", Client_Id)
            };
            return SQLHelper.GetObjectList(new ChartofAccounts(), "USP_Accounting_GetChartOfAccountDetails", sp);
        }
        public List<ChartOfAccountDetails> GetAllPaymentDirectionsSageChartOfAccounts(byte Service_Id, long Client_Id, int PaymentDirection_Id)
        {
            SqlParameter[] sp =
           {
                  new SqlParameter("@Service_Id", Service_Id),
                new SqlParameter("@Client_Id", Client_Id),
                new SqlParameter("@PaymentDirection_Id", PaymentDirection_Id),
            };
            return SQLHelper.GetObjectList(new ChartOfAccountDetails(), "USP_Accounting_GetPaymentDirections_SageChartOfAccounts", sp);
        }
        public List<LU_SubmissionTypes> GetAllSubmissionTypes()
        {
            return SQLHelper.GetObjectList(new LU_SubmissionTypes(), "USP_Accounting_GetAllSubmissionTypes");
        }
        public List<LU_CheckPaymentPrintStatus> GetCheckPaymentPrintStatus()
        {
            return SQLHelper.GetObjectList(new LU_CheckPaymentPrintStatus(), "USP_Accounting_GetAllCheckPaymentPrintStatus");
        }
        public void UpdateQBORefreshToken(long Client_Id, int service_Id, int paramter_Id, string refreshToken)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Client_Id",Client_Id),
                new SqlParameter("@RefreshToken", refreshToken),
                new SqlParameter("@Service_Id", service_Id),
                new SqlParameter("@Parameter_Id", paramter_Id)
            };
            string Query = "USP_UpdateQBORefreshToken";
            SQLHelper.ExecuteNonQuery(CommandType.StoredProcedure, Query, parameters);
        }
        public List<LU_ChecKeeperSignatureTypes> GetAllChecKeeperSignatureTypes()
        {
            return SQLHelper.GetObjectList(new LU_ChecKeeperSignatureTypes(), "USP_Accounting_GetAllChecKeeperSignatureTypes");
        }
        public List<LU_BillingTypes> GetAllClientBillingTypes(long client_Id)
        {
            SqlParameter[] sp =
            {
                new SqlParameter("@Client_Id", client_Id),
            };
            return SQLHelper.GetObjectList(new LU_BillingTypes(), "USP_Accounting_GetAllClientBillingTypes", sp);
        }
        public List<LU_Actions> GetAllActions()
        {
            return SQLHelper.GetObjectList(new LU_Actions(), "USP_Accounting_LU_GetActions");
        }
    }

}
