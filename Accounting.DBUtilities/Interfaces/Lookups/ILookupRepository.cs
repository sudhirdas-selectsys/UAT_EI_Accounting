using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Microsoft.Data.SqlClient;
using Accounting.Entities.Internal;
namespace Accounting.DBUtilities.Interfaces.Lookups
{
    public interface ILookupRepository
    {

        List<LU_BillingTypes> GetAllBillingTypes();
        List<LU_PaymentPlans> GetAllPaymentPlans();
        List<LU_Roles> GetAllRoles();
        List<LU_Sources> GetAllSources();
        List<LU_States> GetAllStates();
        List<LU_OperatingBanks> GetAllOperatingBanks();
        List<LU_PaymentTypes> GetAllPaymentTypes();
        List<LU_PaymentStatus> GetAllPaymentStatus();
        List<LU_LOBs> GetAllLOBs();
        List<LU_Directions> GetAllDirections();
        List<LU_PaymentDirections> GetAllPaymentDirections();
        List<LU_LineItemTypes> GetAllLineItemTypes();
        List<LU_LineItems> GetAllLineItems();
        List<LU_LineItems_Directions> GetAllLineItems_Directions();
        List<LU_BillingType_LineItems_Directions> GetAllBillingType_LineItems_Directions();
        List<LU_EntityTypes> GetAllEntityTypes();
        //void InsertLineItems_Directions(LU_LineItems_Directions lU_LineItems_Directions);
        List<LU_ClientLevelParameters> GetAllClientLevelParameters(long client_Id);
        List<LU_Services> GetAllServices();
        List<LU_Parameters> GetAllParameters();
        List<LU_ServiceLevelParameters> GetAllServiceLevelParameters();
        List<LU_ServiceRequestTypes> GetAllServiceRequestTypes();
        List<LU_PredefinedParameters> GetAllPredefinedParameters();
        List<LU_ServiceProviderTypes> GetAllServiceProviderTypes();
        List<LU_ServiceProviders> GetAllServiceProviders();
        List<LU_LineItems_ChartOfAccounts> GetAllLineItems_ChartOfAccounts(byte Service_Id, long Client_Id);
        List<LU_Datatypes> GetAllDataTypes();
        void Insert_LU_LineItems_ChartOfAccounts(LU_LineItems_ChartOfAccounts lU_LineItems_ChartOfAccounts);
        void Insert_Update_OperatingBanksChartOfAccounts(LU_OperatingBanks_ChartOfAccounts OperatingBanks_ChartOfAccounts);
        void Insert_Update_PaymentDirectionsChartOfAccounts(LU_PaymentDirections_ChartOfAccounts PaymentDirections_ChartOfAccounts);
        List<LU_OperatingBanks_ChartOfAccounts> GetAllOperatingBanksChartOfAccounts(byte Service_Id, long Client_Id);
        List<LU_PaymentDirections_ChartOfAccounts> GetAllPaymentDirectionsChartOfAccounts(byte Service_Id, long Client_Id);
        List<LU_JournalEntryStatus> GetAllJournalEntryStatus();
        List<LU_AccountingTypes> GetAllAccountingTypes();
        List<LU_ChartOfAccountsClassificationTypes> GetAllChartOfAccountsClassificationTypes();
        List<LU_PaymentDirectionsSubTypes> GetAllPaymentDirectionsSubTypes();
        List<LU_BillingType_LineItems_Directions> GetAllLineItems_ByChartOfAccounts_ARAP(long Client_Id);
        List<LU_BillingType_LineItems_Directions> GetAllLineItems_ByChartOfAccounts_FeesTaxes(long Client_Id);
        LU_LineItems_ChartOfAccounts GetLineItems_ChartOfAccounts(int BillingType_LineItems_Directions_Id, byte Service_Id, long Client_Id, int LOB_Carrier_Grouping_Id, byte ChartOfAccountsClassificationType_Id);

        #region OperatingBanks Module
        List<LU_OperatingBanks> GetAllOperatingBanks(int PageNumber, int PageSize, ref int TotalRecords, string? Searchvalue, long Client_Id);
        LU_OperatingBanks GetOperatingBank(int OperatingBank_Id, long Client_Id);
        LU_OperatingBanks InsertUpdateOperatingBanks(LU_OperatingBanks operatingbanks);
        public void DeleteOperatingBank(int OperatingBank_Id, long Client_Id);
        #endregion

        #region LineofBusiness Module
        List<LU_LOBs> GetAllLineofBusiness(int PageNumber, int PageSize, ref int TotalRecords, string? Searchvalue);
        LU_LOBs GetLineofBusiness(byte LOB_Id);
        LU_LOBs InsertUpdateLineofBusiness(LU_LOBs lineofbusiness);
        public void DeleteLineofBusiness(byte LOB_Id);
        #endregion

        #region Installment Plan Module
        List<LU_PaymentPlans> GetAllInstallmentPlan(long Client_Id, int PageNumber, int PageSize, ref int TotalRecords,string? Name, string? Code, string? NoOfPayments, string? Carrier, bool HasuniqueCarrierNames = false);
        LU_PaymentPlans GetInstallmentPlan(byte installmentPlan_Id);
        LU_PaymentPlans InsertUpdateInstallmentPlan(LU_PaymentPlans installmentPlan);
        public void DeleteInstallmentPlan(byte installmentPlan_Id);
        #endregion

        #region Reports
        List<LU_DataDictionaryTypes> GetDataDictionaryTypes();
        List<LU_FileTypes> GetFileTypes();
        List<LU_DataDictionary> GetDataDictionaries();
        List<LU_HTMLControlTypes> GetHTMLControlTypes();
        List<LU_ReportTemplates> GetAllReportTemplates();
        #endregion
        List<LU_LineItems_Clients> GetAllLineItems_Clients();
        #region LineItems (Fee/Tax)
        List<LU_LineItems> GetPagedLineItemsList(int PageNumber, int PageSize, ref int TotalRecords, string? Searchvalue);
        LU_LineItems GetLineItem(long LineItem_Id);
        LU_LineItems InsertUpdateLineItem(LU_LineItems lineitem);
        void DeleteLineItem(long LineItem_Id);
        #endregion
        LU_LineItems_Directions InsertUpdateLineItems_Directions(SqlTransaction sqlTransaction, LU_LineItems_Directions lU_LineItems_Directions);
        LU_BillingType_LineItems_Directions InsertBillingType_LineItems_Directions(SqlTransaction sqlTransaction, LU_BillingType_LineItems_Directions lU_BillingType_LineItems_Directions);
        void InactiveLineItems_Directions(SqlTransaction sqlTransaction, long LineItems_Clients_Id);
        void InactiveBillingType_LineItems_Directions(SqlTransaction sqlTransaction, int BillingType_LineItems_Directions_Id);
        List<LU_LineItems_Clients> GetLineItems_ClientsByBillingType_Id(long Client_Id, byte BillingType_Id);
        List<LU_LineItems_Clients> GetLineItems_ClientsByClient_Id(long Client_Id);
        LU_LineItems_Clients InsertUpdateLineItems_Clients(SqlTransaction sqlTransaction, LU_LineItems_Clients lineItems_Clients);
        List<LU_LineItems_Clients> GetLineItemsDirections(long LineItems_Clients_Id);
        LU_LineItems_Directions GetLineItems_Directions(SqlTransaction sqlTransaction, long LineItems_Clients_Id, byte Direction_Id);
        LU_BillingType_LineItems_Directions GetBillingType_LineItems_Directions(long LineItems_Directions_Id, byte BillingType_Id);
        void Insert_LU_LineItems_ChartOfAccountsWithTransaction(SqlTransaction sqltransaction, LU_LineItems_ChartOfAccounts lU_LineItems_ChartOfAccounts);
        List<LU_AnchorTypes> GetAllAnchorTypes();
        List<LU_InstallmentBasisTypes> GetAllInstallmentBasisTypes();
        List<LU_IntervalUnitTypes> GetAllIntervalUnitTypes();
        void InactiveLineitems_Clients(SqlTransaction sqlTransaction, long LineItems_Clients_Id);
        LU_LineItems_Clients GetLineItemsMapping(LU_LineItems_Clients items);
        List<LU_BillingType_LineItems_Directions> GetExistingBillTypesByLineItems_Clients_Id(long LineItems_Clients_Id);
        List<LU_LineItems_Directions> GetExistingDirectionsByLineItems_Clients_Id(long LineItems_Clients_Id);
        int InactiveExistingBillTypesByBillingType_LineItems_Directions_Id(SqlTransaction sqlTransaction, int BillingType_LineItems_Directions_Id);
        int InactiveExistingDirectionsByLineItems_Directions_Id(SqlTransaction sqlTransaction, int LineItems_Directions_Id);
        LU_Clients_AccountingTypes_BillingTypes GetClients_AccountingTypes_BillingTypes(long Client_Id, byte BillingType_Id);
		List<LU_AmountDueBasedOnTypes> GetAmountDueBasedOnTypes();
        List<LU_CommissionTypes> GetAllCommissionTypes();
        List<LU_CommissionBasisTypes> GetAllCommissionBasisTypes();
        List<ChartOfAccountDetails> GetAllPaymentDirectionsSageChartOfAccounts(byte Service_Id, long Client_Id, int PaymentDirection_Id);
        List<ChartofAccounts> GetChartOfAccountsByChartOfAccount_Id(int? ChartOfAccount_Id, long Client_Id);
        List<LU_SubmissionTypes> GetAllSubmissionTypes();
        List<LU_CheckPaymentPrintStatus> GetCheckPaymentPrintStatus();
        void UpdateQBORefreshToken(long Client_Id, int service_Id, int paramter_Id, string refreshToken);
        List<LU_ChecKeeperSignatureTypes> GetAllChecKeeperSignatureTypes();
        List<LU_BillingTypes> GetAllClientBillingTypes(long client_Id);
        List<LU_Actions> GetAllActions();
    }
}
