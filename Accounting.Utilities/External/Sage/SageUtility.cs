using Accounting.DBUtilities.Implementations.Lookups;
using Accounting.DBUtilities.Implementations.Transactions;
using Accounting.DBUtilities.Interfaces.Lookups;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.External.Sage;
using Accounting.Entities.External.Sage.Journal;
using Accounting.Entities.Internal;
using Accounting.Utilities.Internal;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Accounting.Utilities.External.Sage
{
    public class SageUtility
    {
        public static IMemoryCache _MemoryCache { get; set; }
        public ServiceLogInfo _ServiceLogInfo { get; set; }
        public byte _Service_Id { get; set; }
        public long _Client_Id { get; set; }
        public byte _AccountingType_Id { get; set; }
        public long _User_Id { get; set; }

        SageParameters _SageParameters = new SageParameters();
        //private readonly HttpClient _httpClient;
        IInvoiceRepository _InvoiceRepository = new InvoiceRepository();
        ILookupRepository _LookupRepository = new LookupRepository();
        IPaymentsRepository _PaymentsRepository = new PaymentsRepository();
        IServiceLogInfoRepository _ServiceLogInfoRepository = new ServiceLogInfoRepository();
        ILOBCarrierGroupingRepository _LobCarrierGroupingRepository = new LOBCarrierGroupingRepository();
        IPolicyDetailsRepository _PolicyDetailsRepository = new PolicyDetailsRepository();
        IJournalEntryLogRepository _JournalEntryLogRepository = new JournalEntryLogRepository();
        internal readonly ILOBCarrierGroupingRepository _lobcarriergroupingRepository = new LOBCarrierGroupingRepository();
        internal readonly ILU_ClientLevelSageConfigurationsRepository ClientLevelSageConfigurations = new LU_ClientLevelSageConfigurationsRepository();
        SageFault fault { get; set; }
        public SageUtility(long Client_Id, long User_Id, byte Service_Id, byte AccountingType_Id)
        {
            _MemoryCache = WebAppReferenceUtility._memoryCache;
            _Client_Id = Client_Id;
            _Service_Id = Service_Id;
            _User_Id = User_Id;
            _AccountingType_Id = AccountingType_Id;
            var ClientLevelParameters = LookupUtility.GetAllClientLevelParameters(_Client_Id).Where(x => x.Service_Id == _Service_Id);
            var ServiceLevelParameters = LookupUtility.GetAllServiceLevelParameters().Where(x => x.Service_Id == _Service_Id);
            _SageParameters.BaseURL = ServiceLevelParameters?.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.BaseURL)?.DefaultValue;
            _SageParameters.Company_Id = ClientLevelParameters?.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.Company_Id)?.Value;
            _SageParameters.Sender_Id = ClientLevelParameters?.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.Sender_Id)?.Value;
            _SageParameters.Client_Id = ClientLevelParameters?.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.Client_Id)?.Value;
            _SageParameters.Sender_Password = ClientLevelParameters?.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.Sender_Password)?.Value;
            _SageParameters.User_Id = ClientLevelParameters?.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.User_Id)?.Value;
            _SageParameters.User_Password = ClientLevelParameters?.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.User_Password)?.Value;
        }
        public async Task<string> CallIntacct(ServiceLogInfo _ServiceLogInfo, string functionXml)
        {
            var session = await GetSessionFromCacheAsync(_ServiceLogInfo);
            var response = ExecuteAsync(_ServiceLogInfo, session.SessionId, session.Endpoint, functionXml);

            // Retry if session expired
            if (response.Contains("Session") && response.Contains("not valid"))
            {
                _MemoryCache.Remove(GetCacheKey());
                session = await GetSessionFromCacheAsync(_ServiceLogInfo, true);
                response = ExecuteAsync(_ServiceLogInfo, session.SessionId, session.Endpoint, functionXml);

            }

            return response;
        }

        private async Task<SageSession> GetSessionFromCacheAsync(ServiceLogInfo _ServiceLogInfo, bool Is_Retry = false)
        {
            string key = GetCacheKey();
            if (_MemoryCache.TryGetValue(key, out SageSession cachedSession))
            {
                return CommonUtility.DeepClone<SageSession>(cachedSession);
            }

            ServiceLogInfo latestServiceLog = _ServiceLogInfoRepository.GetServiceLogInfo(_Service_Id, _Client_Id, MasterLookup._ServiceRequestTypes.GetAccessToken);
            if (latestServiceLog != null && !string.IsNullOrWhiteSpace(latestServiceLog.Response) && !Is_Retry)
            {
                var doc = XDocument.Parse(latestServiceLog.Response);

                var sessionId = doc.Descendants("sessionid").FirstOrDefault()?.Value;
                var endpoint = doc.Descendants("endpoint").FirstOrDefault()?.Value;
                var expiryStr = doc.Descendants("sessiontimeout").FirstOrDefault()?.Value;
                var sessionTimestampStr = doc.Descendants("sessiontimestamp").FirstOrDefault()?.Value;

                if (!string.IsNullOrWhiteSpace(sessionId))
                {
                    var session = new SageSession
                    {
                        SessionId = sessionId,
                        Endpoint = endpoint,
                        SessionTimestamp = DateTime.Parse(sessionTimestampStr),
                        Expiry = DateTime.Parse(expiryStr)
                    };

                    // 3. Check if still valid
                    if (session.Expiry > DateTime.UtcNow)
                    {
                        var cacheDuration = (session.Expiry - DateTime.UtcNow);
                        _MemoryCache.Set(key, session, cacheDuration);
                        return CommonUtility.DeepClone<SageSession>(session);
                    }
                }
            }
            var newSession = await GetSessionAsync(_ServiceLogInfo);

            var newCacheDuration = (newSession.Expiry - newSession.SessionTimestamp);
            _MemoryCache.Set(key, newSession, newCacheDuration);
            return CommonUtility.DeepClone<SageSession>(newSession);


            //if (!_MemoryCache.TryGetValue(key, out SageSession session))
            //{
            //    session = await GetSessionAsync(_ServiceLogInfo);

            //    var cacheDuration = session.Expiry - session.SessionTimestamp;

            //    //if (cacheDuration <= TimeSpan.Zero)
            //    //    cacheDuration = TimeSpan.FromMinutes(10);

            //    _MemoryCache.Set(key, session, cacheDuration);
            //}

            //return CommonUtility.DeepClone<SageSession>(session);
        }
        private string GetCacheKey()
        {
            return $"IntacctSession_{_Client_Id}";
        }
        private async Task<SageSession> GetSessionAsync(ServiceLogInfo serviceLogInfo)
        {
            serviceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.GetAccessToken
            };
            var xml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
                    <request>
                      <control>
                        <senderid>{_SageParameters.Sender_Id}</senderid>
                        <password><![CDATA[{_SageParameters.Sender_Password}]]></password>
                        <controlid>{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}</controlid>
                        <uniqueid>false</uniqueid>
                        <dtdversion>3.0</dtdversion>
                      </control>
                      <operation>
                        <authentication>
                          <login>
                            <userid>{_SageParameters.User_Id}</userid>
                            <companyid>{_SageParameters.Company_Id}</companyid>
                            <password><![CDATA[{_SageParameters.User_Password}]]></password>
                          </login>
                        </authentication>
                        <content>
                          <function controlid=""{Guid.NewGuid()}"">
                            <getAPISession/>
                          </function>
                        </content>
                      </operation>
                    </request>";
            serviceLogInfo.Request = xml;
            //var response = await _httpClient.PostAsync(
            //    _SageParameters.BaseURL,
            //    new StringContent(xml, Encoding.UTF8, "application/xml"));
            var response = HttpServiceUtility.ExecuteXMLPostRequest(serviceLogInfo, _SageParameters.BaseURL, xml);
            _ServiceLogInfoRepository.InsertServiceLogInfo(serviceLogInfo);
            var result = response;

            var doc = XDocument.Parse(result);

            var sessionId = doc.Descendants("sessionid").FirstOrDefault()?.Value;
            var endpoint = doc.Descendants("endpoint").FirstOrDefault()?.Value;
            var expiryStr = doc.Descendants("sessiontimeout").FirstOrDefault()?.Value;
            var sessionTimestampStr = doc.Descendants("sessiontimestamp").FirstOrDefault()?.Value;
            if (string.IsNullOrEmpty(sessionId))
                throw new Exception("Failed to get session from Intacct");

            return new SageSession
            {
                SessionId = sessionId,
                Endpoint = endpoint,
                SessionTimestamp = DateTime.Parse(sessionTimestampStr),
                Expiry = DateTime.Parse(expiryStr)
            };
        }
        public async Task<List<SageAccountsResponse>> GetAllAccounts_Intacct()
        {
            List<SageAccountsResponse> sageAccountsResponse = new List<SageAccountsResponse>();

            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.GetAllChartOfAccounts
            };

            string functionXml = $@"
                <function controlid=""{Guid.NewGuid()}"">
                  <readByQuery>
                    <object>GLACCOUNT</object>
                    <fields>ACCOUNTNO, TITLE, STATUS</fields>
                    <query></query>
                    <pagesize>1000</pagesize>
                  </readByQuery>
                </function>";

            string response = string.Empty;

            try
            {
                response = await CallIntacct(_ServiceLogInfo, functionXml);

                if (!string.IsNullOrWhiteSpace(response))
                {
                    sageAccountsResponse = ParseAccounts(response);
                }

                _ServiceLogInfo.Response = response;
                _ServiceLogInfo.Is_Success = true;
                _ServiceLogInfo.Message = "Success";
            }
            catch (Exception ex)
            {
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }

            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);

            return sageAccountsResponse;
        }

        private string ExecuteAsync(ServiceLogInfo serviceLogInfo, string sessionId, string endpoint, string functionXml)
        {
            var xml = $@"<request>
                      <control>
                        <senderid>{_SageParameters.Sender_Id}</senderid>
                        <password><![CDATA[{_SageParameters.Sender_Password}]]></password>
                        <controlid>{Guid.NewGuid()}</controlid>
                        <uniqueid>false</uniqueid>
                        <dtdversion>3.0</dtdversion>
                      </control>
                      <operation>
                        <authentication>
                          <sessionid>{sessionId}</sessionid>
                        </authentication>
                        <content>
                          {functionXml}
                        </content>
                      </operation>
                    </request>";
            serviceLogInfo.Request = xml;
            //var response = await _httpClient.PostAsync(endpoint, new StringContent(xml, Encoding.UTF8, "application/xml"));
            var response = HttpServiceUtility.ExecuteXMLPostRequest(_ServiceLogInfo, endpoint, xml);

            return response;
        }
        public async Task<string> CreatePaymentJournalEntry_Intacct(SqlTransaction PaymentsTransaction, Payments payments, List<PaymentApplicationLineItems> PaymentApplicationLineItemsList, JournalEntries journalEntries)
        {
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.CreateJournalEntry
            };
            var intacctLines = PrepareIntacctJournalLines(
                PaymentsTransaction,
                payments,
                PaymentApplicationLineItemsList);

            if (!intacctLines.Any())
                throw new Exception("No valid journal lines to post.");

            var xml = BuildIntacctJournalXml(
                journalEntries.JournalEntryName,
                DateTime.UtcNow,
                intacctLines);

            var response = await CallIntacct(_ServiceLogInfo, xml);

            return response;
        }
        public List<SageJournalLine> PrepareIntacctJournalLines(SqlTransaction PaymentsTransaction, Payments payments, List<PaymentApplicationLineItems> PaymentApplicationLineItemsList)
        {
            var intacctLines = new List<SageJournalLine>();

            var ChartOfAccountsLineItemsList = _LookupRepository.GetAllLineItems_ChartOfAccounts(_Service_Id, _Client_Id);
            var PaymentDirectionsCOAList = _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id);
            var PaymentDirectionsCOA = PaymentDirectionsCOAList.FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);
            var PaymentDirectionsCOAItem = LookupUtility.GetAllChartofAccounts()
                .FirstOrDefault(c => c.ChartOfAccounts_Id == PaymentDirectionsCOA?.StandardCOA_Id);

            byte _ChartOfAccountsClassificationType_Id = 0;

            foreach (var paymentApplicationLineItems in PaymentApplicationLineItemsList)
            {
                _AccountingType_Id = paymentApplicationLineItems.AccountingType_Id;

                if (_AccountingType_Id == MasterLookup._AccountingTypes.Cash)
                    _ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability;
                else
                    _ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.AccountsReceivableAccountsPayable;

                string DebitCOAValue = null;
                string CreditCOAValue = null;
                string DebitCOAName = null;
                string CreditCOAName = null;
                string DebitDescription = null;
                string CreditDescription = null;
                int? DebitCOA = null;
                int? CreditCOA = null;
                string PaymentType = null;
                var invoiceDetails = _InvoiceRepository.GetInvoiceDetailsByInvoiceDetail_Id(paymentApplicationLineItems.InvoiceLineItem_Id);
                var invoices = _InvoiceRepository.GetInvoicesByInvoice_Id(invoiceDetails.Invoice_Id);
                var policyDetails = _PolicyDetailsRepository.GetPolicyDetails(invoices.PolicyDetails_Id);

                var BillingLineItemsDirections = LookupUtility.GetAllLU_BillingType_LineItems_Directions()
                    .FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id);

                var LineItemsDirections = LookupUtility.GetAllLineItems_Directions()
                    .FirstOrDefault(x => x.LineItems_Directions_Id == BillingLineItemsDirections.LineItems_Directions_Id);

                var lineItems_Clients = LookupUtility.GetAllLineItems_Clients()
                    .FirstOrDefault(x => x.LineItems_Clients_Id == LineItemsDirections.LineItems_Clients_Id);

                var LineItems = LookupUtility.GetAllLineItems()
                    .FirstOrDefault(c => c.LineItem_Id == lineItems_Clients?.LineItem_Id);

                var LOB = LookupUtility.GetAllLOBs()
                    .FirstOrDefault(x => x.LOB_Id == invoiceDetails.LOB_Id);

                var lobcarriergroupingDetails = _LobCarrierGroupingRepository
                    .GetLOBCarrierGroupingDetails(LOB.LOB_Id, policyDetails.CarrierDetail_Id, _Service_Id, _Client_Id);

                var ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x =>
                    x.Client_Id == _Client_Id &&
                    x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id &&
                    x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id &&
                    x.ChartOfAccountsClassificationType_Id == _ChartOfAccountsClassificationType_Id);

                var ChartOfAccounts = LookupUtility.GetAllChartofAccounts()
                    .FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);
                PaymentType = _LookupRepository.GetAllPaymentTypes().FirstOrDefault(x => x.PaymentType_Id == payments.PaymentType_Id)?.Name;

                if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming &&
                    LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR &&
                    paymentApplicationLineItems.AppliedAmount > 0)
                {
                    DebitCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                    CreditCOAValue = ChartOfAccounts?.ReferenceID;

                    DebitDescription = $"Applying incoming payment of Id - {payments.Payment_Id}, ( {PaymentType} {payments.ReferenceNumber} {payments.EntityName} ) for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    CreditDescription = $"Applied to Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                    CreditCOAName = ChartOfAccounts?.Name;
                    DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                    CreditCOA = ChartOfAccounts?.ChartOfAccounts_Id;
                }
                else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing &&
                         LineItemsDirections.Direction_Id == MasterLookup._Directions.AP &&
                         paymentApplicationLineItems.AppliedAmount > 0)
                {
                    if (lineItems_Clients?.Is_Passthrough == true && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                    {
                        ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id
                        == MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability);
                        ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);
                    }
                    DebitCOAValue = ChartOfAccounts?.ReferenceID;
                    CreditCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                    DebitDescription = $"Applied to Expense Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    CreditDescription = $"Applying outgoing payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    DebitCOAName = ChartOfAccounts?.Name;
                    CreditCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.Name!;
                    DebitCOA = ChartOfAccounts?.ChartOfAccounts_Id;
                    CreditCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;

                }

                else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && LineItemsDirections.Direction_Id == MasterLookup._Directions.AP && paymentApplicationLineItems.AppliedAmount > 0)
                {
                    if (lineItems_Clients?.Is_Passthrough == true && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                    {
                        ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id
== MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability);
                        ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);
                    }
                    CreditCOAValue = ChartOfAccounts?.ReferenceID;
                    DebitCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                    CreditDescription = $"[Netting] Applied to Expense Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    DebitDescription = $"[Netting] Applying outgoing payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    CreditCOAName = ChartOfAccounts?.Name;
                    DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.Name!;
                    CreditCOA = ChartOfAccounts?.ChartOfAccounts_Id;
                    DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                }
                else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && LineItemsDirections.Direction_Id == MasterLookup._Directions.AR && paymentApplicationLineItems.AppliedAmount < 0)
                {
                    DebitCOAValue = ChartOfAccounts?.ReferenceID;
                    CreditCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                    DebitDescription = $"[REFUND] Applied to Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    CreditDescription = $"[REFUND] Applying outgoing payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    DebitCOAName = ChartOfAccounts?.Name;
                    CreditCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.Name!;
                    DebitCOA = ChartOfAccounts?.ChartOfAccounts_Id;
                    CreditCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                }
                else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && LineItemsDirections.Direction_Id == MasterLookup._Directions.AP && paymentApplicationLineItems.AppliedAmount < 0)
                {
                    if (lineItems_Clients?.Is_Passthrough == true && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                    {
                        ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id
== MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability);
                        ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);
                    }
                    DebitCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                    CreditCOAValue = ChartOfAccounts?.ReferenceID;
                    DebitDescription = $"[RECOVERY] Applying incoming payment of Id - {payments.Payment_Id}, ( {PaymentType} {payments.ReferenceNumber} {payments.EntityName} ) for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    CreditDescription = $"[RECOVERY] Applied to expense account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                    CreditCOAName = ChartOfAccounts?.Name;
                    DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                    CreditCOA = ChartOfAccounts?.ChartOfAccounts_Id!;
                }
                else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && LineItemsDirections.Direction_Id == MasterLookup._Directions.AR && paymentApplicationLineItems.AppliedAmount < 0)
                {
                    DebitCOAValue = ChartOfAccounts?.ReferenceID;
                    CreditCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                    DebitDescription = $"[Netting] Applied to Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    CreditDescription = $"[Netting] Applying incoming payment of Id - {payments.Payment_Id},( {PaymentType} {payments.ReferenceNumber} {payments.EntityName} ) for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                    CreditCOAName = ChartOfAccounts?.Name;
                    DebitCOA = ChartOfAccounts?.ChartOfAccounts_Id!;
                    CreditCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                }
                else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && LineItemsDirections.Direction_Id == MasterLookup._Directions.AR && paymentApplicationLineItems.AppliedAmount > 0)
                {
                    DebitCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                    CreditCOAValue = ChartOfAccounts?.ReferenceID;
                    CreditDescription = $"[Netting] Applied to Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    DebitDescription = $"[Netting] Applying incoming payment of Id - {payments.Payment_Id}, ( {PaymentType} {payments.ReferenceNumber} {payments.EntityName} ) for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    CreditCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                    DebitCOAName = ChartOfAccounts?.Name;
                    CreditCOA = ChartOfAccounts?.ChartOfAccounts_Id!;
                    DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                }
                else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && LineItemsDirections.Direction_Id == MasterLookup._Directions.AP && paymentApplicationLineItems.AppliedAmount < 0)
                {
                    if (lineItems_Clients?.Is_Passthrough == true && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                    {
                        ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id
                        == MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability);
                        ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);
                    }
                    DebitCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                    CreditCOAValue = ChartOfAccounts?.ReferenceID;
                    DebitDescription = $"[Netting] Applying outgoing payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    CreditDescription = $"[Netting] Applied to Expense Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    DebitCOAName = ChartOfAccounts?.Name;
                    CreditCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                    DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                    CreditCOA = ChartOfAccounts?.ChartOfAccounts_Id!;
                }

                if (string.IsNullOrEmpty(DebitCOAValue) || string.IsNullOrEmpty(CreditCOAValue))
                    continue;


                intacctLines.Add(new SageJournalLine
                {
                    AccountNo = DebitCOAValue,
                    Amount = Math.Abs(paymentApplicationLineItems.AppliedAmount),
                    IsDebit = true,
                    Description = DebitDescription,
                });

                intacctLines.Add(new SageJournalLine
                {
                    AccountNo = CreditCOAValue,
                    Amount = Math.Abs(paymentApplicationLineItems.AppliedAmount),
                    IsDebit = false,
                    Description = CreditDescription,
                });
            }

            return intacctLines;
        }
        public string BuildIntacctEntriesXml(List<SageJournalLine> lines, byte? LOB_ID = null,byte Action_Id = 0, long? Client_Id = null)
        {
            var configurations = ClientLevelSageConfigurations.GetClientLevelSageConfigurations(_Client_Id, Action_Id); 
            string location = configurations?.Location ?? string.Empty;
            string classId = configurations?.Class ?? string.Empty;
            string department = configurations?.Department ?? string.Empty;
            if (Client_Id.HasValue && LOB_ID.HasValue)
            {
                string LOBCarrierDescription = _lobcarriergroupingRepository.GetLOBCarrierDescriptionByLOB_Id(Client_Id ?? 0, LOB_ID ?? 0, MasterLookup._Services.Sage);
                if (!string.IsNullOrWhiteSpace(LOBCarrierDescription))
                {
                    var SpecificClassId = LookupUtility.GetClientSettings(Client_Id ?? 0, $"{MasterLookup._ClientSettings.HasSpecificClassId}_{LOBCarrierDescription}");
                    classId = SpecificClassId ?? classId;
                }
            }
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                sb.Append($@"
                <GLENTRY>
                    <ACCOUNTNO>{line.AccountNo}</ACCOUNTNO>
                    <LOCATION>{location}</LOCATION>
                    <CLASSID>{classId}</CLASSID>
                    <DEPARTMENT>{department}</DEPARTMENT>
                    <TR_TYPE>{(line.IsDebit ? 1 : -1)}</TR_TYPE>
                    <AMOUNT>{line.Amount}</AMOUNT>
                    <DESCRIPTION><![CDATA[{line.Description}]]></DESCRIPTION>
                </GLENTRY>");
            }
            return sb.ToString();
        }
        public string BuildIntacctJournalXml(string docNumber, DateTime txnDate, List<SageJournalLine> lines, string? ReferenceNumber = null, byte Action_Id = 0)
        {
            var sb = new StringBuilder();
            sb.Append($@"
    <function controlid=""{Guid.NewGuid()}"">
      <create>
        <GLBATCH>
          <JOURNAL>GJ</JOURNAL>
          <BATCH_DATE>{txnDate.Month:D2}/{txnDate.Day:D2}/{txnDate.Year}</BATCH_DATE>
          <BATCH_TITLE>{docNumber}</BATCH_TITLE>
          <REFERENCENO>{ReferenceNumber}</REFERENCENO>
          <ENTRIES>
            {BuildIntacctEntriesXml(lines,null,Action_Id)}
          </ENTRIES>
        </GLBATCH>
      </create>
    </function>");
            return sb.ToString();
        }
        public string BuildIntacctReversalEntriesXml(List<SageJournalLine> originalLines)
        {
            var sb = new StringBuilder();
            foreach (var line in originalLines)
            {
                sb.Append($@"
            <GLENTRY>
                <ACCOUNTNO>{line.AccountNo}</ACCOUNTNO>
                <LOCATION>100</LOCATION>
                <TR_TYPE>{(line.IsDebit ? -1 : 1)}</TR_TYPE>
                <AMOUNT>{line.Amount}</AMOUNT>
                <DESCRIPTION><![CDATA[Reversal: {line.Description}]]></DESCRIPTION>
            </GLENTRY>");
            }
            return sb.ToString();
        }
        public async Task<string> CreatePaymentJournalEntry_Intacct(SqlTransaction PaymentsTransaction, Payments payments, int journalIndex = 0)
        {
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.CreateJournalEntry
            };
            SageResponse createSageJournalResponse = new SageResponse();
            var intacctLines = PrepareCreatePaymentJournalLines_Intacct(
                PaymentsTransaction,
                payments);

            if (!intacctLines.Any())
                throw new Exception("No valid journal lines to post.");

            string docNumber = $"PMT{payments.Payment_Id} ({journalIndex})";

            var xml = BuildIntacctJournalXml(docNumber, payments.PaymentDate ?? DateTime.Now, intacctLines, payments.ReferenceNumber, MasterLookup._Actions.CreatePayment);

            var response = await CallIntacct(_ServiceLogInfo, xml);
            var serializer = new XmlSerializer(typeof(SageResponse));
            using (var reader = new StringReader(response))
            {
                createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
            }
            //  InsertJournalEntries needs to do..
            if (createSageJournalResponse != null && createSageJournalResponse?.Operation.Result?.Data?.GlBatch?.RecordNo != null)
            {
                try
                {
                    string RecordNo = createSageJournalResponse?.Operation.Result?.Data?.GlBatch?.RecordNo.ToString();
                    JournalEntries journalEntries = InsertJournalEntries(PaymentsTransaction, MasterLookup._Sources.Payment, payments.Payment_Id, RecordNo, journalIndex, docNumber);
                    if (journalEntries != null)
                    {
                        InsertJournalEntriesLog_Intacct(PaymentsTransaction, intacctLines, journalEntries);
                    }
                }
                catch (Exception ex)
                {
                    PaymentsTransaction.Rollback();
                    _ServiceLogInfo.Status = false;
                }
            }
            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);

            return response;
        }
        public JournalEntries InsertJournalEntries(SqlTransaction Transaction, byte SourceType_Id, long Source_Id, string JournalEntryId, int? JournalIndex, string JournalEntryName)
        {
            JournalEntries journalEntries = new JournalEntries();
            journalEntries.SourceType_Id = SourceType_Id;
            journalEntries.Source_Id = Source_Id;
            journalEntries.ExternalJournalEntryIdentifier = JournalEntryId;
            journalEntries.JournalIndex = JournalIndex;
            journalEntries.JournalEntryName = JournalEntryName;
            return _JournalEntryLogRepository.InsertJournalEntries(Transaction, journalEntries);
        }

        public List<SageJournalLine> PrepareCreatePaymentJournalLines_Intacct(SqlTransaction PaymentsTransaction, Payments payments)
        {
            var lines = new List<SageJournalLine>();

            string DebitCOAValue = null;
            string CreditCOAValue = null;
            string DebitCOAName = null;
            string CreditCOAName = null;
            string DebitDescription = null;
            string CreditDescription = null;
            int? DebitCOA = null;
            int? CreditCOA = null;
            string PaymentType = null;
            var OperatingBanksCOAList = _LookupRepository.GetAllOperatingBanksChartOfAccounts(_Service_Id, _Client_Id);
            var OperatingBankCOA = OperatingBanksCOAList.FirstOrDefault(x => x.OperatingBank_Id == payments.OperatingBank_Id);

            var OperatingBankCOAItem = _LookupRepository.GetChartOfAccountsByChartOfAccount_Id(OperatingBankCOA?.ChartOfAccounts_Id, _Client_Id)?.FirstOrDefault();


            //var OperatingBankCOAItem = LookupUtility.GetAllChartofAccounts()
            //    .FirstOrDefault(c => c.ChartOfAccounts_Id == OperatingBankCOA?.ChartOfAccounts_Id);

            var PaymentDirectionsCOAList = _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id);
            var PaymentDirectionsCOA = PaymentDirectionsCOAList.FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);

            var PaymentDirectionsCOAItem = _LookupRepository.GetChartOfAccountsByChartOfAccount_Id(PaymentDirectionsCOA?.StandardCOA_Id, _Client_Id).FirstOrDefault();

            //var PaymentDirectionsCOAItem = LookupUtility.GetAllChartofAccounts()
            //    .FirstOrDefault(c => c.ChartOfAccounts_Id == PaymentDirectionsCOA?.StandardCOA_Id);

            if (OperatingBankCOA == null || OperatingBankCOAItem == null)
                throw new Exception("Missing COA Configuration(Operating Bank)");

            if (PaymentDirectionsCOA == null || PaymentDirectionsCOAItem == null)
                throw new Exception("Missing COA Configuration(Payment direction)");
            PaymentType = _LookupRepository.GetAllPaymentTypes().FirstOrDefault(x => x.PaymentType_Id == payments.PaymentType_Id)?.Name;
            if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming)
            {
                DebitCOAValue = OperatingBankCOAItem?.ReferenceID;
                CreditCOAValue = PaymentDirectionsCOAItem?.ReferenceID;

                DebitDescription = $"Deposite ( {PaymentType} {payments.ReferenceNumber} {payments.EntityName} )";
                CreditDescription = $"Applied to Incoming Payments Account";
            }
            else
            {
                DebitCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                CreditCOAValue = OperatingBankCOAItem?.ReferenceID;

                DebitDescription = $"Applied to Outgoing Payments Account";
                CreditDescription = $"Record outgoing payment - reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName}";
            }

            DebitCOAName = LookupUtility.GetAllChartofAccounts()
                .FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name;

            CreditCOAName = LookupUtility.GetAllChartofAccounts()
                .FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.Name;

            DebitCOA = LookupUtility.GetAllChartofAccounts()
                .FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id;

            CreditCOA = LookupUtility.GetAllChartofAccounts()
                .FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id;

            lines.Add(new SageJournalLine
            {
                AccountNo = DebitCOAValue,
                Amount = payments.Amount,
                IsDebit = true,
                Description = DebitDescription,
            });

            lines.Add(new SageJournalLine
            {
                AccountNo = CreditCOAValue,
                Amount = payments.Amount,
                IsDebit = false,
                Description = CreditDescription,
            });

            return lines;
        }

        public List<SageJournalLine> PrepareCreateInvoiceJournalLines_Intacct(SqlTransaction Transaction, PolicyDetails policyDetails)
        {
            var lines = new List<SageJournalLine>();

            string DebitCOAValue = null;
            string CreditCOAValue = null;
            string DebitCOAName = null;
            string CreditCOAName = null;
            string DebitDescription = null;
            string CreditDescription = null;
            int? DebitCOA = null;
            int? CreditCOA = null;
            foreach (var item in policyDetails.Invoices.InvoicesDetailsList)
            {
                DebitCOAValue = null;
                CreditCOAValue = null;
                DebitCOAName = null;
                CreditCOAName = null;
                DebitDescription = null;
                CreditDescription = null;
                DebitCOA = null;
                CreditCOA = null;
                LU_BillingType_LineItems_Directions BillingLineItemsDirections = LookupUtility.GetAllLU_BillingType_LineItems_Directions().FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == item.BillingType_LineItems_Directions_Id);
                LU_LineItems_Directions LineItemsDirections = LookupUtility.GetAllLineItems_Directions().FirstOrDefault(x => x.LineItems_Directions_Id == BillingLineItemsDirections?.LineItems_Directions_Id)!;
                LU_LineItems_Clients lineItems_Clients = LookupUtility.GetAllLineItems_Clients().FirstOrDefault(x => x.LineItems_Clients_Id == LineItemsDirections?.LineItems_Clients_Id);
                LU_LineItems LineItems = LookupUtility.GetAllLineItems().FirstOrDefault(c => c.LineItem_Id == lineItems_Clients?.LineItem_Id);
                LU_LineItemTypes LineType = LookupUtility.GetAllLineItemTypes().FirstOrDefault(c => c.LineItemType_Id == LineItems.LineItemType_Id);
                LU_Directions Directions = LookupUtility.GetAllDirections().FirstOrDefault(c => c.Direction_Id == LineItemsDirections.Direction_Id);
                LU_LOBs LOB = LookupUtility.GetAllLOBs().FirstOrDefault(c => c.LOB_Id == item.LOB_Id)!;
                LOBCarrierGrouping lobcarriergroupingDetails = _LobCarrierGroupingRepository.GetLOBCarrierGroupingDetails(LOB.LOB_Id, policyDetails.CarrierDetail_Id, _Service_Id, _Client_Id);
                var LineItemsChartOfAccounts = _LookupRepository.GetAllLineItems_ChartOfAccounts(_Service_Id, _Client_Id);

                var errorDetail = BuildErrorDetail(Directions?.Code, LineItems?.Name, LOB?.Name, policyDetails.BillType);
                if (LineItemsDirections.Direction_Id == MasterLookup._Directions.AR || LineItemsDirections.Direction_Id == MasterLookup._Directions.AP)
                {
                    if (LineItemsDirections.Direction_Id == MasterLookup._Directions.AR)
                    {
                        var DebitLineItemCOA = LineItemsChartOfAccounts.FirstOrDefault(x => x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.BillingType_LineItems_Directions_Id == item.BillingType_LineItems_Directions_Id && x.ChartOfAccountsClassificationType_Id == MasterLookup._ChartOfAccountsClassificationTypes.AccountsReceivableAccountsPayable);
                        var CreditLineItemCOA = LineItemsChartOfAccounts.FirstOrDefault(x => x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.BillingType_LineItems_Directions_Id == item.BillingType_LineItems_Directions_Id && x.ChartOfAccountsClassificationType_Id == MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability);
                        var DebitLineItemCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == DebitLineItemCOA.ChartOfAccount_Id);
                        var CreditLineItemCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == CreditLineItemCOA.ChartOfAccount_Id);
                        if (DebitLineItemCOA == null || DebitLineItemCOAItem == null)
                        {
                            fault = AddError(fault, "Missing COA Configuration (AR/AP)", errorDetail);
                            continue;
                        }
                        if (CreditLineItemCOA == null || CreditLineItemCOAItem == null)
                        {
                            fault = AddError(fault, "Missing COA Configuration(Income/Expense)", errorDetail);
                            continue;
                        }
                        if (item.Amount > 0)
                        {
                            DebitCOAValue = DebitLineItemCOAItem?.ReferenceID;
                            DebitCOAName = DebitLineItemCOAItem?.Name;
                            CreditCOAValue = CreditLineItemCOAItem?.ReferenceID;
                            CreditCOAName = CreditLineItemCOAItem?.Name;
                            DebitDescription = $"Applied from Accounts Recievable account for invoice no. - {policyDetails.Invoices.InvoiceNo}, {LineItems.Name} ({Directions.Name}) with LOB - {LOB.Code}, and Carrier - {policyDetails.CarrierDetails.Name}";
                            CreditDescription = $"Applied to Income Account associated for for invoice no. - {policyDetails.Invoices.InvoiceNo}, {LineItems.Name} ({Directions.Name})";

                        }
                        else
                        {
                            CreditCOAValue = DebitLineItemCOAItem?.ReferenceID;
                            CreditCOAName = DebitLineItemCOAItem?.Name;
                            DebitCOAValue = CreditLineItemCOAItem?.ReferenceID;
                            DebitCOAName = CreditLineItemCOAItem?.Name;
                            CreditDescription = $"Applied from Accounts Recievable account for invoice no. - {policyDetails.Invoices.InvoiceNo}, {LineItems.Name} ({Directions.Name}) with LOB - {LOB.Code}, and Carrier - {policyDetails.CarrierDetails.Name}";
                            DebitDescription = $"Applied to Income Account associated for for invoice no. - {policyDetails.Invoices.InvoiceNo}, {LineItems.Name} ({Directions.Name})";
                        }

                    }
                    else if (LineItemsDirections.Direction_Id == MasterLookup._Directions.AP && (lineItems_Clients?.Is_Passthrough == false))
                    {
                        var DebitLineItemCOA = LineItemsChartOfAccounts.FirstOrDefault(x => x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.BillingType_LineItems_Directions_Id == item.BillingType_LineItems_Directions_Id && x.ChartOfAccountsClassificationType_Id == MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability);
                        var CreditLineItemCOA = LineItemsChartOfAccounts.FirstOrDefault(x => x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.BillingType_LineItems_Directions_Id == item.BillingType_LineItems_Directions_Id && x.ChartOfAccountsClassificationType_Id == MasterLookup._ChartOfAccountsClassificationTypes.AccountsReceivableAccountsPayable);
                        var DebitLineItemCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == DebitLineItemCOA.ChartOfAccount_Id);
                        var CreditLineItemCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == CreditLineItemCOA.ChartOfAccount_Id);
                        if (CreditLineItemCOA == null || CreditLineItemCOAItem == null)
                        {
                            fault = AddError(fault, "Missing COA Configuration(AR/AP)", errorDetail);
                            continue;
                        }
                        if (DebitLineItemCOA == null || DebitLineItemCOAItem == null)
                        {
                            fault = AddError(fault, "Missing COA Configuration(Income/Expense)", errorDetail);
                            continue;
                        }
                        if (item.Amount > 0)
                        {
                            DebitCOAValue = DebitLineItemCOAItem?.ReferenceID;
                            DebitCOAName = DebitLineItemCOAItem?.Name;
                            CreditCOAValue = CreditLineItemCOAItem?.ReferenceID;
                            CreditCOAName = CreditLineItemCOAItem?.Name;
                            DebitDescription = $"Applied from Expense Account for invoice no. - {policyDetails.Invoices.InvoiceNo}, {LineItems.Name} ({Directions.Name}) with LOB - {LOB.Code}, and Carrier - {policyDetails.CarrierDetails.Name}";
                            CreditDescription = $"Applied to Accounts Payable Account for invoice no. - {policyDetails.Invoices.InvoiceNo}, {LineItems.Name} ({Directions.Name}) with LOB - {LOB.Code}, and Carrier - {policyDetails.CarrierDetails.Name}";
                        }
                        else
                        {
                            CreditCOAValue = DebitLineItemCOAItem.ReferenceID;
                            CreditCOAName = DebitLineItemCOAItem.Name;
                            DebitCOAValue = CreditLineItemCOAItem.ReferenceID;
                            DebitCOAName = CreditLineItemCOAItem.Name;
                            CreditDescription = $"Applied from Expense Account for invoice no. - {policyDetails.Invoices.InvoiceNo}, {LineItems.Name} ({Directions.Name}) with LOB - {LOB.Code}, and Carrier - {policyDetails.CarrierDetails.Name}";
                            DebitDescription = $"Applied to Accounts Payable Account for invoice no. - {policyDetails.Invoices.InvoiceNo}, {LineItems.Name} ({Directions.Name}) with LOB - {LOB.Code}, and Carrier - {policyDetails.CarrierDetails.Name}";
                        }
                    }
                    if (!string.IsNullOrEmpty(DebitCOAValue) && !string.IsNullOrEmpty(CreditCOAValue))
                    {
                        double amount = Math.Abs(item.Amount);

                        // Debit
                        lines.Add(new SageJournalLine
                        {
                            AccountNo = DebitCOAValue,
                            Amount = amount,
                            IsDebit = true,
                            Description = DebitDescription,
                        });

                        // Credit
                        lines.Add(new SageJournalLine
                        {
                            AccountNo = CreditCOAValue,
                            Amount = amount,
                            IsDebit = false,
                            Description = CreditDescription,
                        });
                    }
                }
            }
            return lines;
        }
        public async Task<string> CreatePaymentWriteoffJournalEntry_Intacct(
     SqlTransaction PaymentsTransaction,
     Payments payments,
     PaymentApplications paymentApplications,
     PaymentApplicationLineItems paymentApplicationLineItems,
     JournalEntries journalEntries,
     bool Is_Update = true,
     bool Is_Delete = false)
        {
            SageResponse createSageJournalResponse = new SageResponse();

            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.CreateJournalEntry,
            };

            if ((Is_Update || Is_Delete) &&
                !string.IsNullOrEmpty(journalEntries?.ExternalJournalEntryIdentifier))
            {

                string readXml = $@"
            <function controlid=""{Guid.NewGuid()}"">
                <read>
                    <object>GLBATCH</object>
                    <keys>{journalEntries.ExternalJournalEntryIdentifier}</keys>
                    <fields>*</fields>
                </read>
            </function>";

                var readRes = await CallIntacct(_ServiceLogInfo, readXml);

                var serializer = new XmlSerializer(typeof(SageResponse));
                SageResponse originalBatch;

                using (var reader = new StringReader(readRes))
                {
                    originalBatch = (SageResponse)serializer.Deserialize(reader);
                }

                var originalEntries = originalBatch?.Operation?.Result?.Data?.GlBatchRead?.Entries;

                if (originalEntries == null || !originalEntries.Any())
                    throw new Exception("Could not read original GLBATCH entries for reversal.");
                var configurations = ClientLevelSageConfigurations.GetClientLevelSageConfigurations(_Client_Id, Is_Update ? MasterLookup._Actions.UpdatePaymentWriteoff : MasterLookup._Actions.DeletePaymentWriteoff);
                string location = configurations?.Location ?? string.Empty;
                string classId = configurations?.Class ?? string.Empty;
                string department = configurations?.Department ?? string.Empty;
                var reversedEntriesXml = string.Join("",
                    originalEntries.Select(e => $@"
<GLENTRY>
    <ACCOUNTNO>{e.AccountNo}</ACCOUNTNO>
    <LOCATION>{location}</LOCATION>
    <CLASSID>{classId}</CLASSID>
    <DEPARTMENT>{department}</DEPARTMENT>
    <TR_TYPE>{(e.TrType == 1 ? -1 : 1)}</TR_TYPE>
    <AMOUNT>{e.Amount}</AMOUNT>
    <DESCRIPTION><![CDATA[Reversal: {e.Description}]]></DESCRIPTION>
</GLENTRY>"));

                string docNumber = CommonUtility.NormalizeDocNumber(
                    journalEntries.JournalEntryName,
                    journalEntries.JournalIndex);

                string createFunctionXml = "";
                List<SageJournalLine> newLines = new();


                if (Is_Update)
                {
                    newLines = PreparePaymentWriteoffLines_Intacct(
                        PaymentsTransaction,
                        payments,
                        paymentApplications,
                        paymentApplicationLineItems,
                        journalEntries,
                        Is_Update,
                        Is_Delete);

                    if (!newLines.Any())
                        throw new Exception("No valid journal lines for updated entry.");

                    string newEntriesXml = BuildIntacctEntriesXml(newLines,null,MasterLookup._Actions.UpdatePaymentWriteoff);

                    createFunctionXml = $@"
<function controlid=""{Guid.NewGuid()}"">
    <create>
        <GLBATCH>
            <JOURNAL>GJ</JOURNAL>
            <BATCH_DATE>{DateTime.Now:MM/dd/yyyy}</BATCH_DATE>
            <BATCH_TITLE>{docNumber}</BATCH_TITLE>
            <REFERENCENO>{payments.ReferenceNumber}</REFERENCENO>
            <ENTRIES>
                {newEntriesXml}
            </ENTRIES>
        </GLBATCH>
    </create>
</function>";
                }
                else if (Is_Delete)
                {
                    SageResponse createSageJournalRes = new SageResponse();
                    var serialize = new XmlSerializer(typeof(SageResponse));
                    using (var reader = new StringReader(readRes))
                    {
                        createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                    }
                    var originalEntry = originalBatch?.Operation?.Result?.Data?.GlBatchRead?.Entries;

                    if (originalEntries == null || !originalEntries.Any())
                        throw new Exception("No original entries found to reverse.");

                    newLines = originalEntries.Select(x => new SageJournalLine
                    {
                        AccountNo = x.AccountNo,
                        Amount = Convert.ToDouble(x.Amount),
                        IsDebit = x.TrType == -1,
                        Description = $"Reversal: {x.Description}",
                        Department = "Admin"
                    }).ToList();

                    string reverseEntriesXml = BuildIntacctEntriesXml(newLines,null,MasterLookup._Actions.DeletePaymentWriteoff);

                    createFunctionXml = $@"
                <function controlid=""{Guid.NewGuid()}"">
                    <create>
                        <GLBATCH>
                            <JOURNAL>GJ</JOURNAL>
                            <BATCH_DATE>{DateTime.Now:MM/dd/yyyy}</BATCH_DATE>
                            <BATCH_TITLE>Reversal - {journalEntries.JournalEntryName}</BATCH_TITLE>
                            <REFERENCENO>{payments.ReferenceNumber}</REFERENCENO>
                            <ENTRIES>
                                {reverseEntriesXml}
                            </ENTRIES>
                        </GLBATCH>
                    </create>
                </function>";
                }
                string combinedFunctions = $@"
                <function controlid=""{Guid.NewGuid()}"">
                    <create>
                        <GLBATCH>
                            <JOURNAL>GJ</JOURNAL>
                            <BATCH_DATE>{DateTime.Now:MM/dd/yyyy}</BATCH_DATE>
                            <BATCH_TITLE>Reversal - {journalEntries.JournalEntryName}</BATCH_TITLE>
                            <REFERENCENO>{payments.ReferenceNumber}</REFERENCENO>
                            <ENTRIES>
                                {reversedEntriesXml}
                            </ENTRIES>
                        </GLBATCH>
                    </create>
                </function>
                {createFunctionXml}";

                var session = await GetSessionFromCacheAsync(_ServiceLogInfo);

                var res = ExecuteAsync(_ServiceLogInfo, session.SessionId, session.Endpoint, combinedFunctions);


                var xmlDoc = XDocument.Parse(res);

                var recordNos = xmlDoc.Descendants("RECORDNO")
                                      .Select(x => x.Value)
                                      .ToList();

                string? targetRecordNo = Is_Update
                    ? recordNos.LastOrDefault()
                    : recordNos.FirstOrDefault();

                if (!string.IsNullOrEmpty(targetRecordNo))
                {
                    int updatedJournalIndex = (journalEntries?.JournalIndex ?? 0) + 1;

                    var entry = InsertJournalEntries(
                        PaymentsTransaction,
                        MasterLookup._Sources.Payment,
                        payments.Payment_Id,
                        targetRecordNo,
                        updatedJournalIndex,
                        docNumber);

                    //if (!Is_Delete)
                    //{
                    InsertJournalEntriesLog_Intacct(
                        PaymentsTransaction,
                        newLines,
                        entry);
                    //}
                }

                _ServiceLogInfo.Response = res;
                _ServiceLogInfo.Is_Success = true;
                _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);

                return res;
            }
            else
            {
                var lines = PreparePaymentWriteoffLines_Intacct(
                    PaymentsTransaction,
                    payments,
                    paymentApplications,
                    paymentApplicationLineItems,
                    journalEntries,
                    Is_Update,
                    Is_Delete);

                if (!lines.Any())
                    throw new Exception("No valid journal lines.");

                string docNumber = CommonUtility.NormalizeDocNumber(
                    journalEntries.JournalEntryName,
                    journalEntries.JournalIndex);

                var xml = BuildIntacctJournalXml(
                    docNumber,
                    DateTime.Now,
                    lines,
                    payments.ReferenceNumber,
                    MasterLookup._Actions.CreatePaymentWriteoff);

                var response = await CallIntacct(_ServiceLogInfo, xml);

                var serializer = new XmlSerializer(typeof(SageResponse));
                using (var reader = new StringReader(response))
                {
                    createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                }

                if (createSageJournalResponse?.Operation?.Result?.Data?.GlBatch?.RecordNo != null)
                {
                    int updatedJournalIndex = (journalEntries?.JournalIndex ?? 0) + 1;
                    string recordNo = createSageJournalResponse.Operation.Result.Data.GlBatch.RecordNo.ToString();

                    var entry = InsertJournalEntries(
                        PaymentsTransaction,
                        MasterLookup._Sources.Payment,
                        payments.Payment_Id,
                        recordNo,
                        updatedJournalIndex,
                        docNumber);

                    InsertJournalEntriesLog_Intacct(PaymentsTransaction, lines, entry);
                }
                else
                {//Handle trancastion roll back here if fail...
                }

                _ServiceLogInfo.Response = response;
                _ServiceLogInfo.Is_Success = true;
                _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);

                return response;
            }
        }

        public async Task<string> CreateJournalEntry_Intacct(
    SqlTransaction PaymentsTransaction,
    Payments payments,
    List<PaymentApplicationLineItems> PaymentApplicationLineItemsList,
    JournalEntries journalEntries,
    bool Is_Apply = true,
    PaymentApplicationLineItems unApplyPaymentApplicationLineItems = null)
        {
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.CreateJournalEntry,
            };
            var readXml = $@"
<function controlid=""{Guid.NewGuid()}"">
    <read>
        <object>GLBATCH</object>
        <keys>{journalEntries.ExternalJournalEntryIdentifier}</keys>
        <fields>*</fields>
    </read>
</function>";

            var readRes = await CallIntacct(_ServiceLogInfo, readXml);

            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.CreateJournalEntry,
            };

            List<SageJournalLine> lines = new();

            try
            {
                if (Is_Apply)
                {
                    lines = PrepareCreateJournalLines_Intacct(
                        PaymentsTransaction,
                        payments,
                        PaymentApplicationLineItemsList,
                        journalEntries);

                    if (!lines.Any())
                        throw new ApplicationException("No valid journal lines found");
                }
                else
                {
                    lines = PrepareUnApplyJournalLines_Intacct(
                        PaymentsTransaction,
                        unApplyPaymentApplicationLineItems,
                        journalEntries);

                    if (!lines.Any())
                        throw new ApplicationException("No valid unapply journal lines found");
                }
            }
            catch (Exception ex)
            {
                PaymentsTransaction.Rollback();
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }

            try
            {
                string docNumber = CommonUtility.NormalizeDocNumber(
                    journalEntries.JournalEntryName,
                    journalEntries.JournalIndex);

                string entriesXml = BuildIntacctEntriesXml(lines,null, Is_Apply ? MasterLookup._Actions.ApplyPayment : MasterLookup._Actions.UnApply);

                string functionXml = $@"
<function controlid=""{Guid.NewGuid()}"">
    <create>
        <GLBATCH>
            <JOURNAL>GJ</JOURNAL>
            <BATCH_DATE>{DateTime.Now:MM/dd/yyyy}</BATCH_DATE>
            <BATCH_TITLE>{docNumber}</BATCH_TITLE>
            <REFERENCENO>{payments.ReferenceNumber}</REFERENCENO>
            <ENTRIES>
                {entriesXml}
            </ENTRIES>
        </GLBATCH>
    </create>
</function>";

                var response = await CallIntacct(_ServiceLogInfo, functionXml);

                var xmlDoc = XDocument.Parse(response);

                var recordNo = xmlDoc.Descendants("RECORDNO").FirstOrDefault()?.Value;

                if (!string.IsNullOrEmpty(recordNo))
                {
                    int updatedJournalIndex = (journalEntries?.JournalIndex ?? 0) + 1;

                    var entry = InsertJournalEntries(
                        PaymentsTransaction,
                        MasterLookup._Sources.Payment,
                        payments.Payment_Id,
                        recordNo,
                        updatedJournalIndex,
                        docNumber);

                    if (journalEntries != null)
                    {
                        InsertJournalEntriesLog_Intacct(
                            PaymentsTransaction,
                            lines,
                            entry);
                    }
                }

                _ServiceLogInfo.Response = response;
                _ServiceLogInfo.Is_Success = true;
            }
            catch (Exception ex)
            {
                PaymentsTransaction.Rollback();
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }

            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);

            return _ServiceLogInfo.Response;
        }
        public List<SageJournalLine> PrepareCreateJournalLines_Intacct(SqlTransaction PaymentsTransaction, Payments payments, List<PaymentApplicationLineItems> PaymentApplicationLineItemsList,
     JournalEntries journalEntries)
        {
            var lines = new List<SageJournalLine>();
            try
            {
                var ChartOfAccountsLineItemsList = _LookupRepository.GetAllLineItems_ChartOfAccounts(_Service_Id, _Client_Id);
                var PaymentDirectionsCOAList = _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id);
                byte _ChartOfAccountsClassificationType_Id = 0;
                var PaymentDirectionsCOA = PaymentDirectionsCOAList
                    .FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);

                var PaymentDirectionsCOAItem = LookupUtility.GetAllChartofAccounts()
                    .FirstOrDefault(c => c.ChartOfAccounts_Id == PaymentDirectionsCOA?.StandardCOA_Id);

                if (PaymentDirectionsCOA == null || PaymentDirectionsCOAItem == null)
                    throw new Exception("Missing COA Configuration (Payment Direction)");

                foreach (var item in PaymentApplicationLineItemsList)
                {
                    string DebitCOAValue = null;
                    string CreditCOAValue = null;
                    string DebitDescription = null;
                    string CreditDescription = null;
                    string PaymentType = null;
                    var invoiceDetails = _InvoiceRepository.GetInvoiceDetailsByInvoiceDetail_Id(item.InvoiceLineItem_Id);
                    var invoices = _InvoiceRepository.GetInvoicesByInvoice_Id(invoiceDetails.Invoice_Id);
                    var policyDetails = _PolicyDetailsRepository.GetPolicyDetails(invoices.PolicyDetails_Id);

                    var BillingLineItemsDirections = LookupUtility.GetAllLU_BillingType_LineItems_Directions()
                        .FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id);

                    var LineItemsDirections = LookupUtility.GetAllLineItems_Directions()
                        .FirstOrDefault(x => x.LineItems_Directions_Id == BillingLineItemsDirections.LineItems_Directions_Id);

                    var lineItems_Clients = LookupUtility.GetAllLineItems_Clients()
                        .FirstOrDefault(x => x.LineItems_Clients_Id == LineItemsDirections.LineItems_Clients_Id);

                    var LineItems = LookupUtility.GetAllLineItems()
                        .FirstOrDefault(c => c.LineItem_Id == lineItems_Clients?.LineItem_Id);

                    var LOB = LookupUtility.GetAllLOBs()
                        .FirstOrDefault(x => x.LOB_Id == invoiceDetails.LOB_Id);

                    var lobcarriergroupingDetails = _LobCarrierGroupingRepository
                        .GetLOBCarrierGroupingDetails(LOB.LOB_Id, policyDetails.CarrierDetail_Id, _Service_Id, _Client_Id);
                    PaymentType = _LookupRepository.GetAllPaymentTypes().FirstOrDefault(x => x.PaymentType_Id == payments.PaymentType_Id)?.Name;

                    if (item.AccountingType_Id == MasterLookup._AccountingTypes.Cash)
                    {
                        _ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability;
                    }
                    else
                    {
                        _ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.AccountsReceivableAccountsPayable;
                    }
                    var ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x =>
                        x.Client_Id == _Client_Id &&
                        x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id &&
                        x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id &&
                        x.ChartOfAccountsClassificationType_Id == _ChartOfAccountsClassificationType_Id);

                    var ChartOfAccounts = LookupUtility.GetAllChartofAccounts()
                        .FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);

                    if (ChartOfAccounts == null)
                        throw new Exception("Missing COA Configuration (line-item)");

                    if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR && item.AppliedAmount > 0)
                    {
                        DebitCOAValue = PaymentDirectionsCOAItem.ReferenceID;
                        CreditCOAValue = ChartOfAccounts.ReferenceID;

                        DebitDescription = $"Applying incoming payment of Id - {payments.Payment_Id}, (  {PaymentType}   {payments.ReferenceNumber}   {payments.EntityName}  ) for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                        CreditDescription = $"Applied to Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    }
                    else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && LineItemsDirections.Direction_Id == MasterLookup._Directions.AP && item.AppliedAmount > 0)
                    {
                        if (lineItems_Clients?.Is_Passthrough == true && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                        {
                            ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id
                            == MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability);
                            ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);
                        }
                        DebitCOAValue = ChartOfAccounts?.ReferenceID;
                        CreditCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                        DebitDescription = $"Applied to Expense Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                        CreditDescription = $"Applying outgoing payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    }
                    else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && LineItemsDirections.Direction_Id == MasterLookup._Directions.AP && item.AppliedAmount > 0)
                    {
                        if (lineItems_Clients?.Is_Passthrough == true && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                        {
                            ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id
                            == MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability);
                            ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);
                        }
                        CreditCOAValue = ChartOfAccounts?.ReferenceID;
                        DebitCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                        CreditDescription = $"[Netting] Applied to Expense Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                        DebitDescription = $"[Netting] Applying outgoing payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    }
                    else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && LineItemsDirections.Direction_Id == MasterLookup._Directions.AR && item.AppliedAmount < 0)
                    {
                        DebitCOAValue = ChartOfAccounts?.ReferenceID;
                        CreditCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                        DebitDescription = $"[REFUND] Applied to Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                        CreditDescription = $"[REFUND] Applying outgoing payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    }
                    else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && LineItemsDirections.Direction_Id == MasterLookup._Directions.AP && item.AppliedAmount < 0)
                    {
                        if (lineItems_Clients?.Is_Passthrough == true && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                        {
                            ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id
                            == MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability);
                            ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);
                        }
                        DebitCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                        CreditCOAValue = ChartOfAccounts?.ReferenceID;
                        DebitDescription = $"[RECOVERY] Applying incoming payment of Id - {payments.Payment_Id}, (  {PaymentType}   {payments.ReferenceNumber}   {payments.EntityName}  ) for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                        CreditDescription = $"[RECOVERY] Applied to expense account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    }
                    else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && LineItemsDirections.Direction_Id == MasterLookup._Directions.AR && item.AppliedAmount < 0)
                    {
                        DebitCOAValue = ChartOfAccounts?.ReferenceID;
                        CreditCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                        DebitDescription = $"[Netting] Applied to Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                        CreditDescription = $"[Netting] Applying incoming payment of Id - {payments.Payment_Id} , ( {PaymentType} {payments.ReferenceNumber} {payments.EntityName} ) for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    }
                    else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && LineItemsDirections.Direction_Id == MasterLookup._Directions.AR && item.AppliedAmount > 0)
                    {
                        DebitCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                        CreditCOAValue = ChartOfAccounts?.ReferenceID;
                        CreditDescription = $"[Netting] Applied to Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                        DebitDescription = $"[Netting] Applying incoming payment of Id - {payments.Payment_Id}, ( {PaymentType} {payments.ReferenceNumber} {payments.EntityName} ) for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";

                    }
                    else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && LineItemsDirections.Direction_Id == MasterLookup._Directions.AP && item.AppliedAmount < 0)
                    {
                        if (lineItems_Clients?.Is_Passthrough == true && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                        {
                            ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id
                            == MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability);
                            ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);
                        }
                        DebitCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                        CreditCOAValue = ChartOfAccounts?.ReferenceID;
                        DebitDescription = $"[Netting] Applying outgoing payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                        CreditDescription = $"[Netting] Applied to Expense Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";

                    }

                    // Debit
                    lines.Add(new SageJournalLine
                    {
                        AccountNo = DebitCOAValue,
                        Amount = Math.Abs(item.AppliedAmount),
                        IsDebit = true,
                        Description = DebitDescription,
                    });

                    // Credit
                    lines.Add(new SageJournalLine
                    {
                        AccountNo = CreditCOAValue,
                        Amount = Math.Abs(item.AppliedAmount),
                        IsDebit = false,
                        Description = CreditDescription,
                    });
                }

            }
            catch (Exception ex)
            {
                _ServiceLogInfo.Status = false;
                _ServiceLogInfo.Message = ex.Message;
            }
            return lines;
        }

        public List<SageJournalLine> PrepareUnApplyJournalLines_Intacct(SqlTransaction PaymentsTransaction, PaymentApplicationLineItems paymentApplicationLineItems,
    JournalEntries journalEntries)
        {
            var lines = new List<SageJournalLine>();

            var ChartOfAccountsLineItemsList = _LookupRepository.GetAllLineItems_ChartOfAccounts(_Service_Id, _Client_Id);

            var paymentApplications = _PaymentsRepository
                .GetPaymentApplicationsByPaymentApplication_Id(paymentApplicationLineItems.PaymentApplication_Id);

            var payments = _PaymentsRepository
                .GetPaymentDetails(paymentApplications.Payment_Id);

            byte _ChartOfAccountsClassificationType_Id = 0;

            _AccountingType_Id = paymentApplicationLineItems.AccountingType_Id;

            if (_AccountingType_Id == MasterLookup._AccountingTypes.Cash)
                _ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability;
            else
                _ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.AccountsReceivableAccountsPayable;

            string DebitCOAValue = null;
            string CreditCOAValue = null;
            string DebitDescription = null;
            string CreditDescription = null;
            string PaymentType = null;

            var invoiceDetails = _InvoiceRepository.GetInvoiceDetailsByInvoiceDetail_Id(paymentApplicationLineItems.InvoiceLineItem_Id);
            var invoices = _InvoiceRepository.GetInvoicesByInvoice_Id(invoiceDetails.Invoice_Id);
            var policyDetails = _PolicyDetailsRepository.GetPolicyDetails(invoices.PolicyDetails_Id);

            var BillingLineItemsDirections = LookupUtility.GetAllLU_BillingType_LineItems_Directions()
                .FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id);

            var LineItemsDirections = LookupUtility.GetAllLineItems_Directions()
                .FirstOrDefault(x => x.LineItems_Directions_Id == BillingLineItemsDirections?.LineItems_Directions_Id);

            var lineItems_Clients = LookupUtility.GetAllLineItems_Clients()
                .FirstOrDefault(x => x.LineItems_Clients_Id == LineItemsDirections?.LineItems_Clients_Id);

            var LineItems = LookupUtility.GetAllLineItems()
                .FirstOrDefault(c => c.LineItem_Id == lineItems_Clients?.LineItem_Id);

            var LOB = LookupUtility.GetAllLOBs()
                .FirstOrDefault(x => x.LOB_Id == invoiceDetails.LOB_Id);

            var lobcarriergroupingDetails = _LobCarrierGroupingRepository
                .GetLOBCarrierGroupingDetails(LOB.LOB_Id, policyDetails.CarrierDetail_Id, _Service_Id, _Client_Id);

            var ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x =>
                x.Client_Id == _Client_Id &&
                x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id &&
                x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id &&
                x.ChartOfAccountsClassificationType_Id == _ChartOfAccountsClassificationType_Id);

            var ChartOfAccounts = LookupUtility.GetAllChartofAccounts()
                .FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);

            var PaymentDirectionsCOAList = _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id);

            var PaymentDirectionsCOA = PaymentDirectionsCOAList
                .FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);

            var PaymentDirectionsCOAItem = LookupUtility.GetAllChartofAccounts()
                .FirstOrDefault(c => c.ChartOfAccounts_Id == PaymentDirectionsCOA?.StandardCOA_Id);

            PaymentType = _LookupRepository.GetAllPaymentTypes().FirstOrDefault(x => x.PaymentType_Id == payments.PaymentType_Id)?.Name;

            if (PaymentDirectionsCOA == null || PaymentDirectionsCOAItem == null)
                throw new Exception("Missing COA Configuration (Payment Direction)");

            if (ChartOfAccounts == null)
                throw new Exception("Missing COA Configuration (Line Item)");


            if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming &&
                LineItemsDirections.Direction_Id == MasterLookup._Directions.AR &&
                paymentApplicationLineItems.AppliedAmount > 0)
            {
                CreditCOAValue = PaymentDirectionsCOAItem.ReferenceID;
                DebitCOAValue = ChartOfAccounts.ReferenceID;

                DebitDescription = $"Un Applied from Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                CreditDescription = $"Un Applying incoming payment of Id - {payments.Payment_Id}, ( {PaymentType} {payments.ReferenceNumber} {payments.EntityName} ) for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
            }

            else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && LineItemsDirections?.Direction_Id == MasterLookup._Directions.AP && paymentApplicationLineItems.AppliedAmount > 0)
            {
                if (lineItems_Clients?.Is_Passthrough == true && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                {
                    ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id
== MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability);
                    ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);
                }
                DebitCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                CreditCOAValue = ChartOfAccounts?.ReferenceID;
                CreditDescription = $"Un Applied to Expense Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                DebitDescription = $"Un Applying outgoing payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
            }
            else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && LineItemsDirections.Direction_Id == MasterLookup._Directions.AR && paymentApplicationLineItems.AppliedAmount < 0)
            {
                DebitCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                CreditCOAValue = ChartOfAccounts?.ReferenceID;
                DebitDescription = $"[REFUND] Un Applying outgoing payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                CreditDescription = $"[REFUND] Un Applying to Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
            }
            else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && LineItemsDirections.Direction_Id == MasterLookup._Directions.AP && paymentApplicationLineItems.AppliedAmount < 0)
            {
                if (lineItems_Clients?.Is_Passthrough == true && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                {
                    ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id
                    == MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability);
                    ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);
                }
                CreditCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                DebitCOAValue = ChartOfAccounts?.ReferenceID;
                DebitDescription = $"[RECOVERY] Un Applied to expense account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                CreditDescription = $"[RECOVERY] Un Applying incoming payment of Id - {payments.Payment_Id}, ( {PaymentType} {payments.ReferenceNumber} {payments.EntityName} ) for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
            }
            else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && LineItemsDirections.Direction_Id == MasterLookup._Directions.AR && paymentApplicationLineItems.AppliedAmount < 0)
            {
                DebitCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                CreditCOAValue = ChartOfAccounts?.ReferenceID;
                DebitDescription = $"[Netting] Un Applying incoming payment of Id - {payments.Payment_Id}, ( {PaymentType} {payments.ReferenceNumber} {payments.EntityName} ) for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                CreditDescription = $"[Netting] Un Applied to Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
            }
            else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && LineItemsDirections.Direction_Id == MasterLookup._Directions.AP && paymentApplicationLineItems.AppliedAmount < 0)
            {
                if (lineItems_Clients?.Is_Passthrough == true && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                {
                    ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id
                    == MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability);
                    ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);
                }
                DebitCOAValue = ChartOfAccounts?.ReferenceID;
                CreditCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                DebitDescription = $"[Netting] Un Applied to Expense Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                CreditDescription = $"[Netting] Un Applying outgoing payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
            }
            else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && LineItemsDirections.Direction_Id == MasterLookup._Directions.AP && paymentApplicationLineItems.AppliedAmount > 0)
            {
                if (lineItems_Clients?.Is_Passthrough == true && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                {
                    ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id
                  == MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability);
                    ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);
                }
                DebitCOAValue = ChartOfAccounts?.ReferenceID;
                CreditCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                DebitDescription = $"[Netting] Applied to Expense Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                CreditDescription = $"[Netting] Applying outgoing payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
            }

            else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && LineItemsDirections.Direction_Id == MasterLookup._Directions.AR && paymentApplicationLineItems.AppliedAmount > 0)
            {
                CreditCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                DebitCOAValue = ChartOfAccounts?.ReferenceID;
                DebitDescription = $"[Netting] Un Applied to Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                CreditDescription = $"[Netting] Un Applying incoming payment of Id - {payments.Payment_Id}, ( {PaymentType} {payments.ReferenceNumber} {payments.EntityName} ) for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
            }


            // Debit
            lines.Add(new SageJournalLine
            {
                AccountNo = DebitCOAValue,
                Amount = Math.Abs(paymentApplicationLineItems.AppliedAmount),
                IsDebit = true,
                Description = DebitDescription,
            });

            // Credit
            lines.Add(new SageJournalLine
            {
                AccountNo = CreditCOAValue,
                Amount = Math.Abs(paymentApplicationLineItems.AppliedAmount),
                IsDebit = false,
                Description = CreditDescription,
            });

            return lines;
        }

        public async Task<string> GetJournalEntryById_Intacct(string recordNo)
        {
            SageResponse createSageJournalResponse = new SageResponse();
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.ReadJournalEntryById
            };

            string functionXml = $@"
            <function controlid=""{Guid.NewGuid()}"">
              <readByQuery>
                <object>GLBATCH</object>
                <fields>*</fields>
                <query>RECORDNO = '{recordNo}'</query>
                <pagesize>1</pagesize>
              </readByQuery>
            </function>";

            string response = string.Empty;

            try
            {
                response = await CallIntacct(_ServiceLogInfo, functionXml);

                _ServiceLogInfo.Response = response;
                _ServiceLogInfo.Is_Success = true;
                _ServiceLogInfo.Message = "Success";
            }
            catch (Exception ex)
            {
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }
            var serializer = new XmlSerializer(typeof(SageResponse));
            using (var reader = new StringReader(response))
            {
                createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
            }

            _ServiceLogInfo.Response = response;
            _ServiceLogInfo.Is_Success = true;
            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);

            return response;
        }
        public List<SageJournalLine> PreparePaymentWriteoffLines_Intacct(SqlTransaction PaymentsTransaction, Payments payments, PaymentApplications paymentApplications, PaymentApplicationLineItems PaymentApplicationLineItems,
        JournalEntries journalEntries, bool Is_Update, bool Is_Delete)
        {
            var lines = new List<SageJournalLine>();
            List<ChartOfAccountDetails> coaList = new List<ChartOfAccountDetails>();
            ChartOfAccountDetails coa = new ChartOfAccountDetails();


            //var standardCOAItem = LookupUtility.GetAllChartofAccounts()
            //    .FirstOrDefault(c => c.ChartOfAccounts_Id ==
            //        _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id)
            //        .FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id)?.StandardCOA_Id);

            //var writeoffCOAItem = LookupUtility.GetAllChartofAccounts()
            //    .FirstOrDefault(c => c.ChartOfAccounts_Id ==
            //        _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id)
            //        .FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id)?.WriteoffCOA_Id);

            coaList = _LookupRepository.GetAllPaymentDirectionsSageChartOfAccounts(_Service_Id, _Client_Id, payments.PaymentDirection_Id);
            coa = coaList?.FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);

            var standardCOAItem = _LookupRepository.GetChartOfAccountsByChartOfAccount_Id(coa.StandardCOA_Id, _Client_Id).FirstOrDefault();
            var writeoffCOAItem = _LookupRepository.GetChartOfAccountsByChartOfAccount_Id(coa.WriteoffCOA_Id, _Client_Id).FirstOrDefault();


            if (coa == null)
            {
                throw new InvalidOperationException(
                    $"No COA mapping found for PaymentDirection_Id: {payments.PaymentDirection_Id}");
            }

            if (standardCOAItem == null || writeoffCOAItem == null)
                throw new Exception("Missing COA Configuration");

            // Extract values
            var standardCOAId = coa.StandardCOA_Id;
            var writeoffCOAId = coa.WriteoffCOA_Id;

            string prefix = Is_Delete ? "[Deleted]" :
                            Is_Update ? "[Updated]" :
                            "[Added]";

            if (PaymentApplicationLineItems == null)
                return lines;

            string debitCOA = null;
            string creditCOA = null;
            if (Is_Delete)
            {
                if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming)
                {
                    debitCOA = writeoffCOAItem?.ReferenceID;
                    creditCOA = standardCOAItem?.ReferenceID;
                }
                else
                {
                    debitCOA = standardCOAItem?.ReferenceID;
                    creditCOA = writeoffCOAItem?.ReferenceID;
                }
            }
            else
            {
                if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming)
                {
                    debitCOA = standardCOAItem?.ReferenceID;
                    creditCOA = writeoffCOAItem?.ReferenceID;
                }
                else
                {
                    debitCOA = writeoffCOAItem?.ReferenceID;
                    creditCOA = standardCOAItem?.ReferenceID;
                }
            }

            string desc = $"{prefix} - {paymentApplications.Notes}";

            lines.Add(new SageJournalLine
            {
                AccountNo = debitCOA,
                Amount = Math.Abs(PaymentApplicationLineItems.AppliedAmount),
                IsDebit = true,
                Description = desc,
            });

            lines.Add(new SageJournalLine
            {
                AccountNo = creditCOA,
                Amount = Math.Abs(PaymentApplicationLineItems.AppliedAmount),
                IsDebit = false,
                Description = desc,
            });

            return lines;
        }
        public async Task<string> UpdatePaymentJournalEntry_Intacct(SqlTransaction PaymentsTransaction, Payments payments, JournalEntries journalEntries)
        {
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.UpdateJournalEntry
            };

            string response = string.Empty;

            try
            {

                if (!string.IsNullOrEmpty(journalEntries?.ExternalJournalEntryIdentifier))
                {
                    var reverseXml = BuildReverseJournalXml(journalEntries.ExternalJournalEntryIdentifier);

                    await CallIntacct(_ServiceLogInfo, reverseXml);
                }

                var lines = PrepareCreatePaymentJournalLines_Intacct(
                    PaymentsTransaction,
                    payments);

                if (!lines.Any())
                    throw new Exception("No journal lines found.");

                string docNumber = $"PMT{payments.Payment_Id}-UPDATED";

                var createXml = BuildIntacctJournalXml(
                    docNumber,
                    payments.PaymentDate ?? DateTime.UtcNow,
                    lines,
                    payments.ReferenceNumber,
                    MasterLookup._Actions.UpdatePayment);

                response = await CallIntacct(_ServiceLogInfo, createXml);

                if (journalEntries != null)
                {
                    _JournalEntryLogRepository.InactiveJournalEntryLogsByJournalEntry_Id(
                        PaymentsTransaction,
                        journalEntries.JournalEntry_Id);

                    // You can parse response if needed and log new entries
                }

                _ServiceLogInfo.Is_Success = true;
                _ServiceLogInfo.Response = response;
            }
            catch (Exception ex)
            {
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
                throw;
            }

            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);

            return response;
        }

        public string BuildReverseJournalXml(string recordNo)
        {
            return $@"
            <function controlid=""{Guid.NewGuid()}"">
              <reverse_glbatch>
                <BATCHKEY>{recordNo}</BATCHKEY>
              </reverse_glbatch>
            </function>";
        }
        public async Task<SageBillResponse> ReadBillById_Intacct(string recordNo)
        {
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.ReadBillById
            };

            string functionXml = $@"
    <function controlid=""{Guid.NewGuid()}"">
      <readByQuery>
        <object>APBILL</object>
        <fields>*</fields>
        <query>RECORDNO = '{recordNo}'</query>
        <pagesize>1</pagesize>
      </readByQuery>
    </function>";

            string response = string.Empty;

            try
            {
                response = await CallIntacct(_ServiceLogInfo, functionXml);

                _ServiceLogInfo.Response = response;
                _ServiceLogInfo.Is_Success = true;
                _ServiceLogInfo.Message = "Success";
            }
            catch (Exception ex)
            {
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }

            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);

            return ParseBill(response);
        }
        public async Task<string> DeleteBill_Intacct(string recordNo)
        {
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.DeleteBill
            };

            string functionXml = $@"
            <function controlid=""{Guid.NewGuid()}"">
              <delete>
                <object>APBILL</object>
                <keys>{recordNo}</keys>
              </delete>
            </function>";

            string response = string.Empty;

            try
            {
                response = await CallIntacct(_ServiceLogInfo, functionXml);

                _ServiceLogInfo.Response = response;
                _ServiceLogInfo.Is_Success = true;
                _ServiceLogInfo.Message = "Success";
            }
            catch (Exception ex)
            {
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }

            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);

            return response;
        }
        public async Task<string> DeleteJournalEntry_Intacct(JournalEntries journalEntries)
        {
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.DeleteJournalEntry,
            };

            string response = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(journalEntries?.ExternalJournalEntryIdentifier))
                {
                    _ServiceLogInfo.Is_Success = false;
                    _ServiceLogInfo.Message = "Invalid Journal Identifier";
                    return null;
                }

                string readXml = $@"
                <function controlid=""{Guid.NewGuid()}"">
                    <read>
                        <object>GLBATCH</object>
                        <keys>{journalEntries.ExternalJournalEntryIdentifier}</keys>
                        <fields>*</fields>
                    </read>
                </function>";

                var readRes = await CallIntacct(_ServiceLogInfo, readXml);

                var serializer = new XmlSerializer(typeof(SageResponse));
                SageResponse originalBatch;

                using (var reader = new StringReader(readRes))
                {
                    originalBatch = (SageResponse)serializer.Deserialize(reader);
                }

                var originalEntries = originalBatch?.Operation?.Result?.Data?.GlBatchRead?.Entries;

                if (originalEntries == null || !originalEntries.Any())
                    throw new Exception("No original entries found to reverse.");

                var reversedLines = originalEntries.Select(x => new SageJournalLine
                {
                    AccountNo = x.AccountNo,
                    Amount = Convert.ToDouble(x.Amount),
                    IsDebit = x.TrType == -1,
                    Description = $"Reversal: {x.Description}",
                    Department = "Admin"
                }).ToList();

                string entriesXml = BuildIntacctEntriesXml(reversedLines,null,MasterLookup._Actions.DeletePayment);

                string createXml = $@"
                <function controlid=""{Guid.NewGuid()}"">
                    <create>
                        <GLBATCH>
                            <JOURNAL>GJ</JOURNAL>
                            <BATCH_DATE>{DateTime.Now:MM/dd/yyyy}</BATCH_DATE>
                            <BATCH_TITLE>Reversal - {journalEntries.JournalEntryName}</BATCH_TITLE>
                            <REFERENCENO>{journalEntries.ReferenceNumber}</REFERENCENO>
                            <ENTRIES>
                                {entriesXml}
                            </ENTRIES>
                        </GLBATCH>
                    </create>
                </function>";

                response = await CallIntacct(_ServiceLogInfo, createXml);

                _ServiceLogInfo.Response = response;
                _ServiceLogInfo.Is_Success = true;
                _ServiceLogInfo.Message = "Journal reversed successfully";
            }
            catch (Exception ex)
            {
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }

            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);

            return response;
        }
        //optional if want Hard delete..
        public async Task<string> HardDeleteJournalEntry_Intacct(string recordNo)
        {
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.DeleteJournalEntry,
            };
            string functionXml = $@"
            <function controlid=""{Guid.NewGuid()}"">
              <delete>
                <object>GLBATCH</object>
                <keys>{recordNo}</keys>
              </delete>
            </function>";

            return await CallIntacct(_ServiceLogInfo, functionXml);
        }
        public async Task<string> CreateInvoiceJournalEntry_Intacct(SqlTransaction Transaction, PolicyDetails policyDetails)
        {
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.CreateJournalEntry
            };

            string response = string.Empty;

            List<SageJournalLine> lines = new List<SageJournalLine>();

            try
            {
                lines = PrepareCreateInvoiceJournalLines_Intacct(Transaction, policyDetails);

                if (lines == null || !lines.Any())
                    throw new ApplicationException("No valid journal lines found");

            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error while preparing invoice journal entry:\n" + ex.Message, ex);
            }
            try
            {
                string docNumber = policyDetails?.Invoices?.InvoiceNo;

                string entriesXml = BuildIntacctEntriesXml(lines, policyDetails?.LOB_Id, MasterLookup._Actions.CreateInvoice,policyDetails?.Client_Id);

                string functionXml = $@"
                <function controlid=""{Guid.NewGuid()}"">
                    <create>
                        <GLBATCH>
                            <JOURNAL>GJ</JOURNAL>
                            <BATCH_DATE>{DateTime.Now:MM/dd/yyyy}</BATCH_DATE>
                            <BATCH_TITLE>{docNumber}</BATCH_TITLE>
                            <REFERENCENO>{docNumber}</REFERENCENO>
                            <ENTRIES>
                                {entriesXml}
                            </ENTRIES>
                        </GLBATCH>
                    </create>
                </function>";

                response = await CallIntacct(_ServiceLogInfo, functionXml);

                var xmlDoc = XDocument.Parse(response);
                var recordNo = xmlDoc.Descendants("RECORDNO").FirstOrDefault()?.Value;
                var description2 = xmlDoc.Descendants("description2").FirstOrDefault()?.Value;

                if (!string.IsNullOrEmpty(recordNo))
                {
                    JournalEntries journalEntries = InsertJournalEntries(Transaction, MasterLookup._Sources.Invoice, policyDetails.Invoices.Id,
                        recordNo, null, docNumber);

                    if (journalEntries != null)
                    {
                        InsertJournalEntriesLog_Intacct(Transaction, lines, journalEntries);
                    }
                    _ServiceLogInfo.Response = response;
                    _ServiceLogInfo.Is_Success = true;
                    _ServiceLogInfo.Message = "Success";
                }
                else
                {
                    _ServiceLogInfo.Status = false;
                    _ServiceLogInfo.Message = description2;
                }

            }
            catch (Exception ex)
            {
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }

            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);

            return response;
        }
        public async Task<string> UpdateInvoiceJournalEntry_Intacct(SqlTransaction Transaction, PolicyDetails policyDetails, JournalEntries journalEntries)
        {
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.UpdateJournalEntry
            };

            string response = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(journalEntries?.ExternalJournalEntryIdentifier))
                {
                    _ServiceLogInfo.Is_Success = false;
                    _ServiceLogInfo.Message = "Invalid Journal Identifier";
                    return null;
                }

                string readXml = $@"
                <function controlid=""{Guid.NewGuid()}"">
                    <read>
                        <object>GLBATCH</object>
                        <keys>{journalEntries.ExternalJournalEntryIdentifier}</keys>
                        <fields>*</fields>
                    </read>
                </function>";

                var readRes = await CallIntacct(_ServiceLogInfo, readXml);

                var serializer = new XmlSerializer(typeof(SageResponse));
                SageResponse originalBatch;

                using (var reader = new StringReader(readRes))
                {
                    originalBatch = (SageResponse)serializer.Deserialize(reader);
                }

                var originalEntries = originalBatch?.Operation?.Result?.Data?.GlBatchRead?.Entries;
                if (originalEntries == null || !originalEntries.Any())
                    throw new Exception("No original entries found.");


                var reversedLines = originalEntries.Select(x => new SageJournalLine
                {
                    AccountNo = x.AccountNo,
                    Amount = Convert.ToDouble(x.Amount),
                    IsDebit = x.TrType == -1,
                    Description = $"Reversal: {x.Description}"
                }).ToList();

                string reverseEntriesXml = BuildIntacctEntriesXml(reversedLines);

                string reverseXml = $@"
                <function controlid=""{Guid.NewGuid()}"">
                    <create>
                        <GLBATCH>
                            <JOURNAL>GJ</JOURNAL>
                            <BATCH_DATE>{DateTime.Now:MM/dd/yyyy}</BATCH_DATE>
                            <BATCH_TITLE>Reversal - {journalEntries.JournalEntryName}</BATCH_TITLE>
                            <REFERENCENO>{journalEntries.JournalEntryName}</REFERENCENO>
                            <ENTRIES>
                                {reverseEntriesXml}
                            </ENTRIES>
                        </GLBATCH>
                    </create>
                </function>";

                var newLines = PrepareUpdateInvoiceJournalLines_Intacct(policyDetails);

                if (!newLines.Any())
                    throw new Exception("No valid journal lines for updated entry.");

                string newEntriesXml = BuildIntacctEntriesXml(newLines);

                string newXml = $@"
                <function controlid=""{Guid.NewGuid()}"">
                    <create>
                        <GLBATCH>
                            <JOURNAL>GJ</JOURNAL>
                            <BATCH_DATE>{DateTime.Now:MM/dd/yyyy}</BATCH_DATE>
                            <BATCH_TITLE>{journalEntries.JournalEntryName}</BATCH_TITLE>
                            <REFERENCENO>{policyDetails?.Invoices?.InvoiceNo}</REFERENCENO>
                            <ENTRIES>
                                {newEntriesXml}
                            </ENTRIES>
                        </GLBATCH>
                    </create>
                </function>";


                string combinedXml = reverseXml + newXml;

                response = await CallIntacct(_ServiceLogInfo, combinedXml);


                if (!string.IsNullOrEmpty(response))
                {
                    if (journalEntries != null)
                    {
                        _JournalEntryLogRepository.InactiveJournalEntryLogsByJournalEntry_Id(Transaction, journalEntries.JournalEntry_Id);

                        InsertJournalEntriesLog_Intacct(Transaction, newLines, journalEntries);
                    }
                }

                _ServiceLogInfo.Response = response;
                _ServiceLogInfo.Is_Success = true;
                _ServiceLogInfo.Message = "Journal updated successfully";
            }
            catch (Exception ex)
            {
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }

            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);

            return response;
        }
        public List<SageJournalLine> PrepareUpdateInvoiceJournalLines_Intacct(PolicyDetails policyDetails)
        {
            var lines = new List<SageJournalLine>();

            foreach (var item in policyDetails.Invoices.InvoicesDetailsList)
            {
                string DebitCOAValue = null;
                string CreditCOAValue = null;
                string DebitDescription = null;
                string CreditDescription = null;

                var BillingLineItemsDirections = LookupUtility
                    .GetAllLU_BillingType_LineItems_Directions()
                    .FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == item.BillingType_LineItems_Directions_Id);

                var LineItemsDirections = LookupUtility
                    .GetAllLineItems_Directions()
                    .FirstOrDefault(x => x.LineItems_Directions_Id == BillingLineItemsDirections?.LineItems_Directions_Id);

                var lineItems_Clients = LookupUtility
                    .GetAllLineItems_Clients()
                    .FirstOrDefault(x => x.LineItems_Clients_Id == LineItemsDirections?.LineItems_Clients_Id);

                var LineItems = LookupUtility
                    .GetAllLineItems()
                    .FirstOrDefault(c => c.LineItem_Id == lineItems_Clients?.LineItem_Id);

                var Directions = LookupUtility
                    .GetAllDirections()
                    .FirstOrDefault(c => c.Direction_Id == LineItemsDirections.Direction_Id);

                var LOB = LookupUtility
                    .GetAllLOBs()
                    .FirstOrDefault(c => c.LOB_Id == item.LOB_Id);

                var lobcarriergroupingDetails = _LobCarrierGroupingRepository
                    .GetLOBCarrierGroupingDetails(LOB.LOB_Id, policyDetails.CarrierDetail_Id, _Service_Id, _Client_Id);

                var LineItemsChartOfAccounts = _LookupRepository
                    .GetAllLineItems_ChartOfAccounts(_Service_Id, _Client_Id);

                var errorDetail = BuildErrorDetail(Directions?.Code, LineItems?.Name, LOB?.Name, policyDetails.BillType);

                if (LineItemsDirections.Direction_Id == MasterLookup._Directions.AR)
                {
                    var DebitCOA = LineItemsChartOfAccounts.FirstOrDefault(x =>
                        x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id &&
                        x.BillingType_LineItems_Directions_Id == item.BillingType_LineItems_Directions_Id &&
                        x.ChartOfAccountsClassificationType_Id == MasterLookup._ChartOfAccountsClassificationTypes.AccountsReceivableAccountsPayable);

                    var CreditCOA = LineItemsChartOfAccounts.FirstOrDefault(x =>
                        x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id &&
                        x.BillingType_LineItems_Directions_Id == item.BillingType_LineItems_Directions_Id &&
                        x.ChartOfAccountsClassificationType_Id == MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability);

                    var DebitItem = LookupUtility.GetAllChartofAccounts()
                        .FirstOrDefault(c => c.ChartOfAccounts_Id == DebitCOA.ChartOfAccount_Id);

                    var CreditItem = LookupUtility.GetAllChartofAccounts()
                        .FirstOrDefault(c => c.ChartOfAccounts_Id == CreditCOA.ChartOfAccount_Id);
                    if (DebitCOA == null || CreditCOA == null || DebitItem == null || CreditItem == null)
                    {
                        fault = AddError(fault, "Missing COA Configuration", errorDetail);
                        continue;
                    }
                    if (item.Amount > 0)
                    {
                        DebitCOAValue = DebitItem.ReferenceID;
                        CreditCOAValue = CreditItem.ReferenceID;

                        DebitDescription = $"AR Debit - {policyDetails.Invoices.InvoiceNo}";
                        CreditDescription = $"Income Credit - {policyDetails.Invoices.InvoiceNo}";
                    }
                    else
                    {
                        DebitCOAValue = CreditItem.ReferenceID;
                        CreditCOAValue = DebitItem.ReferenceID;

                        DebitDescription = $"Income Debit - {policyDetails.Invoices.InvoiceNo}";
                        CreditDescription = $"AR Credit - {policyDetails.Invoices.InvoiceNo}";
                    }
                }
                else if (LineItemsDirections.Direction_Id == MasterLookup._Directions.AP &&
                         lineItems_Clients?.Is_Passthrough == false)
                {
                    var DebitCOA = LineItemsChartOfAccounts.FirstOrDefault(x =>
                        x.ChartOfAccountsClassificationType_Id == MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability);

                    var CreditCOA = LineItemsChartOfAccounts.FirstOrDefault(x =>
                        x.ChartOfAccountsClassificationType_Id == MasterLookup._ChartOfAccountsClassificationTypes.AccountsReceivableAccountsPayable);

                    var DebitItem = LookupUtility.GetAllChartofAccounts()
                        .FirstOrDefault(c => c.ChartOfAccounts_Id == DebitCOA.ChartOfAccount_Id);

                    var CreditItem = LookupUtility.GetAllChartofAccounts()
                        .FirstOrDefault(c => c.ChartOfAccounts_Id == CreditCOA.ChartOfAccount_Id);

                    if (DebitCOA == null || CreditCOA == null || DebitItem == null || CreditItem == null)
                    {
                        fault = AddError(fault, "Missing COA Configuration", errorDetail);
                        continue;
                    }
                    if (item.Amount > 0)
                    {
                        DebitCOAValue = DebitItem.ReferenceID;
                        CreditCOAValue = CreditItem.ReferenceID;
                    }
                    else
                    {
                        DebitCOAValue = CreditItem.ReferenceID;
                        CreditCOAValue = DebitItem.ReferenceID;
                    }
                }

                if (!string.IsNullOrEmpty(DebitCOAValue) && !string.IsNullOrEmpty(CreditCOAValue))
                {
                    // Debit
                    lines.Add(new SageJournalLine
                    {
                        AccountNo = DebitCOAValue,
                        Amount = Math.Abs(item.Amount),
                        IsDebit = true,
                        Description = DebitDescription
                    });

                    // Credit
                    lines.Add(new SageJournalLine
                    {
                        AccountNo = CreditCOAValue,
                        Amount = Math.Abs(item.Amount),
                        IsDebit = false,
                        Description = CreditDescription
                    });
                }
            }

            if (fault != null && fault.Error?.Any() == true)
                throw new Exception("COA Mapping Errors");

            return lines;
        }
        public void InsertJournalEntriesLog_Intacct(SqlTransaction transaction, List<SageJournalLine> journalLines, JournalEntries journalEntries)
        {
            if (journalLines?.Count > 0)
            {
                foreach (var item in journalLines)
                {
                    int? coaId = LookupUtility.GetAllChartofAccounts()
                        .FirstOrDefault(c => c.ReferenceID == item.AccountNo && c.Client_Id == _Client_Id)
                        ?.ChartOfAccounts_Id;

                    JournalEntriesLog log = new JournalEntriesLog
                    {
                        JournalEntry_Id = journalEntries.JournalEntry_Id,
                        DebitChartOfAccount_Id = item.IsDebit ? coaId : null,
                        CreditChartOfAccount_Id = !item.IsDebit ? coaId : null,
                        Description = item.Description,
                        Amount = (double)item.Amount,
                        JournalEntryStatus_Id = MasterLookup._JournalEntryStatus.Created,
                        CreatedBy = _User_Id
                    };

                    _JournalEntryLogRepository.InsertJournalEntriesLog(transaction, log);
                }
            }
        }
        public SageJournalResponse ParseJournal(string xml)
        {
            var doc = XDocument.Parse(xml);

            var batch = doc.Descendants("glbatch").FirstOrDefault();

            if (batch == null) return null;

            return new SageJournalResponse
            {
                RecordNo = batch.Element("RECORDNO")?.Value,
                Journal = batch.Element("JOURNAL")?.Value,
                BatchDate = batch.Element("BATCH_DATE")?.Value,
                ReferenceNo = batch.Element("REFERENCENO")?.Value,
                Entries = doc.Descendants("GLENTRY")
                    .Select(x => new SageJournalLine
                    {
                        AccountNo = x.Element("ACCOUNTNO")?.Value,
                        Amount = Convert.ToDouble(x.Element("AMOUNT")?.Value),
                        Description = x.Element("DESCRIPTION")?.Value,
                        IsDebit = x.Element("TR_TYPE")?.Value == "1"
                    }).ToList()
            };
        }
        public SageBillResponse ParseBill(string xml)
        {
            var doc = XDocument.Parse(xml);

            var bill = doc.Descendants("APBILL").FirstOrDefault();

            if (bill == null) return null;

            return new SageBillResponse
            {
                RecordNo = bill.Element("RECORDNO")?.Value,
                VendorId = bill.Element("VENDORID")?.Value,
                BillNo = bill.Element("BILLNO")?.Value,
                TotalDue = bill.Element("TOTALDUE")?.Value
            };
        }
        public List<SageAccountsResponse> ParseAccounts(string xml)
        {
            var doc = XDocument.Parse(xml);

            return doc.Descendants("glaccount")
                .Select(x => new SageAccountsResponse
                {
                    AccountNo = x.Element("ACCOUNTNO")?.Value,
                    Title = x.Element("TITLE")?.Value,
                    Status = x.Element("STATUS")?.Value
                })
                .ToList();
        }
        private string BuildErrorDetail(
    string direction = null,
    string lineItem = null,
    string lob = null,
    string billingType = null)
        {
            var details = new List<string>();

            if (!string.IsNullOrWhiteSpace(lob))
                details.Add($"LOB: {lob}");

            if (!string.IsNullOrWhiteSpace(direction))
                details.Add($"Direction: {direction}");

            if (!string.IsNullOrWhiteSpace(billingType))
                details.Add($"Bill Type: {billingType}");

            var detailText = details.Any()
                ? $"({string.Join(", ", details)})"
                : string.Empty;

            return !string.IsNullOrWhiteSpace(lineItem)
                ? $"Line Item: {lineItem} {detailText}".Trim()
                : detailText;
        }
        public SageFault AddError(SageFault fault, string message, string detail = null)
        {
            if (fault == null)
            {
                fault = new SageFault
                {
                    type = "ValidationFault"
                };
            }

            if (fault.Error == null)
            {
                fault.Error = new List<Error>();
            }

            //fault.Error.Add(new Error
            //{
            //    Message = message,
            //    Detail = detail,
            //});

            return fault;
        }
        public string ReadVendorByVendorId(string vendorId)
        {
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.CreateVendor
            };

            string functionXml = $@"
                <function controlid=""{Guid.NewGuid()}"">
                    <readByQuery>
                        <object>VENDOR</object>
                        <fields>*</fields>
                        <query>VENDORID = '{vendorId}'</query>
                        <pagesize>1</pagesize>
                    </readByQuery>
                </function>";

            string response = CallIntacct(_ServiceLogInfo, functionXml).Result;
            _ServiceLogInfo.Response = response;
            _ServiceLogInfo.Is_Success = true;
            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);

            return response;
        }
        public SageResponse CreateVendor(string vendorId, string vendorName)
        {
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.CreateVendor
            };

            string functionXml = $@"
        <function controlid=""{Guid.NewGuid()}"">
            <create>
                <VENDOR>
                    <VENDORID>{vendorId}</VENDORID>
                    <NAME><![CDATA[{vendorName}]]></NAME>
                    <STATUS>active</STATUS>
                </VENDOR>
            </create>
        </function>";
            _ServiceLogInfo.Request = functionXml;
            string response = CallIntacct(_ServiceLogInfo, functionXml).Result;

            _ServiceLogInfo.Response = response;

            var serializer = new XmlSerializer(typeof(SageResponse));
            SageResponse sageResponse;
            using (var reader = new StringReader(response))
            {
                sageResponse = (SageResponse)serializer.Deserialize(reader);
            }

            _ServiceLogInfo.Is_Success = sageResponse?.Operation?.Result?.Status == "success";

            if (!_ServiceLogInfo.Is_Success)
            {
                var error = sageResponse?.Operation?.Result?.Errormessage?.Error?.FirstOrDefault();

                _ServiceLogInfo.Message =
                    $"ErrorNo: {error?.Errorno}, " +
                    $"Description: {error?.Description2}";
            }

            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);

            return sageResponse;
        }
        public SageResponse UpdateVendor(string recordNo, string vendorName)
        {
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.UpdateVendor
            };

            string functionXml = $@"
        <function controlid=""{Guid.NewGuid()}"">
            <update>
                <VENDOR>
                    <RECORDNO>{recordNo}</RECORDNO>
                    <NAME><![CDATA[{vendorName}]]></NAME>
                </VENDOR>
            </update>
        </function>";
            _ServiceLogInfo.Request = functionXml;
            string response = CallIntacct(_ServiceLogInfo, functionXml).Result;

            _ServiceLogInfo.Response = response;

            var serializer = new XmlSerializer(typeof(SageResponse));
            SageResponse sageResponse;
            using (var reader = new StringReader(response))
            {
                sageResponse =
                    (SageResponse)serializer.Deserialize(reader);
            }

            _ServiceLogInfo.Is_Success = sageResponse?.Operation?.Result?.Status == "success";

            if (!_ServiceLogInfo.Is_Success)
            {
                var error = sageResponse?.Operation?.Result?.Errormessage?.Error?.FirstOrDefault();

                _ServiceLogInfo.Message =
                    $"ErrorNo: {error?.Errorno}, " +
                    $"Description: {error?.Description2}";
            }

            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);

            return sageResponse;
        }
        public SageResponse CreateBill(List<PolicyDetails> policydetailsList, Payments payments, string vendorId)
        {
            try
            {
                _ServiceLogInfo = new ServiceLogInfo()
                {
                    Client_Id = _Client_Id,
                    Service_Id = _Service_Id,
                    ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.CreateBill
                };
                var ChartOfAccountsLineItemsList = _LookupRepository.GetAllLineItems_ChartOfAccounts(_Service_Id, _Client_Id);
                byte _ChartOfAccountsClassificationType_Id = 0;
                string accountNo = null;
                StringBuilder apBillItemsXml = new StringBuilder();
                var configurations = ClientLevelSageConfigurations.GetClientLevelSageConfigurations(_Client_Id, MasterLookup._Actions.CreateBill);
                foreach (var policy in policydetailsList)
                {
                    InvoiceDetails invoiceDetails = policy.Invoices.InvoiceDetails;
                    LU_BillingType_LineItems_Directions BillingLineItemsDirections = LookupUtility.GetAllLU_BillingType_LineItems_Directions().FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id);
                    LU_LineItems_Directions LineItemsDirections = LookupUtility.GetAllLineItems_Directions().FirstOrDefault(x => x.LineItems_Directions_Id == BillingLineItemsDirections.LineItems_Directions_Id);
                    LU_LineItems_Clients lineItems_Clients = LookupUtility.GetAllLineItems_Clients().FirstOrDefault(x => x.LineItems_Clients_Id == LineItemsDirections.LineItems_Clients_Id);
                    LU_LOBs LOB = LookupUtility.GetAllLOBs().FirstOrDefault(x => x.LOB_Id == invoiceDetails.LOB_Id);
                    LOBCarrierGrouping lobcarriergroupingDetails = _LobCarrierGroupingRepository.GetLOBCarrierGroupingDetails(LOB.LOB_Id, policy.CarrierDetail_Id, MasterLookup._Services.Sage, policy.Client_Id);
                    if (_AccountingType_Id == MasterLookup._AccountingTypes.Cash)
                    {
                        _ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability;
                    }
                    else if (_AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                    {
                        if (lineItems_Clients?.Is_Passthrough == true && (LineItemsDirections?.Direction_Id == MasterLookup._Directions.AP))
                        {
                            _ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability;
                        }
                        else
                        {
                            _ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.AccountsReceivableAccountsPayable;
                        }
                    }
                    var ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id == _ChartOfAccountsClassificationType_Id);
                    var ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);
                    accountNo = ChartOfAccounts?.ReferenceID;
                    decimal amount = (decimal)invoiceDetails?.PaymentApplicationLineItems?.AppliedAmount;
                    
                    string location = configurations?.Location ?? string.Empty;
                    string classId = configurations?.Class ?? string.Empty;
                    string department = configurations?.Department ?? string.Empty;

           //         string location = "200";
           //         string classId = "PC";

           //         if (_Client_Id == 10022)
           //         {
           //             location = "E11000";
           //             classId = "0060G00000lCzdMQAS";
           //         }
           //         var department = _Client_Id == 10022
           //? "VT"
           //: "Admin";
                    apBillItemsXml.Append($@"
                    <APBILLITEM>
                        <ACCOUNTNO>{accountNo}</ACCOUNTNO>
                        <TRX_AMOUNT>{amount}</TRX_AMOUNT>
                        <ENTRYDESCRIPTION>
                            <![CDATA[{payments.EntityName}]]>
                        </ENTRYDESCRIPTION>
                        <LOCATIONID>{location}</LOCATIONID>
                        <CLASSID>{classId}</CLASSID>
                        <DEPARTMENTID>{department}</DEPARTMENTID>
                    </APBILLITEM>");
                }
                DateTime dueDate = DateTime.Now.AddDays(30);
                string functionXml = $@"
                    <function controlid=""{Guid.NewGuid()}"">
                        <create>
                            <APBILL>
                                <WHENCREATED>{DateTime.Now:MM/dd/yyyy}</WHENCREATED>
                                <WHENPOSTED>{DateTime.Now:MM/dd/yyyy}</WHENPOSTED>
                                <VENDORID>{vendorId}</VENDORID>
                                <RECORDID>EI-PAY-{payments.Payment_Id}</RECORDID>
                                <DOCNUMBER>EI-PAY-{payments.Payment_Id}</DOCNUMBER>
                                <DESCRIPTION><![CDATA[Payment Bill]]></DESCRIPTION>
                                <TERMNAME>Net 30</TERMNAME>
                                <WHENDUE>{dueDate.ToString("MM/dd/yyyy")}</WHENDUE>
                                <CURRENCY>USD</CURRENCY>
                                <BASECURR>USD</BASECURR>
                                <APBILLITEMS>
                                    {apBillItemsXml}
                                </APBILLITEMS>
                            </APBILL>
                        </create>
                    </function>";
                _ServiceLogInfo.Request = functionXml;

                string response = CallIntacct(_ServiceLogInfo, functionXml).Result;

                _ServiceLogInfo.Response = response;
                var serializer = new XmlSerializer(typeof(SageResponse));
                SageResponse sageResponse;
                using (var reader = new StringReader(response))
                {
                    sageResponse = (SageResponse)serializer.Deserialize(reader);
                }

                _ServiceLogInfo.Is_Success =
                    sageResponse?.Operation?.Result?.Status == "success";

                if (!_ServiceLogInfo.Is_Success)
                {
                    var error =
                        sageResponse?.Operation?.Result?.Errormessage?.Error?.FirstOrDefault();
                    _ServiceLogInfo.Message =
                        $"ErrorNo: {error?.Errorno}, " +
                        $"Description: {error?.Description2}";
                }

                _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);

                return sageResponse;
            }
            catch (Exception ex)
            {
                throw new(ex.ToString());
            }
        }
    }
}
