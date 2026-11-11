using Accounting.DBUtilities.Implementations.Lookups;
using Accounting.DBUtilities.Implementations.Transactions;
using Accounting.DBUtilities.Interfaces.Lookups;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.Internal.Enums;
using Microsoft.Extensions.Caching.Memory;
namespace Accounting.Utilities.Internal
{
    public static class LookupUtility
    {
        public static IMemoryCache _memoryCache { get; set; }
        static ILookupRepository lookupRepository = new LookupRepository();
        static IChartofAccountsRepository chartofAccountsRepository = new ChartofAccountsRepository();
        static IClientSettingsRepository clientSettingsRepository = new ClientSettingsRepository();
        static LookupUtility()
        {
            _memoryCache = WebAppReferenceUtility._memoryCache;
        }

        public static List<LU_BillingTypes> GetAllBillingTypes()
        {
            string key = "LU_BillingTypes";
            List<LU_BillingTypes> list = null;
            if (!_memoryCache.TryGetValue(key, out list))
            {
                list = lookupRepository.GetAllBillingTypes();
                _memoryCache.Set(key, list);
            }
            if (list != null)
                return CommonUtility.DeepClone<List<LU_BillingTypes>>(list);
            return null;
        }
        public static List<LU_CommissionTypes> GetAllCommissionTypes()
        {
            string key = "LU_CommissionTypes";
            List<LU_CommissionTypes> list = null;
            if (!_memoryCache.TryGetValue(key, out list))
            {
                list = lookupRepository.GetAllCommissionTypes();
                _memoryCache.Set(key, list);
            }
            if (list != null)
                return CommonUtility.DeepClone<List<LU_CommissionTypes>>(list);
            return null;
        }
        public static List<LU_PaymentPlans> GetAllPaymentPlans()
        {
            // Note: Caching is temporarily disabled to always read the latest data from the database.
            
            List<LU_PaymentPlans> list = null;
            list = lookupRepository.GetAllPaymentPlans();

            //string key = "LU_PaymentPlans";
            //if (!_memoryCache.TryGetValue(key, out list))
            //{
            //    list = lookupRepository.GetAllPaymentPlans();
            //    _memoryCache.Set(key, list);
            //}
            if (list != null)
                return CommonUtility.DeepClone<List<LU_PaymentPlans>>(list);
            return null;

        }
        public static List<LU_Roles> GetAllRoles()
        {
            string key = "LU_Roles";
            List<LU_Roles> List = null;
            if (!_memoryCache.TryGetValue(key, out List))
            {
                List = lookupRepository.GetAllRoles();
                _memoryCache.Set(key, List);
            }
            if (List != null)
                return CommonUtility.DeepClone<List<LU_Roles>>(List);
            return null;
        }
        public static List<LU_Sources> GetAllSources()
        {
            string key = "LU_Sources";
            List<LU_Sources> List = null;
            if (!_memoryCache.TryGetValue(key, out List))
            {
                List = lookupRepository.GetAllSources();
                _memoryCache.Set(key, List);
            }
            if (List != null)
                return CommonUtility.DeepClone<List<LU_Sources>>(List);
            return null;
        }
        public static List<LU_States> GetAllStates()
        {
            string key = "LU_States";
            List<LU_States> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllStates();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_States>>(List);
            return null!;
        }


        public static List<LU_OperatingBanks> GetAllOperatingBanks()
        {
            string key = "LU_OperatingBanks";
            List<LU_OperatingBanks> List = null;
            if (!_memoryCache.TryGetValue(key, out List))
            {
                List = lookupRepository.GetAllOperatingBanks();
                _memoryCache.Set(key, List);
            }
            if (List != null)
                return CommonUtility.DeepClone<List<LU_OperatingBanks>>(List);
            return null;
        }
        public static List<LU_PaymentTypes> GetAllPaymentTypes()
        {
            string key = "LU_PaymentTypes";
            List<LU_PaymentTypes> List = null;
            if (!_memoryCache.TryGetValue(key, out List))
            {
                List = lookupRepository.GetAllPaymentTypes();
                _memoryCache.Set(key, List);
            }
            if (List != null)
                return CommonUtility.DeepClone<List<LU_PaymentTypes>>(List);
            return null;
        }
        public static List<LU_PaymentStatus> GetAllPaymentStatus()
        {
            string key = "LU_PaymentStatus";
            List<LU_PaymentStatus> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllPaymentStatus();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_PaymentStatus>>(List);
            return null!;
        }
        public static List<LU_LOBs> GetAllLOBs()
        {
            string key = "LU_LOBs";
            List<LU_LOBs> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllLOBs();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_LOBs>>(List);
            return null!;
        }




        public static List<LU_Directions> GetAllDirections()
        {
            string key = "LU_Directions";
            List<LU_Directions> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllDirections();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_Directions>>(List);
            return null!;
        }
        public static List<LU_LineItemTypes> GetAllLineItemTypes()
        {
            string key = "LU_LineItemTypes";
            List<LU_LineItemTypes> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllLineItemTypes();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_LineItemTypes>>(List);
            return null!;
        }
        public static List<LU_LineItems> GetAllLineItems()
        {
            string key = "LU_LineItems";
            List<LU_LineItems> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllLineItems();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_LineItems>>(List);
            return null!;
        }
        public static List<LU_LineItems_Directions> GetAllLineItems_Directions()
        {
            string key = "LU_LineItems_Directions";
            List<LU_LineItems_Directions> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllLineItems_Directions();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_LineItems_Directions>>(List);
            return null!;
        }
        public static List<LU_BillingType_LineItems_Directions> GetAllLU_BillingType_LineItems_Directions()
        {
            string key = "LU_BillingType_LineItems_Directions";
            List<LU_BillingType_LineItems_Directions> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllBillingType_LineItems_Directions();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_BillingType_LineItems_Directions>>(List);
            return null!;
        }
        public static List<LU_LineItems_Clients> GetAllLineItems_Clients()
        {
            string key = "LU_LineItems_Clients";
            List<LU_LineItems_Clients> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllLineItems_Clients();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_LineItems_Clients>>(List);
            return null!;
        }

        public static List<ChartofAccounts> GetAllChartofAccounts()
        {
            string key = "ChartofAccounts";
            List<ChartofAccounts> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = chartofAccountsRepository.GetAllChartofAccounts();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<ChartofAccounts>>(List);
            return null!;
        }

        public static List<LU_EntityTypes> GetAllEntityTypes()
        {
            string key = "LU_EntityTypes";
            List<LU_EntityTypes> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllEntityTypes();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_EntityTypes>>(List);
            return null!;
        }

        public static List<LU_PaymentDirections> GetAllPaymentDirections()
        {
            string key = "LU_PaymentDirections";
            List<LU_PaymentDirections> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllPaymentDirections();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_PaymentDirections>>(List);
            return null!;
        }
        public static Enum_QBOSettings GetQBOSettings()
        {
            string key = "QBOSettings";
            Enum_QBOSettings QBOSettings = null!;
            if (!_memoryCache.TryGetValue(key, out QBOSettings!))
            {
                QBOSettings.baseurl = "quickbooks.api.intuit.com";
                QBOSettings.companyid = "9341454630928093";
                QBOSettings.minorversion = "73";
                QBOSettings.client_id = "ABSGAM8DxhzPDN3pSyDtXw9uT04cFSfINVmb7lDy3YPV4bQAak";
                QBOSettings.client_secret = "jYjdMIkqCgp0qX8zBu04MbECRORVepAhlcRzzVbZ";
                QBOSettings.TokenEndpoint = "https://oauth.platform.intuit.com/oauth2/v1/tokens/bearer";
                QBOSettings.refresh_token = "RT1-176-H0-1756053513l5ugad86xlzpc41cygl8";
                _memoryCache.Set(key, QBOSettings);
            }
            if (QBOSettings != null!)
            {
                return CommonUtility.DeepClone<Enum_QBOSettings>(QBOSettings);
            }
            return null!;
        }
        public static List<LU_ClientLevelParameters> GetAllClientLevelParameters(long client_Id)
        {
            //string key = "LU_ClientLevelParameters";
            List<LU_ClientLevelParameters> List = lookupRepository.GetAllClientLevelParameters(client_Id);
            //if (!_memoryCache.TryGetValue(key, out List!))
            //{
            //    List = lookupRepository.GetAllClientLevelParameters();
            //    _memoryCache.Set(key, List);
            //}
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_ClientLevelParameters>>(List);
            return null!;
        }
        public static List<LU_Services> GetAllServices()
        {
            string key = "LU_Services";
            List<LU_Services> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllServices();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_Services>>(List);
            return null!;
        }
        public static List<LU_Parameters> GetAllParameters()
        {
            string key = "LU_Parameters";
            List<LU_Parameters> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllParameters();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_Parameters>>(List);
            return null!;
        }
        public static List<LU_ServiceLevelParameters> GetAllServiceLevelParameters()
        {
            string key = "LU_ServiceLevelParameters";
            List<LU_ServiceLevelParameters> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllServiceLevelParameters();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_ServiceLevelParameters>>(List);
            return null!;
        }
        public static List<LU_ServiceRequestTypes> GetAllServiceRequestTypes()
        {
            string key = "LU_ServiceRequestTypes";
            List<LU_ServiceRequestTypes> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllServiceRequestTypes();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_ServiceRequestTypes>>(List);
            return null!;
        }
        public static List<LU_PredefinedParameters> GetAllPredefinedParameters()
        {
            string key = "LU_PredefinedParameters";
            List<LU_PredefinedParameters> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllPredefinedParameters();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_PredefinedParameters>>(List);
            return null!;
        }
        public static List<LU_ServiceProviderTypes> GetAllServiceProviderTypes()
        {
            string key = "LU_ServiceProviderTypes";
            List<LU_ServiceProviderTypes> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllServiceProviderTypes();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_ServiceProviderTypes>>(List);
            return null!;
        }

        public static List<LU_ServiceProviders> GetAllServiceProviders()
        {
            string key = "LU_ServiceProviders";
            List<LU_ServiceProviders> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllServiceProviders();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_ServiceProviders>>(List);
            return null!;
        }
        public static List<LU_Datatypes> GetAllDataTypes()
        {
            string key = "LU_Datatypes";
            List<LU_Datatypes> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllDataTypes();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_Datatypes>>(List);
            return null!;
        }
        public static List<LU_JournalEntryStatus> GetAllJournalEntryStatus()
        {
            string key = "LU_JournalEntryStatus";
            List<LU_JournalEntryStatus> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllJournalEntryStatus();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_JournalEntryStatus>>(List);
            return null!;
        }
        public static List<LU_AccountingTypes> GetAllAccountingTypes()
        {
            string key = "LU_AccountingTypes";
            List<LU_AccountingTypes> list = null;
            if (!_memoryCache.TryGetValue(key, out list))
            {
                list = lookupRepository.GetAllAccountingTypes();
                _memoryCache.Set(key, list);
            }
            if (list != null)
                return CommonUtility.DeepClone<List<LU_AccountingTypes>>(list);
            return null;
        }
        public static List<LU_ChartOfAccountsClassificationTypes> GetAllChartOfAccountsClassificationTypes()
        {
            string key = "LU_ChartOfAccountsClassificationTypes";
            List<LU_ChartOfAccountsClassificationTypes> list = null;
            if (!_memoryCache.TryGetValue(key, out list))
            {
                list = lookupRepository.GetAllChartOfAccountsClassificationTypes();
                _memoryCache.Set(key, list);
            }
            if (list != null)
                return CommonUtility.DeepClone<List<LU_ChartOfAccountsClassificationTypes>>(list);
            return null;
        }
        public static List<LU_PaymentDirectionsSubTypes> GetAllPaymentDirectionsSubTypes()
        {
            string key = "LU_PaymentDirectionsSubTypes";
            List<LU_PaymentDirectionsSubTypes> list = null;
            if (!_memoryCache.TryGetValue(key, out list))
            {
                list = lookupRepository.GetAllPaymentDirectionsSubTypes();
                _memoryCache.Set(key, list);
            }
            if (list != null)
                return CommonUtility.DeepClone<List<LU_PaymentDirectionsSubTypes>>(list);
            return null;
        }
        public static LU_DataDictionaryTypes GetDataDictionaryType(int DataDictionaryType_Id)
        {
            List<LU_DataDictionaryTypes> list = GetDataDictionaryTypes();
            if (list != null && list.Count > 0)
                return list.FirstOrDefault(c => c.DataDictionaryType_Id == DataDictionaryType_Id);
            return null;
        }

        public static List<LU_DataDictionaryTypes> GetDataDictionaryTypes()
        {
            string key = "LU_DataDictionaryTypes";
            List<LU_DataDictionaryTypes> list = null;
            if (!_memoryCache.TryGetValue(key, out list))
            {
                list = lookupRepository.GetDataDictionaryTypes();
                _memoryCache.Set(key, list);
            }
            if (list != null)
                return CommonUtility.DeepClone<List<LU_DataDictionaryTypes>>(list);
            return null;
        }
        public static List<LU_FileTypes> GetFileTypes()
        {
            string key = "LU_FileTypes";
            List<LU_FileTypes> list = null;
            if (!_memoryCache.TryGetValue(key, out list))
            {
                list = lookupRepository.GetFileTypes();
                _memoryCache.Set(key, list);
            }
            if (list != null)
                return CommonUtility.DeepClone<List<LU_FileTypes>>(list);
            return null;
        }
        public static LU_FileTypes GetFileType(byte FileType_Id)
        {
            List<LU_FileTypes> list = GetFileTypes();
            if (list != null && list.Count > 0)
                return list.Where(c => c.FileType_Id == FileType_Id && c.Status).FirstOrDefault();
            return null;
        }
        public static List<LU_DataDictionary> GetDataDictionaries()
        {
            string key = "LU_DataDictionary";
            List<LU_DataDictionary> list = null;
            if (!_memoryCache.TryGetValue(key, out list))
            {
                list = lookupRepository.GetDataDictionaries();
                _memoryCache.Set(key, list);
            }
            if (list != null)
                return CommonUtility.DeepClone<List<LU_DataDictionary>>(list);
            return null;
        }
        public static List<LU_DataDictionary> GetDataDictionaries(int DataDictionaryType_Id)
        {
            List<LU_DataDictionary> list = GetDataDictionaries();
            if (list != null && list.Count > 0)
                return list.Where(c => c.DataDictionaryType_Id == DataDictionaryType_Id).ToList();
            return null;
        }
        public static List<LU_HTMLControlTypes> GetHTMLControlTypes()
        {
            string key = "LU_HTMLControlTypes";
            List<LU_HTMLControlTypes> list = null;
            if (!_memoryCache.TryGetValue(key, out list))
            {
                list = lookupRepository.GetHTMLControlTypes();
                _memoryCache.Set(key, list);
            }
            if (list != null)
                return CommonUtility.DeepClone<List<LU_HTMLControlTypes>>(list);
            return null;
        }
        public static List<LU_ReportTemplates> GetAllReportTemplates()
        {
            string key = "LU_ReportTemplates";
            List<LU_ReportTemplates> list = null;
            if (!_memoryCache.TryGetValue(key, out list))
            {
                list = lookupRepository.GetAllReportTemplates();
                _memoryCache.Set(key, list);
            }
            if (list != null)
                return CommonUtility.DeepClone<List<LU_ReportTemplates>>(list);
            return null;
        }
        public static List<LU_AnchorTypes> GetAllAnchorTypes()
        {
            string key = "LU_AnchorTypes";
            List<LU_AnchorTypes> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllAnchorTypes();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_AnchorTypes>>(List);
            return null!;
        }
        public static List<LU_InstallmentBasisTypes> GetAllInstallmentBasisTypes()
        {
            string key = "LU_InstallmentBasisTypes";
            List<LU_InstallmentBasisTypes> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllInstallmentBasisTypes();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_InstallmentBasisTypes>>(List);
            return null!;
        }
        public static List<LU_IntervalUnitTypes> GetAllIntervalUnitTypes()
        {
            string key = "LU_IntervalUnitTypes";
            List<LU_IntervalUnitTypes> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAllIntervalUnitTypes();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_IntervalUnitTypes>>(List);
            return null!;
        }

        public static string GetClientSettings(long Client_Id, string SettingKey)
        {
            string key = $"GetClientSettings-{Client_Id}";
            List<ClientSettings> List = clientSettingsRepository.GetClientSettings(Client_Id, SettingKey);
            //if (!_memoryCache.TryGetValue(key, out List!))
            //{
            //    List = clientSettingsRepository.GetClientSettings(Client_Id);
            //    _memoryCache.Set(key, List);
            //}
            if(List?.Count() > 0)
            {
                return List.FirstOrDefault(x => x.SettingKey == SettingKey)?.SettingValue;
            }
            return null;
        }
        public static List<LU_AmountDueBasedOnTypes> GetAmountDueBasedOnTypes()
        {
            string key = "LU_AmountDueBasedOn";
            List<LU_AmountDueBasedOnTypes> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetAmountDueBasedOnTypes();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_AmountDueBasedOnTypes>>(List);
            return null!;
        }
        public static List<LU_CommissionBasisTypes> GetAllCommissionBasisTypes()
        {
            string key = "LU_CommissionBasisTypes";
            List<LU_CommissionBasisTypes> list = null;
            if (!_memoryCache.TryGetValue(key, out list))
            {
                list = lookupRepository.GetAllCommissionBasisTypes();
                _memoryCache.Set(key, list);
            }
            if (list != null)
                return CommonUtility.DeepClone<List<LU_CommissionBasisTypes>>(list);
            return null;
        }

        public static List<LU_SubmissionTypes> GetAllSubmissionTypes()
        {
            string key = "LU_SubmissionTypes";
            List<LU_SubmissionTypes> list = null;
            if (!_memoryCache.TryGetValue(key, out list))
            {
                list = lookupRepository.GetAllSubmissionTypes();
                _memoryCache.Set(key, list);
            }
            if (list != null)
                return CommonUtility.DeepClone<List<LU_SubmissionTypes>>(list);
            return null;
        }
        public static List<LU_CheckPaymentPrintStatus> GetCheckPaymentPrintStatus()
        {
            string key = "LU_CheckPayments";
            List<LU_CheckPaymentPrintStatus> List = null!;
            if (!_memoryCache.TryGetValue(key, out List!))
            {
                List = lookupRepository.GetCheckPaymentPrintStatus();
                _memoryCache.Set(key, List);
            }
            if (List != null!)
                return CommonUtility.DeepClone<List<LU_CheckPaymentPrintStatus>>(List);
            return null!;
        }
        public static List<LU_ChecKeeperSignatureTypes> GetAllChecKeeperSignatureTypes()
        {
            string key = "LU_AllChecKeeperSignatureTypes";
            List<LU_ChecKeeperSignatureTypes> list = null;
            if (!_memoryCache.TryGetValue(key, out list))
            {
                list = lookupRepository.GetAllChecKeeperSignatureTypes();
                _memoryCache.Set(key, list);
            }
            if (list != null)
                return CommonUtility.DeepClone<List<LU_ChecKeeperSignatureTypes>>(list);
            return null;
        }
        public static string GetApplicationSettings(string settingKey)
        {
            var List = clientSettingsRepository.GetApplicationSettings(settingKey);
            if (List?.Count() > 0)
            {
                return List.FirstOrDefault(x => x.SettingKey == settingKey)?.SettingValue;
            }
            return null;
        }
        public static List<LU_BillingTypes> GetAllClientBillingTypes(long client_Id)
        {
            string key = $"LU_ClientBillingTypes_{client_Id}";
            List<LU_BillingTypes> list = null;
            if (!_memoryCache.TryGetValue(key, out list))
            {
                list = lookupRepository.GetAllClientBillingTypes(client_Id);
                _memoryCache.Set(key, list);
            }
            if (list != null)
                return CommonUtility.DeepClone<List<LU_BillingTypes>>(list);
            return null;
        }
        public static List<LU_Actions> GetAllActions()
        {
            string key = "LU_Actions";
            List<LU_Actions> List = null;
            if (!_memoryCache.TryGetValue(key, out List))
            {
                List = lookupRepository.GetAllActions();
                _memoryCache.Set(key, List);
            }
            if (List != null)
                return CommonUtility.DeepClone<List<LU_Actions>>(List);
            return null;
        }
    }
}
