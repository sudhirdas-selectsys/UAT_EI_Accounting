using Accounting.Entities.DB.Lookups;
using Accounting.Entities.Internal;
using Accounting.Entities.Internal.Enums;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;

namespace Accounting.Utilities.Internal
{
    public class MasterLookup
    {

        public static IConfiguration _configuration { get; set; }
        static MasterLookup()
        {
            _configuration = WebAppReferenceUtility._configuration;
            _ServerDateFormat = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;//"MM/DD/YYYY";//
            #region BillingTypes
            _BillingTypes = new Enum_BillingTypes();
            var BillingTypeList = LookupUtility.GetAllBillingTypes();
            CommonUtility.BindLookupData(_BillingTypes, BillingTypeList.Select(c => new LookupEntity() { Name = c.Code, Value = c.BillingType_Id }).ToList());
            #endregion

            #region PaymentPlans
            _PaymentPlans = new Enum_PaymentPlans();
            var PaymentPlansList = LookupUtility.GetAllPaymentPlans();
            //CommonUtility.BindLookupData(_PaymentPlans, PaymentPlansList.Select(c => new LookupEntity() { Name = c.Code, Value = c.PaymentPlan_Id }).ToList());
            CommonUtility.BindLookupData(_PaymentPlans, (PaymentPlansList ?? new List<LU_PaymentPlans>()).Select(c => new LookupEntity() { Name = c.Code, Value = c.PaymentPlan_Id }).ToList());
            #endregion

            #region Roles
            _Roles = new Enum_Roles();
            var RolesList = LookupUtility.GetAllRoles();
            CommonUtility.BindLookupData(_Roles, RolesList.Select(c => new LookupEntity() { Name = c.Code, Value = c.Role_Id }).ToList());
            #endregion


            #region Sources
            _Sources = new Enum_Sources();
            var SourcesList = LookupUtility.GetAllSources();
            CommonUtility.BindLookupData(_Sources, SourcesList.Select(c => new LookupEntity() { Name = c.Code, Value = c.Source_Id }).ToList());
            #endregion

            _Directions = new Enum_Directions();
            var DirectionsList = LookupUtility.GetAllDirections();
            CommonUtility.BindLookupData(_Directions, DirectionsList.Select(c => new LookupEntity() { Name = c.Code, Value = c.Direction_Id }).ToList());

            _LineItemTypes = new Enum_LineItemTypes();
            var LineItemsList = LookupUtility.GetAllLineItemTypes();
            CommonUtility.BindLookupData(_LineItemTypes, LineItemsList.Select(c => new LookupEntity() { Name = c.Code, Value = c.LineItemType_Id }).ToList());

            _LOBs = new Enum_LOBs();
            var LobList = LookupUtility.GetAllLOBs();
            CommonUtility.BindLookupData(_LOBs, LobList.Select(c => new LookupEntity() { Name = c.Code, Value = c.LOB_Id }).ToList());

            _LineItems = new Enum_LineItems();
            var LineItems = LookupUtility.GetAllLineItems();
            CommonUtility.BindLookupData(_LineItems, LineItems.Select(c => new LookupEntity() { Name = c.Code, Value = c.LineItem_Id }).ToList());


            #region QBOSettings
            _QBOSettings = new Enum_QBOSettings();
            _QBOSettings.baseurl = "quickbooks.api.intuit.com";
            _QBOSettings.companyid = "9341454630928093";
            _QBOSettings.minorversion = "75";
            _QBOSettings.client_id = "ABSGAM8DxhzPDN3pSyDtXw9uT04cFSfINVmb7lDy3YPV4bQAak";
            _QBOSettings.client_secret = "jYjdMIkqCgp0qX8zBu04MbECRORVepAhlcRzzVbZ";
            _QBOSettings.TokenEndpoint = "https://oauth.platform.intuit.com/oauth2/v1/tokens/bearer";
            _QBOSettings.refresh_token = "RT1-176-H0-1756053513l5ugad86xlzpc41cygl8";
            #endregion

            _EntityTypes = new Enum_EntityTypes();
            var EntityTypesList = LookupUtility.GetAllEntityTypes();
            CommonUtility.BindLookupData(_EntityTypes, EntityTypesList.Select(c => new LookupEntity() { Name = c.Code, Value = c.EntityType_Id }).ToList());

            _PaymentDirections = new Enum_PaymentDirections();
            var PaymentDirectionsList = LookupUtility.GetAllPaymentDirections();
            CommonUtility.BindLookupData(_PaymentDirections, PaymentDirectionsList.Select(c => new LookupEntity() { Name = c.Code, Value = c.PaymentDirection_Id }).ToList());

            //_ChartOfAccounts = new Enum_ChartOfAccounts();
            //var ChartOfAccountList = LookupUtility.GetAllChartofAccounts();
            //CommonUtility.BindLookupData(_ChartOfAccounts, ChartOfAccountList.Select(c => new LookupEntity() { Name = c.Name, Value = c.ReferenceID }).ToList());

            #region _ServiceRequestTypes
            _ServiceRequestTypes = new Enum_ServiceRequestTypes();
            var ServiceRequestList = LookupUtility.GetAllServiceRequestTypes();
            CommonUtility.BindLookupData(_ServiceRequestTypes, ServiceRequestList.Select(c => new LookupEntity() { Name = c.Name, Value = c.ServiceRequestType_Id }).ToList());
            #endregion

            #region _Services
            _Services = new Enum_Services();
            var ServicesList = LookupUtility.GetAllServices();
            CommonUtility.BindLookupData(_Services, ServicesList.Select(c => new LookupEntity() { Name = c.Name, Value = c.Service_Id }).ToList());
            #endregion

            #region _Parameters
            _Parameters = new Enum_Parameters();
            var ParametersList = LookupUtility.GetAllParameters();
            CommonUtility.BindLookupData(_Parameters, ParametersList.Select(c => new LookupEntity() { Name = c.Name, Value = c.Parameter_Id }).ToList());
            #endregion

            #region _DataTypes
            _DataTypes = new Enum_DataTypes();
            var DataTypeList = LookupUtility.GetAllDataTypes();
            CommonUtility.BindLookupData(_DataTypes, DataTypeList.Select(c => new LookupEntity() { Name = c.Name, Value = c.DataType_Id }).ToList());
            #endregion

            #region _ServiceProviderTypes
            _ServiceProviderTypes = new Enum_ServiceProviderTypes();
            var ServiceProviderTypeList = LookupUtility.GetAllServiceProviderTypes();
            CommonUtility.BindLookupData(_ServiceProviderTypes, ServiceProviderTypeList.Select(c => new LookupEntity() { Name = c.Name, Value = c.ServiceProviderType_Id }).ToList());
            #endregion

            #region _JournalEntryStatus
            _JournalEntryStatus = new Enum_JournalEntryStatus();
            var JournalEntryStatusList = LookupUtility.GetAllJournalEntryStatus();
            CommonUtility.BindLookupData(_JournalEntryStatus, JournalEntryStatusList.Select(c => new LookupEntity() { Name = c.Name, Value = c.JournalEntryStatus_Id }).ToList());
            #endregion

            _DefaultPageSize = 20;
            _DefaultPageNumber = 1;
            _EIBrokerStatementDocType = 1089;
            #region AccountingTypes
            _AccountingTypes = new Enum_AccountingTypes();
            var AccountingTypeList = LookupUtility.GetAllAccountingTypes();
            CommonUtility.BindLookupData(_AccountingTypes, AccountingTypeList.Select(c => new LookupEntity() { Name = c.Code, Value = c.AccountingType_Id }).ToList());
            #endregion

            #region CommissionTypes
            _CommissionTypes = new Enum_CommissionTypes();
            var CommissionTypeList = LookupUtility.GetAllCommissionTypes();
            CommonUtility.BindLookupData(_CommissionTypes, CommissionTypeList.Select(c => new LookupEntity() { Name = c.Code, Value = c.CommissionType_Id }).ToList());
            #endregion
            #region CommissionBasisTypes
            _CommissionBasisTypes = new Enum_CommissionBasisTypes();
            var CommissionBasisTypeList = LookupUtility.GetAllCommissionBasisTypes();
            CommonUtility.BindLookupData(_CommissionBasisTypes, CommissionBasisTypeList.Select(c => new LookupEntity() { Name = c.Code, Value = c.CommissionBasisType_Id }).ToList());
            #endregion

            #region ChartOfAccountsClassificationTypes
            _ChartOfAccountsClassificationTypes = new Enum_ChartOfAccountsClassificationTypes();
            var ChartOfAccountsClassificationTypeList = LookupUtility.GetAllChartOfAccountsClassificationTypes();
            CommonUtility.BindLookupData(_ChartOfAccountsClassificationTypes, ChartOfAccountsClassificationTypeList.Select(c => new LookupEntity() { Name = c.Code, Value = c.ChartOfAccountsClassificationType_Id }).ToList());
            #endregion

            #region PaymentDirectionsSubTypes
            _PaymentDirectionsSubTypes = new Enum_PaymentDirectionsSubTypes();
            var PaymentDirectionsSubTypeList = LookupUtility.GetAllPaymentDirectionsSubTypes();
            CommonUtility.BindLookupData(_PaymentDirectionsSubTypes, PaymentDirectionsSubTypeList.Select(c => new LookupEntity() { Name = c.Code, Value = c.PaymentDirectionsSubType_Id }).ToList());
            #endregion

            #region LUFileTypes

            _LUFileTypes = new LUFileTypes();
            var lu_FileTypes = LookupUtility.GetFileTypes();
            CommonUtility.BindLookupData(_LUFileTypes, lu_FileTypes.Select(c => new LookupEntity()
            {
                Name = c.Name,
                Value = c.FileType_Id.ToString()
            }).ToList());

            #endregion

            #region LUHTMLControlTypes

            _LUHTMLControlTypes = new LUHTMLControlTypes();
            var lu_HTMLControlTypes = LookupUtility.GetHTMLControlTypes();
            CommonUtility.BindLookupData(_LUHTMLControlTypes, lu_HTMLControlTypes.Select(c => new LookupEntity()
            {
                Name = c.Name,
                Value = c.HTMLControlType_Id.ToString()
            }).ToList());

            #endregion
            #region LU_ReportTemplates
            _ReportTemplates = new Enum_ReportTemplates();
            var ReportTemplatesList = LookupUtility.GetAllReportTemplates();
            CommonUtility.BindLookupData(_ReportTemplates, ReportTemplatesList.Select(c => new LookupEntity() { Name = c.Code, Value = c.ReportTemplate_Id }).ToList());
            #endregion

            #region PredefinedParameters
            _PredefinedParameters = new Enum_PredefinedParameters();
            var PredefinedParametersList = LookupUtility.GetAllPredefinedParameters();
            CommonUtility.BindLookupData(_PredefinedParameters, PredefinedParametersList.Select(c => new LookupEntity() { Name = c.Name, Value = c.PredefinedParameter_Id }).ToList());
            #endregion

            #region Reports
            _Reports = new Enum_Reports()
            {
                InvoiceProduction = 1,
                AgencyBill = 2,
                DirectBill = 3,
                Payment = 4,
                Transaction = 5,
                AccountReceivable=6,
                AccountPayable=7,
                TaxFiling = 8,
                InsuredBill = 9,
                JournalEntry = 10,
                Report1099 = 11,
                AgencyDetailed = 12,
                Underwriter = 13,
                BDXReport = 14,
                DirectInsuredBill=15,
                EarnedUnearnedPremiumCommission = 16,
                PaidBDXReport = 17,
            };
            #endregion

            #region Installment Plan

            //_InstallmentBasisTypes = new Enum_InstallmentBasisTypes();
            //var InstallmentBasisTypeList = LookupUtility.GetAllInstallmentBasisTypes();
            //CommonUtility.BindLookupData(_InstallmentBasisTypes, InstallmentBasisTypeList.Select(c => new LookupEntity() { Name = c.Code, Value = c.InstallmentBasisType_Id }).ToList());

            //_IntervalUnitTypes = new Enum_IntervalUnitTypes();
            //var IntervalUnitTypesList = LookupUtility.GetAllIntervalUnitTypes();
            //CommonUtility.BindLookupData(_IntervalUnitTypes, IntervalUnitTypesList.Select(c => new LookupEntity() { Name = c.Code, Value = c.IntervalUnitType_Id }).ToList());

            //_IntervalUnitTypes = new Enum_IntervalUnitTypes()
            //{
            //    Days = 1,
            //    Months = 2,
            //};
            //_InstallmentBasisTypes = new Enum_InstallmentBasisTypes()
            //{
            //    EffectiveDate = 1,
            //    DateBilled = 2,
            //    DayOfMonth = 3,
            //};
            #endregion
            _PDFCoverterAPIURL = _configuration["PDFCoverterAPIURL"]!;
			_CustomLineitems = new Enums_CustomLineitems();

            #region PaymentsUploadColumnPropertyMappings
            _PaymentsUploadColumnPropertyMappings = new Enum_PaymentsUploadColumnPropertyMappings();
            #endregion

            #region PaymentsUploadErrorMessages
            _PaymentsUploadErrorMessages = new Enum_PaymentsUploadErrorMessages();
            #endregion
            #region PaymentStatus
            _PaymentStatus = new Enum_PaymentStatus();
            var PaymentStatusList = LookupUtility.GetAllPaymentStatus();
            CommonUtility.BindLookupData(_PaymentStatus, PaymentStatusList.Select(c => new LookupEntity() { Name = c.Code, Value = c.PaymentStatus_Id }).ToList());
            #endregion

            #region PaymentTypes
            _PaymentTypes = new Enum_PaymentTypes();
            var PaymentTypesList = LookupUtility.GetAllPaymentTypes();
            CommonUtility.BindLookupData(_PaymentTypes, PaymentTypesList.Select(c => new LookupEntity() { Name = c.Code, Value = c.PaymentType_Id }).ToList());
            #endregion
            _EITransactionTypes = new Enum_EITransactionTypes();
            _EILineTypes = new Enum_EILineTypes();
            _AccountNumberPattern = @"^(\d+)-([A-Z]+)-(\d+)$";
            _AccountingCodes = new Enum_AccountingCodes();
            _ePayPaymentTypeCodes = new Enum_ePayPaymentTypeCodes();
            _epayDefaultPaymentTypeCodes = new Enum_EpayDefaultPaymentTypeCodes();
            _ClientSettings = new Enum_ClientSettings();
            _EI_EWSDetails = new Enum_EI_EWSDetails()
            {
                Service_URL = _configuration["EI_EWSDetails:Service_URL"]!,
                Token = _configuration["EI_EWSDetails:Token"]!,
                App_Id = _configuration["EI_EWSDetails:App_Id"]!,
                App_IdPW = _configuration["EI_EWSDetails:App_IdPW"]!,
                Support_CCEmailAddress = _configuration["EI_EWSDetails:Support_CCEmailAddress"]!,
                EI_NoReplyEmailAddress = _configuration["EI_EWSDetails:EI_NoReplyEmailAddress"]!,
                Support_BCCEmailAddresses = _configuration["EI_EWSDetails:Support_BCCEmailAddresses"]!,
            };
            _GeneralSettings = new Enum_GeneralSettings()
            {
                Currency_Format = _configuration["GeneralSettings:Currency_Format"],
                Culture_Info = _configuration["GeneralSettings:Culture_Info"],
                MathRoundPrecision = Convert.ToInt32(_configuration["GeneralSettings:MathRoundPrecision"]),
            };
            _Settings = new Enum_Settings();

            _microsoftSSO = new MicrosoftSSO()
            {
                BaseURL = _configuration["Microsoft:BaseURL"],
                ApplicationID = _configuration["Microsoft:ApplicationID"],
                DirectoryID = _configuration["Microsoft:DirectoryID"]
            };

            SSORedirectURL = _configuration["SSO_RedirectURL"];
            _CustomLOBs = new Enum_CustomLOBs();
            _ProducerTypes = new Enum_ProducerTypes();
            ApplicationAuthToken = _configuration["ApplicationAuthToken"];
            LogsFolder = _configuration["FileLogger:LogsFolder"];
            _ApplicationSettings = new Enum_ApplicationSettings();
            _BillProcessWorkFlowStatus = new Enum_BillProcessWorkFlowStatus();
            #region
            _Actions = new Enums_Actions();
            var ActionsList = LookupUtility.GetAllActions();
            CommonUtility.BindLookupData(_Actions, ActionsList.Select(c => new LookupEntity() { Name = c.ActionName, Value = c.Action_Id }).ToList());
            #endregion
        }
        public static string _PDFCoverterAPIURL { get; set; }
        public static string _ServerDateFormat { get; set; }
        public static string _ReportDateFormat = "MMMM d, yyyy";
        public static Enum_BillingTypes _BillingTypes { get; set; }
        public static Enum_CommissionBasisTypes _CommissionBasisTypes { get; set; }
        public static Enum_PaymentPlans _PaymentPlans { get; set; }
        public static Enum_Roles _Roles { get; set; }
        public static Enum_Sources _Sources { get; set; }
        public static Enum_ProducerTypes _ProducerTypes { get; set; }
        public static Enum_Directions _Directions { get; set; }
        public static Enum_LineItemTypes _LineItemTypes { get; set; }
        public static Enum_LineItems _LineItems { get; set; }
        public static Enum_QBOSettings _QBOSettings { get; set; }
        public static Enum_LOBs _LOBs { get; set; }
        public static Enum_CustomLOBs _CustomLOBs { get; set; }
        public static Enum_EntityTypes _EntityTypes { get; set; }
        public static Enum_PaymentDirections _PaymentDirections { get; set; }
        public static Enum_ChartOfAccounts _ChartOfAccounts { get; set; }
        public static Enum_ServiceRequestTypes _ServiceRequestTypes { get; set; }
        public static Enum_Services _Services { get; set; }
        public static Enum_Parameters _Parameters { get; set; }
        public static Enum_DataTypes _DataTypes { get; set; }
        public static Enum_ServiceProviderTypes _ServiceProviderTypes { get; set; }
        public static Enum_JournalEntryStatus _JournalEntryStatus { get; set; }
        public static int _DefaultPageSize { get; set; }
        public static int _EIBrokerStatementDocType { get; set; }

        public static int _DefaultPageNumber { get; set; }
        public static Enum_AccountingTypes _AccountingTypes { get; set; }
        public static Enum_ChartOfAccountsClassificationTypes _ChartOfAccountsClassificationTypes { get; set; }
        public static Enum_PaymentDirectionsSubTypes _PaymentDirectionsSubTypes { get; set; }
        public static LUFormatTypes _FormatTypes { get; set; }
        public static LUFileTypes _LUFileTypes { get; set; }
        public static LUPredefinedParameters _LUPredefinedParameters { get; set; }
        public static LUHTMLControlTypes _LUHTMLControlTypes { get; set; }
        public static Enum_ReportTemplates _ReportTemplates { get; set; }
        public static Enum_PredefinedParameters _PredefinedParameters { get; set; }
        public static Enum_Reports _Reports { get; set; }
        public static Enum_InstallmentBasisTypes _InstallmentBasisTypes { get; set; }
        public static Enum_IntervalUnitTypes _IntervalUnitTypes { get; set; }
        public static Enums_CustomLineitems _CustomLineitems { get; set; }
        public static Enum_PaymentsUploadColumnPropertyMappings _PaymentsUploadColumnPropertyMappings { get; set; }
        public static Enum_PaymentsUploadErrorMessages _PaymentsUploadErrorMessages { get; set; }
        public static Enum_EITransactionTypes _EITransactionTypes { get; set; }
        public static Enum_EILineTypes _EILineTypes { get; set; }
        public static string _AccountNumberPattern { get; set; }
        public static Enum_AccountingCodes _AccountingCodes { get; set; }
        public static Enum_PaymentStatus _PaymentStatus { get; set; }
        public static Enum_PaymentTypes _PaymentTypes { get; set; }
        public static Enum_ePayPaymentTypeCodes _ePayPaymentTypeCodes { get; set; }
        public static Enum_EpayDefaultPaymentTypeCodes _epayDefaultPaymentTypeCodes { get; set; }
        public static Enum_ClientSettings _ClientSettings { get; set; }
        public static Enum_EI_EWSDetails _EI_EWSDetails { get; set; }
        public static Enum_Settings _Settings { get; set; }
        public static Enum_GeneralSettings _GeneralSettings { get; set; }
        public static MicrosoftSSO _microsoftSSO { get; set; }
        public static Enum_CommissionTypes _CommissionTypes { get; set; }
        public static string SSORedirectURL { get; set; }
        public static string ApplicationAuthToken { get; set; }
        public static string LogsFolder { get; set; }
        public static Enum_ApplicationSettings _ApplicationSettings { get; set; }
        public static Enum_BillProcessWorkFlowStatus _BillProcessWorkFlowStatus { get; set; }
        public static Enums_Actions _Actions { get; set; }
    }
    public class MasterLookupDynamic
    {
        public MasterLookupDynamic()
        {
            _Directions = MasterLookup._Directions;
            _LUFileTypes = MasterLookup._LUFileTypes;
            _EntityTypes = MasterLookup._EntityTypes;
            _Services = MasterLookup._Services;
            _DefaultPageSize = MasterLookup._DefaultPageSize;
            _EIBrokerStatementDocType = MasterLookup._EIBrokerStatementDocType;
            _DefaultPageNumber = MasterLookup._DefaultPageNumber;
            _BillingTypes = MasterLookup._BillingTypes;
            _InstallmentBasisTypes = MasterLookup._InstallmentBasisTypes;
            _IntervalUnitTypes = MasterLookup._IntervalUnitTypes;
            _CustomLineItems = MasterLookup._CustomLineitems;
            _LineItemTypes = MasterLookup._LineItemTypes;
            _Roles = MasterLookup._Roles;
        }
        public Enum_Directions _Directions { get; set; }
        public LUFileTypes _LUFileTypes { get; set; }
        public Enum_EntityTypes _EntityTypes { get; set; }
        public Enum_Services _Services { get; set; }
        public Enum_InstallmentBasisTypes _InstallmentBasisTypes { get; set; }
        public Enum_IntervalUnitTypes _IntervalUnitTypes { get; set; }
        public int _DefaultPageSize { get; set; }
        public int _EIBrokerStatementDocType { get; set; }
        public int _DefaultPageNumber { get; set; }
        public Enum_BillingTypes _BillingTypes { get; set; }
        public Enums_CustomLineitems _CustomLineItems { get; set; }
        public Enum_LineItemTypes _LineItemTypes { get; set; }
        public Enum_Roles _Roles { get; set; }
    }
    public class LUFormatTypes
    {
        public byte Date { get; set; }
        public byte Currency { get; set; }
        public byte CurrencyWithoutDecimalDigits { get; set; }
        public byte Percent { get; set; }
        public byte ApendPercent { get; set; }
    }
    public class LUFileTypes
    {
        public byte Excel { get; set; }
        public byte Word { get; set; }
        public byte CSV { get; set; }
        public byte PDF { get; set; }
        public byte AgentSpecificPDF { get; set; }
    }
    public class LUPredefinedParameters
    {
        public byte IMSUserID { get; set; }
        public byte IMSUserGUID { get; set; }
    }
    public class LUHTMLControlTypes
    {
        public byte Text { get; set; }
        public byte Textarea { get; set; }
        public byte Radiobutton { get; set; }
        public byte Checkbox { get; set; }
        public byte Select { get; set; }
        public byte Date { get; set; }
        public byte DateTime { get; set; }
        public byte MultiSelect { get; set; }
    }
    public class MicrosoftSSO
    {
        public string ApplicationID { get; set; }
        public string DirectoryID { get; set; }
        public string BaseURL { get; set; }
    }
}
