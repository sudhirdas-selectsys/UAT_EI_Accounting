using Accounting.DBUtilities.Implementations.Lookups;
using Accounting.DBUtilities.Implementations.Transactions;
using Accounting.DBUtilities.Interfaces.Lookups;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.External.QBO;
using Accounting.Entities.External.QBO.ChartOfAccounts;
using Accounting.Entities.External.QBO.Journal;
using Accounting.Entities.Internal;
using Accounting.Entities.Internal.Enums;
using Accounting.Utilities.Internal;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Text;
using System.Transactions;
using static iTextSharp.text.pdf.AcroFields;

namespace Accounting.Utilities.External.QBO
{
    public class QBOUtility
    {
        public static IMemoryCache _MemoryCache { get; set; }
        public ServiceLogInfo _ServiceLogInfo { get; set; }
        public byte _Service_Id { get; set; }
        public long _Client_Id { get; set; }
        public long _User_Id { get; set; }
        public byte _AccountingType_Id { get; set; }
        public static TokenResponse _Token = new TokenResponse();
        QBOParameters _QBOParameters = new QBOParameters();
        IServiceLogInfoRepository _ServiceLogInfoRepository = new ServiceLogInfoRepository();
        ServiceResponse _ServiceResponse = new ServiceResponse();
        IInvoiceRepository _InvoiceRepository = new InvoiceRepository();
        IPolicyDetailsRepository _PolicyDetailsRepository = new PolicyDetailsRepository();
        IPaymentsRepository _PaymentsRepository = new PaymentsRepository();
        ILookupRepository _LookupRepository = new LookupRepository();
        IJournalEntryLogRepository _JournalEntryLogRepository = new JournalEntryLogRepository();
        ILOBCarrierGroupingRepository _LobCarrierGroupingRepository = new LOBCarrierGroupingRepository();
        IJournalEntryLogRepository _journalEntryLogRepository = new JournalEntryLogRepository();
        ILookupRepository _lookupRepository = new LookupRepository();
        Clients _Clients { get; set; }
        Fault fault { get; set; }
        public QBOUtility(long Client_Id, long User_Id, byte Service_Id, byte AccountingType_Id)
        {
            _MemoryCache = WebAppReferenceUtility._memoryCache;
            _Client_Id = Client_Id;
            _Service_Id = Service_Id;
            _User_Id = User_Id;
            _AccountingType_Id = AccountingType_Id;
            var ClientLevelParameters = LookupUtility.GetAllClientLevelParameters(_Client_Id).Where(x => x.Service_Id == _Service_Id);
            var ServiceLevelParameters = LookupUtility.GetAllServiceLevelParameters().Where(x => x.Service_Id == _Service_Id);
            _QBOParameters.BaseURL = ServiceLevelParameters.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.BaseURL)?.DefaultValue;
            _QBOParameters.Company_Id = ClientLevelParameters.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.Company_Id)?.Value;
            _QBOParameters.MinorVersion = ServiceLevelParameters.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.MinorVersion)?.DefaultValue;
            _QBOParameters.Client_Id = ClientLevelParameters.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.Client_Id)?.Value;
            _QBOParameters.Client_Secret_Id = ClientLevelParameters.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.Client_Secret_Id)?.Value;
            _QBOParameters.TokenEndpoint = ServiceLevelParameters.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.TokenEndpoint)?.DefaultValue;
            _QBOParameters.RefreshToken = ClientLevelParameters.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.RefreshToken)?.Value;
            _Token = GetAuthToken();
            fault = new Fault();

        }

        public AccountsResponse GetAllAccounts()
        {
            //string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/query?query= Select * from Account &minorversion={_QBOParameters.MinorVersion}";
            string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/query?query= Select * from Account startPosition 1 maxResults 1000 &minorversion={_QBOParameters.MinorVersion}";
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.GetAllChartOfAccounts,
                Request = Service_Url
            };
            ServiceResponse serviceResponse = new ServiceResponse();
            AccountsResponse accountsRes = null!;
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            if (_Token == null)
                _Token = RefreshAccessToken();

            Headers.Add("Authorization", "Bearer " + _Token.AccessToken);
            try
            {
                accountsRes = (AccountsResponse)HttpServiceUtility.ExecuteJsonPostRequest(_ServiceLogInfo, Service_Url, null, typeof(AccountsResponse), false, Headers);
            }
            catch (Exception ex)
            {
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }
            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);
            return accountsRes;
        }

        public ReadByIdRes GetJournalEntryById(string Id)
        {
            _ServiceLogInfo = new ServiceLogInfo();

            _ServiceLogInfo.Client_Id = _Client_Id;
            _ServiceLogInfo.Service_Id = _Service_Id;
            _ServiceLogInfo.ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.ReadJournalEntryById;
            ReadByIdRes readByIdRes = new ReadByIdRes();
            string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/journalentry/{Id}";
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            if (_Token == null)
                _Token = RefreshAccessToken();

            Headers.Add("Authorization", "Bearer " + _Token.AccessToken);
            _ServiceLogInfo.Request = Service_Url;
            try
            {
                readByIdRes = (ReadByIdRes)HttpServiceUtility.ExecuteJsonGetRequest(_ServiceLogInfo, Service_Url, typeof(ReadByIdRes), false, Headers);
            }
            catch (Exception ex)
            {
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }
            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);
            return readByIdRes;
        }
        public CreateJournalRes CreateInvoiceJournalEntry(SqlTransaction Transaction, PolicyDetails policyDetails)
        {
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.CreateJournalEntry
            };
            CreateJournalRes createJournalResponse = new CreateJournalRes();
            CreateJournalReq createJournalReq = new CreateJournalReq();
            try
            {
                createJournalReq = PrepareCreateInvoiceJournalReqObj(Transaction, policyDetails);
                if (createJournalReq.Fault?.Error?.Any() == true)
                {
                    var errorMessages = string.Join(Environment.NewLine + Environment.NewLine, createJournalReq.Fault.Error.Select(e => $"Message: {e.Message} {Environment.NewLine} Detail: {e.Detail}"));

                    throw new ApplicationException("Error while preparing the invoice journal entry request:" + Environment.NewLine + errorMessages);
                }

            }
            catch (Exception ex)
            {
                if (createJournalReq.Fault?.Error?.Any() == true)
                {
                    var errorMessages = string.Join(Environment.NewLine + Environment.NewLine, createJournalReq.Fault.Error.Select(e => $"Message: {e.Message} {Environment.NewLine} Detail: {e.Detail}"));

                    throw new ApplicationException("Error while preparing the invoice journal entry request:" + Environment.NewLine + errorMessages);
                }
                else
                {
                    throw new ApplicationException(ex.Message, ex);
                }
            }
            string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/journalentry?minorversion={_QBOParameters.MinorVersion}";
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            if (_Token == null)
                _Token = RefreshAccessToken();

            Headers.Add("Authorization", "Bearer " + _Token.AccessToken);
            try
            {
                createJournalResponse = (CreateJournalRes)HttpServiceUtility.ExecuteJsonPostRequest(_ServiceLogInfo, Service_Url, createJournalReq, typeof(CreateJournalRes), false, Headers);
                if (!string.IsNullOrEmpty(createJournalResponse?.JournalEntry?.Id))
                {
                    JournalEntries journalEntries = InsertJournalEntries(Transaction, MasterLookup._Sources.Invoice, policyDetails.Invoices.Id, createJournalResponse.JournalEntry.Id, null, createJournalResponse.JournalEntry.DocNumber);
                    if (journalEntries != null)
                    {
                        InsertJournalEntriesLog(Transaction, createJournalResponse?.JournalEntry?.Line, journalEntries);
                    }
                }
            }
            catch (Exception ex)
            {
                if (createJournalReq.Fault?.Error?.Any() == true)
                {
                    var errorMessages = string.Join("\n", createJournalReq.Fault.Error.Select(e => $"{e.Message}" + (!string.IsNullOrEmpty(e.Detail) ? ("Detail: " + e.Detail) : "")));

                    throw new ApplicationException("Error while preparing the invoice journal entry request:\n\n" + errorMessages);
                }
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }
            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);

            return createJournalResponse;
        }
        public CreateJournalRes UpdateInvoiceJournalEntry(SqlTransaction Transaction, PolicyDetails policyDetails, JournalEntries journalEntries)
        {
            ReadByIdRes readByIdRes = GetJournalEntryById(journalEntries.ExternalJournalEntryIdentifier);
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.UpdateJournalEntry
            };
            string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/journalentry?minorversion={_QBOParameters.MinorVersion}";
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            if (_Token == null)
                _Token = RefreshAccessToken();
            Headers.Add("Authorization", "Bearer " + _Token.AccessToken);
            CreateJournalRes updateJournalResponse = new CreateJournalRes();
            if (readByIdRes != null)
            {
                UpdateJournalReq updateJournalRequest = new UpdateJournalReq();
                try
                {
                    updateJournalRequest = PrepareUpdateInvoiceJournalReqObj(policyDetails, readByIdRes);

                }
                catch (Exception ex)
                {
                    if (updateJournalRequest.Fault?.Error?.Any() == true)
                    {
                        var errorMessages = string.Join("\n", updateJournalRequest.Fault.Error.Select(e => $"{e.Message} ({e.Detail})"));

                        throw new ApplicationException(errorMessages);
                    }
                    else
                    {
                        throw new ApplicationException("Error while preparing the invoice journal entry request.\n" + ex.Message, ex);
                    }
                }
                try
                {
                    updateJournalResponse = (CreateJournalRes)HttpServiceUtility.ExecuteJsonPostRequest(_ServiceLogInfo, Service_Url, updateJournalRequest, typeof(CreateJournalRes), false, Headers);
                    if (!string.IsNullOrEmpty(updateJournalResponse?.JournalEntry?.Id))
                    {
                        if (journalEntries != null)
                        {
                            _JournalEntryLogRepository.InactiveJournalEntryLogsByJournalEntry_Id(Transaction, journalEntries.JournalEntry_Id);
                            InsertJournalEntriesLog(Transaction, updateJournalResponse?.JournalEntry?.Line, journalEntries);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _ServiceLogInfo.Is_Success = false;
                    _ServiceLogInfo.Message = ex.Message;
                }
                _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);
            }
            return updateJournalResponse;
        }

        public CreateJournalRes CreatePaymentJournalEntry(SqlTransaction PaymentsTransaction, Payments payments, List<PaymentApplicationLineItems> PaymentApplicationLineItemsList = null, int journalIndex = 0)
        {
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.CreateJournalEntry
            };
            CreateJournalRes createJournalResponse = new CreateJournalRes();
            CreateJournalReq createJournalReq = new CreateJournalReq();
            try
            {
                createJournalReq = PrepareCreatePaymentJournalReqObj(PaymentsTransaction, payments, journalIndex);
                if (createJournalReq.Fault?.Error?.Any() == true)
                {
                    var errorMessages = string.Join("\n", createJournalReq.Fault.Error.Select(e => $"{e.Message} ({e.Detail})"));

                    throw new ApplicationException("Error while preparing the invoice journal entry request:\n\n" + errorMessages);
                }
            }
            catch (Exception ex)
            {
                if (createJournalReq.Fault?.Error?.Any() == true)
                {
                    var errorMessages = string.Join("\n", createJournalReq.Fault.Error.Select(e => $"{e.Message} ({e.Detail})"));

                    throw new ApplicationException("Error while preparing the invoice journal entry request:\n\n" + errorMessages);
                }
                throw new ApplicationException(ex.Message, ex);
            }
            string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/journalentry?minorversion={_QBOParameters.MinorVersion}";
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            if (_Token == null)
                _Token = RefreshAccessToken();

            Headers.Add("Authorization", "Bearer " + _Token.AccessToken);
            try
            {
                createJournalResponse = (CreateJournalRes)HttpServiceUtility.ExecuteJsonPostRequest(_ServiceLogInfo, Service_Url, createJournalReq, typeof(CreateJournalRes), false, Headers);
                if (!string.IsNullOrEmpty(createJournalResponse?.JournalEntry?.Id))
                {
                    JournalEntries journalEntries = InsertJournalEntries(PaymentsTransaction, MasterLookup._Sources.Payment, payments.Payment_Id, createJournalResponse.JournalEntry.Id, journalIndex, createJournalResponse.JournalEntry.DocNumber);
                    if (journalEntries != null)
                    {
                        InsertJournalEntriesLog(PaymentsTransaction, createJournalResponse?.JournalEntry?.Line, journalEntries);
                    }
                }
            }
            catch (Exception ex)
            {
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }
            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);


            return createJournalResponse;
        }

        //public CreateJournalRes CreatePaymentJournalEntry(SqlTransaction PaymentsTransaction, Payments payments)
        //{
        //    _ServiceLogInfo = new ServiceLogInfo()
        //    {
        //        Client_Id = _Client_Id,
        //        Service_Id = _Service_Id,
        //        ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.CreateJournalEntry
        //    };
        //    CreateJournalRes createJournalResponse = new CreateJournalRes();
        //    CreateJournalReq createJournalReq = new CreateJournalReq();
        //    try
        //    {
        //        createJournalReq = PrepareCreatePaymentJournalReqObj(PaymentsTransaction, payments);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ApplicationException("Error while preparing the payment journal entry request. Please check the COA/LOB – Carrier grouping/other configurations.\n" + ex.Message, ex);
        //    }
        //    string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/journalentry?minorversion={_QBOParameters.MinorVersion}";
        //    Dictionary<string, string> Headers = new Dictionary<string, string>();
        //    if (_Token == null)
        //        _Token = RefreshAccessToken();

        //    Headers.Add("Authorization", "Bearer " + _Token.AccessToken);
        //    try
        //    {
        //        createJournalResponse = (CreateJournalRes)HttpServiceUtility.ExecuteJsonPostRequest(_ServiceLogInfo, Service_Url, createJournalReq, typeof(CreateJournalRes), false, Headers);
        //        if (!string.IsNullOrEmpty(createJournalResponse?.JournalEntry?.Id))
        //        {
        //            JournalEntries journalEntries = InsertJournalEntries(PaymentsTransaction, MasterLookup._Sources.Payment, payments.Payment_Id, createJournalResponse.JournalEntry.Id);
        //            if (journalEntries != null)
        //            {
        //                InsertJournalEntriesLog(PaymentsTransaction, createJournalResponse?.JournalEntry?.Line, journalEntries);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _ServiceLogInfo.Is_Success = false;
        //        _ServiceLogInfo.Message = ex.Message;
        //    }
        //    _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);


        //    return createJournalResponse;
        //}
        //public UpdateJournalRes CreatePaymentWriteoffJournalEntry(SqlTransaction PaymentsTransaction, Payments payments, PaymentApplications paymentApplications, PaymentApplicationLineItems PaymentApplicationLineItems, JournalEntries journalEntries)
        //{
        //    ReadByIdRes readByIdRes = GetJournalEntryById(journalEntries.ExternalJournalEntryIdentifier);
        //    _ServiceLogInfo = new ServiceLogInfo()
        //    {
        //        Client_Id = _Client_Id,
        //        Service_Id = _Service_Id,
        //        ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.UpdateJournalEntry,
        //    };
        //    string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/journalentry?minorversion={_QBOParameters.MinorVersion}";
        //    Dictionary<string, string> Headers = new Dictionary<string, string>();
        //    if (_Token == null)
        //        _Token = RefreshAccessToken();

        //    Headers.Add("Authorization", "Bearer " + _Token.AccessToken);
        //    UpdateJournalRes updateJournalResponse = new UpdateJournalRes();
        //    if (readByIdRes != null)
        //    {
        //        UpdateJournalReq updateJournalRequest = new UpdateJournalReq();
        //        try
        //        {
        //            updateJournalRequest = PrepareCreatePaymentWriteoffJournalReqObj(PaymentsTransaction, readByIdRes, payments, paymentApplications, PaymentApplicationLineItems, journalEntries);
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new ApplicationException("Error while preparing the payment(write off) journal entry request. Please check the COA/LOB – Carrier grouping/other configurations.\n" + ex.Message, ex);
        //        }
        //        try
        //        {
        //            updateJournalResponse = (UpdateJournalRes)HttpServiceUtility.ExecuteJsonPostRequest(_ServiceLogInfo, Service_Url, updateJournalRequest, typeof(UpdateJournalRes), false, Headers);
        //        }
        //        catch (Exception ex)
        //        {
        //            _ServiceLogInfo.Is_Success = false;
        //            _ServiceLogInfo.Message = ex.Message;
        //        }
        //        _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);
        //    }
        //    return updateJournalResponse;
        //}
        public CreateJournalRes CreatePaymentWriteoffJournalEntry(SqlTransaction PaymentsTransaction, Payments payments, PaymentApplications paymentApplications, PaymentApplicationLineItems PaymentApplicationLineItems, JournalEntries journalEntries, bool Is_Update = true, bool Is_Delete = false)
        {
            ReadByIdRes readByIdRes = GetJournalEntryById(journalEntries.ExternalJournalEntryIdentifier);
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.CreateJournalEntry,
            };
            string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/journalentry?minorversion={_QBOParameters.MinorVersion}";
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            if (_Token == null)
                _Token = RefreshAccessToken();

            Headers.Add("Authorization", "Bearer " + _Token.AccessToken);
            CreateJournalRes createJournalResponse = new();
            if (readByIdRes != null)
            {
                CreateJournalReq createJournalRequest = new();
                try
                {
                    if (Is_Update)
                    {
                        createJournalRequest = PreparePaymentWriteoffUpdateJournalReqObj(PaymentsTransaction, readByIdRes, payments, paymentApplications, PaymentApplicationLineItems, journalEntries);
                        if (createJournalRequest.Fault?.Error?.Any() == true)
                        {
                            var errorMessages = string.Join("\n", createJournalRequest.Fault.Error.Select(e => $"{e.Message} ({e.Detail})"));

                            throw new ApplicationException("Error while preparing payment write off request:\n\n" + errorMessages);
                        }
                    }
                    else if (Is_Delete)
                    {
                        createJournalRequest = PrepareUnApplyPaymentWriteoffUpdateJournalReqObj(PaymentsTransaction, payments, paymentApplications, PaymentApplicationLineItems, journalEntries, Is_Delete);
                        if (createJournalRequest.Fault?.Error?.Any() == true)
                        {
                            var errorMessages = string.Join("\n", createJournalRequest.Fault.Error.Select(e => $"{e.Message} ({e.Detail})"));

                            throw new ApplicationException("Error while preparing payment write off request:\n\n" + errorMessages);
                        }
                    }
                    else
                    {
                        createJournalRequest = PrepareCreatePaymentWriteoffJournalReqObj(PaymentsTransaction, readByIdRes, payments, paymentApplications, PaymentApplicationLineItems, journalEntries);
                        if (createJournalRequest.Fault?.Error?.Any() == true)
                        {
                            var errorMessages = string.Join("\n", createJournalRequest.Fault.Error.Select(e => $"{e.Message} ({e.Detail})"));

                            throw new ApplicationException(errorMessages);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (createJournalRequest.Fault?.Error?.Any() == true)
                    {
                        var errorMessages = string.Join("\n", createJournalRequest.Fault.Error.Select(e => $"{e.Message} ({e.Detail})"));

                        throw new ApplicationException("Error while preparing payment write off request:\n\n" + errorMessages);
                    }
                    throw new ApplicationException("Error while preparing the payment(write off) journal entry request\n" + ex.Message, ex);
                }
                try
                {
                    createJournalResponse = (CreateJournalRes)HttpServiceUtility.ExecuteJsonPostRequest(_ServiceLogInfo, Service_Url, createJournalRequest, typeof(CreateJournalRes), false, Headers);
                    if (!string.IsNullOrEmpty(createJournalResponse?.JournalEntry?.Id))
                    {
                        int updatedJournalIndex = (journalEntries?.JournalIndex ?? 0) + 1;
                        JournalEntries entries = new JournalEntries();
                        entries.JournalIndex = updatedJournalIndex;
                        entries.ExternalJournalEntryIdentifier = createJournalResponse.JournalEntry?.Id;
                        entries.Source_Id = payments.Payment_Id;
                        entries.SourceType_Id = MasterLookup._Sources.Payment;
                        entries.JournalEntryName = createJournalResponse.JournalEntry?.DocNumber;
                        JournalEntries journalEntry = InsertJournalEntries(PaymentsTransaction, MasterLookup._Sources.Payment, payments.Payment_Id, createJournalResponse.JournalEntry.Id, entries.JournalIndex, createJournalResponse.JournalEntry.DocNumber);
                        if (journalEntries != null)
                        {
                            InsertJournalEntriesLog(PaymentsTransaction, createJournalResponse?.JournalEntry?.Line, journalEntry);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _ServiceLogInfo.Is_Success = false;
                    _ServiceLogInfo.Message = ex.Message;
                }
                _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);
            }
            return createJournalResponse;
        }

        public CreateJournalRes UpdateJournalEntry(SqlTransaction PaymentsTransaction, Payments payments, List<PaymentApplicationLineItems> PaymentApplicationLineItemsList, JournalEntries journalEntries, bool Is_Apply = true, PaymentApplicationLineItems unApplyPaymentApplicationLineItems = null)
        {
            ReadByIdRes readByIdRes = GetJournalEntryById(journalEntries.ExternalJournalEntryIdentifier);
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.UpdateJournalEntry,
            };
            string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/journalentry?minorversion={_QBOParameters.MinorVersion}";
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            if (_Token == null)
                _Token = RefreshAccessToken();

            Headers.Add("Authorization", "Bearer " + _Token.AccessToken);
            CreateJournalRes updateJournalResponse = new CreateJournalRes();
            if (readByIdRes != null)
            {
                UpdateJournalReq updateJournalRequest = new UpdateJournalReq();
                try
                {
                    if (Is_Apply)
                    {
                        updateJournalRequest = PrepareUpdateJournalReqObj(PaymentsTransaction, readByIdRes, payments, PaymentApplicationLineItemsList, journalEntries);
                        if (updateJournalRequest.Fault?.Error?.Any() == true)
                        {
                            var errorMessages = string.Join("\n", updateJournalRequest.Fault.Error.Select(e => $"{e.Message} ({e.Detail})"));

                            throw new ApplicationException("Error while preparing the update journal entry request:\n\n" + errorMessages);
                        }
                    }
                    else
                    {
                        updateJournalRequest = PrepareUnApplyUpdateJournalReqObj(PaymentsTransaction, unApplyPaymentApplicationLineItems, journalEntries);
                        if (updateJournalRequest.Fault?.Error?.Any() == true)
                        {
                            var errorMessages = string.Join("\n", updateJournalRequest.Fault.Error.Select(e => $"{e.Message} ({e.Detail})"));

                            throw new ApplicationException("Error while preparing the update journal entry request:\n\n" + errorMessages);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (updateJournalRequest.Fault?.Error?.Any() == true)
                    {
                        var errorMessages = string.Join("\n", updateJournalRequest.Fault.Error.Select(e => $"{e.Message} ({e.Detail})"));

                        throw new ApplicationException("Error while preparing the update journal entry request:\n\n" + errorMessages);
                    }
                    throw new ApplicationException(ex.Message, ex);
                }
                try
                {
                    updateJournalResponse = (CreateJournalRes)HttpServiceUtility.ExecuteJsonPostRequest(_ServiceLogInfo, Service_Url, updateJournalRequest, typeof(CreateJournalRes), false, Headers);

                }
                catch (Exception ex)
                {
                    _ServiceLogInfo.Is_Success = false;
                    _ServiceLogInfo.Message = ex.Message;
                }
                _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);
            }
            return updateJournalResponse;
        }
        //public UpdateJournalRes UpdateJournalEntry(SqlTransaction PaymentsTransaction, Payments payments, List<PaymentApplicationLineItems> PaymentApplicationLineItemsList, JournalEntries journalEntries, bool Is_Apply = true, PaymentApplicationLineItems unApplyPaymentApplicationLineItems = null)
        //{
        //    ReadByIdRes readByIdRes = GetJournalEntryById(journalEntries.ExternalJournalEntryIdentifier);
        //    _ServiceLogInfo = new ServiceLogInfo()
        //    {
        //        Client_Id = _Client_Id,
        //        Service_Id = _Service_Id,
        //        ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.UpdateJournalEntry,
        //    };
        //    string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/journalentry?minorversion={_QBOParameters.MinorVersion}";
        //    Dictionary<string, string> Headers = new Dictionary<string, string>();
        //    if (_Token == null)
        //        _Token = RefreshAccessToken();

        //    Headers.Add("Authorization", "Bearer " + _Token.AccessToken);
        //    UpdateJournalRes updateJournalResponse = new UpdateJournalRes();
        //    if (readByIdRes != null)
        //    {
        //        UpdateJournalReq updateJournalRequest = new UpdateJournalReq();
        //        try
        //        {
        //            if (Is_Apply)
        //            {
        //                updateJournalRequest = PrepareUpdateJournalReqObj(PaymentsTransaction, readByIdRes, payments, PaymentApplicationLineItemsList, journalEntries);
        //            }
        //            else
        //            {
        //                updateJournalRequest = PrepareUnApplyUpdateJournalReqObj(PaymentsTransaction, unApplyPaymentApplicationLineItems, journalEntries);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new ApplicationException("Error while preparing the payment journal entry request. Please check the COA/LOB – Carrier grouping/other configurations.\n" + ex.Message, ex);
        //        }
        //        try
        //        {
        //            updateJournalResponse = (UpdateJournalRes)HttpServiceUtility.ExecuteJsonPostRequest(_ServiceLogInfo, Service_Url, updateJournalRequest, typeof(UpdateJournalRes), false, Headers);
        //        }
        //        catch (Exception ex)
        //        {
        //            _ServiceLogInfo.Is_Success = false;
        //            _ServiceLogInfo.Message = ex.Message;
        //        }
        //        _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);
        //    }
        //    return updateJournalResponse;
        //}
        public CreateJournalRes UpdatePaymentWriteoffJournalEntry(SqlTransaction PaymentsTransaction, Payments payments, PaymentApplications paymentApplications, PaymentApplicationLineItems PaymentApplicationLineItems, JournalEntries journalEntries, bool Is_Update = true, bool Is_Delete = false)
        {
            ReadByIdRes readByIdRes = GetJournalEntryById(journalEntries.ExternalJournalEntryIdentifier);
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.UpdateJournalEntry,
            };
            string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/journalentry?minorversion={_QBOParameters.MinorVersion}";
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            if (_Token == null)
                _Token = RefreshAccessToken();

            Headers.Add("Authorization", "Bearer " + _Token.AccessToken);
            CreateJournalRes createJournalResponse = new CreateJournalRes();
            if (readByIdRes != null)
            {
                CreateJournalReq createJournalRequest = new CreateJournalReq();
                try
                {
                    if (Is_Update)
                    {
                        createJournalRequest = PreparePaymentWriteoffUpdateJournalReqObj(PaymentsTransaction, readByIdRes, payments, paymentApplications, PaymentApplicationLineItems, journalEntries);
                        if (createJournalRequest.Fault?.Error?.Any() == true)
                        {
                            var errorMessages = string.Join("\n", createJournalRequest.Fault.Error.Select(e => $"{e.Message} ({e.Detail})"));

                            throw new ApplicationException("Error while preparing the payment write off request:\n\n" + errorMessages);
                        }
                    }
                    else if (Is_Delete)
                    {
                        createJournalRequest = PrepareUnApplyPaymentWriteoffUpdateJournalReqObj(PaymentsTransaction, payments, paymentApplications, PaymentApplicationLineItems, journalEntries, Is_Delete);
                        if (createJournalRequest.Fault?.Error?.Any() == true)
                        {
                            var errorMessages = string.Join("\n", createJournalRequest.Fault.Error.Select(e => $"{e.Message} ({e.Detail})"));

                            throw new ApplicationException("Error while preparing the payment write off request:\n\n" + errorMessages);
                        }
                    }
                    else
                    {
                        createJournalRequest = PrepareUnApplyPaymentWriteoffUpdateJournalReqObj(PaymentsTransaction, payments, paymentApplications, PaymentApplicationLineItems, journalEntries, Is_Delete);
                        if (createJournalRequest.Fault?.Error?.Any() == true)
                        {
                            var errorMessages = string.Join("\n", createJournalRequest.Fault.Error.Select(e => $"{e.Message} ({e.Detail})"));

                            throw new ApplicationException(errorMessages);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Error while preparing the payment(write off) journal entry request.\n" + ex.Message, ex);
                }

                try
                {
                    createJournalResponse = (CreateJournalRes)HttpServiceUtility.ExecuteJsonPostRequest(_ServiceLogInfo, Service_Url, createJournalRequest, typeof(CreateJournalRes), false, Headers);
                }
                catch (Exception ex)
                {
                    _ServiceLogInfo.Is_Success = false;
                    _ServiceLogInfo.Message = ex.Message;
                }
                _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);
            }
            return createJournalResponse;
        }

        public CreateJournalRes CreateJournalEntry(SqlTransaction PaymentsTransaction, Payments payments, List<PaymentApplicationLineItems> PaymentApplicationLineItemsList, JournalEntries journalEntries, bool Is_Apply = true, PaymentApplicationLineItems unApplyPaymentApplicationLineItems = null)
        {
            ReadByIdRes readByIdRes = GetJournalEntryById(journalEntries.ExternalJournalEntryIdentifier);
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.CreateJournalEntry,
            };
            string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/journalentry?minorversion={_QBOParameters.MinorVersion}";
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            if (_Token == null)
                _Token = RefreshAccessToken();

            Headers.Add("Authorization", "Bearer " + _Token.AccessToken);
            CreateJournalRes createJournalResponse = new CreateJournalRes();
            if (readByIdRes != null)
            {
                CreateJournalReq createJournalRequest = new CreateJournalReq();
                try
                {
                    if (Is_Apply)
                    {
                        createJournalRequest = PrepareCreateJournalReqObj(PaymentsTransaction, readByIdRes, payments, PaymentApplicationLineItemsList, journalEntries);
                        if (createJournalRequest.Fault?.Error?.Any() == true)
                        {
                            var errorMessages = string.Join(Environment.NewLine + Environment.NewLine, createJournalRequest.Fault.Error.Select(e => $"{e.Message} ({e.Detail})"));

                            throw new ApplicationException("Error while preparing the invoice journal entry request:" + Environment.NewLine + Environment.NewLine + errorMessages);
                        }
                    }
                    else
                    {
                        createJournalRequest = PrepareUnApplyCreateJournalReqObj(PaymentsTransaction, unApplyPaymentApplicationLineItems, journalEntries);
                        if (createJournalRequest.Fault?.Error?.Any() == true)
                        {
                            var errorMessages = string.Join("\n", createJournalRequest.Fault.Error.Select(e => $"{e.Message} ({e.Detail})"));

                            throw new ApplicationException("Error while preparing the invoice journal entry request:\n\n" + errorMessages);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(ex.Message, ex);
                }


                try
                {
                    createJournalResponse = (CreateJournalRes)HttpServiceUtility.ExecuteJsonPostRequest(_ServiceLogInfo, Service_Url, createJournalRequest, typeof(CreateJournalRes), false, Headers);
                    if (!string.IsNullOrEmpty(createJournalResponse?.JournalEntry?.Id))
                    {
                        int updatedJournalIndex = (journalEntries?.JournalIndex ?? 0) + 1;
                        JournalEntries entries = new JournalEntries();
                        entries.JournalIndex = updatedJournalIndex;
                        entries.ExternalJournalEntryIdentifier = createJournalResponse.JournalEntry?.Id;
                        entries.Source_Id = payments.Payment_Id;
                        entries.SourceType_Id = MasterLookup._Sources.Payment;
                        entries.JournalEntryName = createJournalResponse.JournalEntry?.DocNumber;
                        JournalEntries journalEntry = InsertJournalEntries(PaymentsTransaction, MasterLookup._Sources.Payment, payments.Payment_Id, createJournalResponse.JournalEntry.Id, entries.JournalIndex, createJournalResponse.JournalEntry.DocNumber);
                        if (journalEntries != null)
                        {
                            InsertJournalEntriesLog(PaymentsTransaction, createJournalResponse?.JournalEntry?.Line, journalEntry);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _ServiceLogInfo.Is_Success = false;
                    _ServiceLogInfo.Message = ex.Message;
                }
                _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);
            }
            return createJournalResponse;
        }

        public DeleteJournalResponse DeleteJournalEntry(JournalEntries journalEntries)
        {

            ReadByIdRes readByIdRes = GetJournalEntryById(journalEntries.ExternalJournalEntryIdentifier);
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.DeleteJournalEntry,
            };
            DeleteJournalReq deleteJournal = new DeleteJournalReq();
            DeleteJournalResponse deleteJournalResponse = new DeleteJournalResponse();
            deleteJournal.Id = journalEntries.ExternalJournalEntryIdentifier;
            deleteJournal.SyncToken = readByIdRes.JournalEntry?.SyncToken;
            string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/journalentry?operation=delete";
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            if (_Token == null)
                _Token = RefreshAccessToken();

            Headers.Add("Authorization", "Bearer " + _Token.AccessToken);
            try
            {
                deleteJournalResponse = (DeleteJournalResponse)HttpServiceUtility.ExecuteJsonPostRequest(_ServiceLogInfo, Service_Url, deleteJournal, typeof(DeleteJournalResponse), false, Headers);
            }
            catch (Exception ex)
            {
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }
            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);
            return deleteJournalResponse;
        }
        public TokenResponse RefreshAccessToken()
        {
            try
            {
                string RefreshToken = null;
                ServiceLogInfo _LatestServiceLogInfo = _ServiceLogInfoRepository.GetServiceLogInfo(_Service_Id, _Client_Id, MasterLookup._ServiceRequestTypes.GetAccessToken);
                if (_LatestServiceLogInfo != null)
                {
                    TokenResponse tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(_LatestServiceLogInfo.Response);
                    if (tokenResponse != null)
                    {
                        RefreshToken = tokenResponse?.RefreshToken;
                    }
                    else
                    {
                        RefreshToken = _QBOParameters.RefreshToken;
                    }
                }
                else
                {
                    RefreshToken = _QBOParameters.RefreshToken;
                }
                if (string.IsNullOrEmpty(RefreshToken))
                {
                    var ClientLevelParameters = LookupUtility.GetAllClientLevelParameters(_Client_Id);
                    RefreshToken = ClientLevelParameters?.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.RefreshToken)?.Value ?? string.Empty;
                }

                string postData = $"grant_type=refresh_token&refresh_token={RefreshToken}";
                _ServiceLogInfo = new ServiceLogInfo()
                {
                    Client_Id = _Client_Id,
                    Service_Id = _Service_Id,
                    ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.GetAccessToken,
                };
                Dictionary<string, string> Headers = new Dictionary<string, string>();
                string authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_QBOParameters.Client_Id}:{_QBOParameters.Client_Secret_Id}"));
                Headers.Add("Authorization", $"Basic {authHeader}");
                var ContentType = "application/x-www-form-urlencoded";
                _Token = (TokenResponse)HttpServiceUtility.ExecuteJsonPostRequest(_ServiceLogInfo, _QBOParameters.TokenEndpoint, postData, typeof(TokenResponse), false, Headers, false, ContentType);
                _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);
                if (!string.IsNullOrEmpty(_Token?.RefreshToken))
                {
                    _lookupRepository.UpdateQBORefreshToken(_Client_Id, _Service_Id, MasterLookup._Parameters.RefreshToken, _Token.RefreshToken);
                }
                return _Token;
            }
            catch (Exception ex)
            {
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }
            return _Token;
        }

        public TokenResponse GetAuthToken()
        {
            string key = $"AccessToken_{_Client_Id}";
            TokenResponse authTokenDetails = null;
            if (!_MemoryCache.TryGetValue(key, out authTokenDetails))
            {
                authTokenDetails = RefreshAccessToken();
                _MemoryCache.Set(key, authTokenDetails, TimeSpan.FromHours(1));
            }
            if (authTokenDetails != null)
                return CommonUtility.DeepClone<TokenResponse>(authTokenDetails);
            return null;
        }
        //public Fault AddError(Fault fault, string message, string detail = null)
        //{
        //    if (fault == null)
        //    {
        //        fault = new Fault
        //        {
        //            Error = new List<Error>(),
        //            type = "ValidationFault"
        //        };
        //    }

        //    fault.Error.Add(new Error
        //    {
        //        Message = message,
        //        Detail = detail,
        //        code = "400"
        //    });
        //    return fault;
        //}

        public Fault AddError(Fault fault, string message, string detail = null)
        {
            if (fault == null)
            {
                fault = new Fault
                {
                    type = "ValidationFault"
                };
            }

            if (fault.Error == null)
            {
                fault.Error = new List<Error>();
            }

            fault.Error.Add(new Error
            {
                Message = message,
                Detail = detail,
            });

            return fault;
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
        public CreateJournalReq PrepareCreateInvoiceJournalReqObj(SqlTransaction InvoiceTransaction, PolicyDetails policyDetails)
        {
            CreateJournalReq createJournalReq = new CreateJournalReq();
            var journalList = new List<JournalEntryLine>();


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
                var JournalEntryDebit = new JournalEntryLine();
                var journalEntryCredit = new JournalEntryLine();
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
                        var DebitLineItemCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == DebitLineItemCOA?.ChartOfAccount_Id);
                        var CreditLineItemCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == CreditLineItemCOA?.ChartOfAccount_Id);
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
                        var DebitLineItemCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == DebitLineItemCOA?.ChartOfAccount_Id);
                        var CreditLineItemCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == CreditLineItemCOA?.ChartOfAccount_Id);
                        if (DebitLineItemCOA == null || DebitLineItemCOAItem == null)
                        {
                            fault = AddError(fault, "Missing COA Configuration(Income/Expense)", errorDetail);
                            continue;
                        }
                        if (CreditLineItemCOA == null || CreditLineItemCOAItem == null)
                        {
                            fault = AddError(fault, "Missing COA Configuration(AR/AP)", errorDetail);
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
                            CreditCOAValue = DebitLineItemCOAItem?.ReferenceID;
                            CreditCOAName = DebitLineItemCOAItem?.Name;
                            DebitCOAValue = CreditLineItemCOAItem?.ReferenceID;
                            DebitCOAName = CreditLineItemCOAItem?.Name;
                            CreditDescription = $"Applied from Expense Account for invoice no. - {policyDetails.Invoices.InvoiceNo}, {LineItems.Name} ({Directions.Name}) with LOB - {LOB.Code}, and Carrier - {policyDetails.CarrierDetails.Name}";
                            DebitDescription = $"Applied to Accounts Payable Account for invoice no. - {policyDetails.Invoices.InvoiceNo}, {LineItems.Name} ({Directions.Name}) with LOB - {LOB.Code}, and Carrier - {policyDetails.CarrierDetails.Name}";
                        }

                    }
                    if (!string.IsNullOrEmpty(DebitCOAValue) && !string.IsNullOrEmpty(CreditCOAValue))
                    {
                        JournalEntryDebit.Amount = Math.Abs(item.Amount);
                        JournalEntryDebit.Description = DebitDescription;
                        JournalEntryDebit.DetailType = "JournalEntryLineDetail";
                        var JournalEntryLine = JournalEntryDebit.JournalEntryLineDetail = new JournalEntryLineDetail();
                        JournalEntryLine.PostingType = "Debit";
                        var AccountRef = JournalEntryLine.AccountRef = new AccountRef();
                        AccountRef.value = DebitCOAValue;
                        AccountRef.name = DebitCOAName;
                        journalList.Add(JournalEntryDebit);

                        journalEntryCredit.Amount = Math.Abs(item.Amount);
                        journalEntryCredit.Description = CreditDescription;
                        journalEntryCredit.DetailType = "JournalEntryLineDetail";
                        var JournalEntryLine2 = journalEntryCredit.JournalEntryLineDetail = new JournalEntryLineDetail();
                        JournalEntryLine2.PostingType = "Credit";
                        var AccountRef2 = JournalEntryLine2.AccountRef = new AccountRef();
                        AccountRef2.value = CreditCOAValue;
                        AccountRef2.name = CreditCOAName;
                        journalList.Add(journalEntryCredit);
                    }
                }
            }
            var useEffectivedate = LookupUtility.GetClientSettings(policyDetails.Client_Id, MasterLookup._ClientSettings.UseEffectiveDateForQBO);
            createJournalReq.TxnDate = string.IsNullOrEmpty(useEffectivedate) ? policyDetails.Invoices.CreatedOn.ToString("yyyy-MM-dd") : (policyDetails.TransactionType == MasterLookup._EITransactionTypes.END ? policyDetails?.EndorsementEffectiveDate?.ToString("yyyy-MM-dd") : policyDetails.EffectiveDate.ToString("yyyy-MM-dd"));
            createJournalReq.Line = journalList;
            createJournalReq.DocNumber = $"{policyDetails.Invoices.InvoiceNo} ({policyDetails.Invoices.Id})";

            if (fault != null && fault.Error != null && fault.Error.Any())
            {
                createJournalReq.Fault = fault;
            }
            return createJournalReq;
        }
        public UpdateJournalReq PrepareUpdateInvoiceJournalReqObj(PolicyDetails policyDetails, ReadByIdRes readByIdRes)
        {
            UpdateJournalReq updateJournalReq = new UpdateJournalReq();
            var journalList = new List<JournalEntryLine>();
            string DebitCOAValue = null;
            string CreditCOAValue = null;
            string DebitCOAName = null;
            string CreditCOAName = null;
            string DebitDescription = null;
            string CreditDescription = null;
            int? DebitCOA = null;
            int? CreditCOA = null;
            if (readByIdRes != null && readByIdRes.JournalEntry != null)
            {
                updateJournalReq.Adjustment = readByIdRes.JournalEntry.Adjustment;
                updateJournalReq.domain = readByIdRes.JournalEntry.domain;
                updateJournalReq.sparse = readByIdRes.JournalEntry.sparse;
                updateJournalReq.Id = readByIdRes.JournalEntry.Id;
                updateJournalReq.SyncToken = readByIdRes.JournalEntry.SyncToken;
                updateJournalReq.TxnDate = readByIdRes.JournalEntry.TxnDate;
                updateJournalReq.DocNumber = readByIdRes.JournalEntry.DocNumber;
                updateJournalReq.CurrencyRef = new Entities.External.QBO.Journal.CurrencyRef()
                {
                    value = readByIdRes.JournalEntry.CurrencyRef?.value,
                    name = readByIdRes.JournalEntry.CurrencyRef?.name
                };
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
                    var JournalEntryDebit = new JournalEntryLine();
                    var journalEntryCredit = new JournalEntryLine();
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
                            var DebitLineItemCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == DebitLineItemCOA?.ChartOfAccount_Id);
                            var CreditLineItemCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == CreditLineItemCOA?.ChartOfAccount_Id);
                            if (DebitLineItemCOA == null || DebitLineItemCOAItem == null)
                            {
                                fault = AddError(fault, "Missing COA Configuration(AR/AP)", errorDetail);
                                continue;
                            }
                            if (CreditLineItemCOA == null || CreditLineItemCOAItem == null)
                            {
                                fault = AddError(fault, "Missing COA Configuration(Income/Expense/Liability)", errorDetail);
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
                            var DebitLineItemCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == DebitLineItemCOA?.ChartOfAccount_Id);
                            var CreditLineItemCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == CreditLineItemCOA?.ChartOfAccount_Id);
                            if (DebitLineItemCOA == null || DebitLineItemCOAItem == null)
                            {
                                fault = AddError(fault, "Missing COA Configuration(Income/Expense).", errorDetail);
                                continue;
                            }
                            if (CreditLineItemCOA == null || CreditLineItemCOAItem == null)
                            {
                                fault = AddError(fault, "Missing COA Configuration(AR/AP)", errorDetail);
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
                            JournalEntryDebit.Amount = Math.Abs(item.Amount);
                            JournalEntryDebit.Description = DebitDescription;
                            JournalEntryDebit.DetailType = "JournalEntryLineDetail";
                            var JournalEntryLine = JournalEntryDebit.JournalEntryLineDetail = new JournalEntryLineDetail();
                            JournalEntryLine.PostingType = "Debit";
                            var AccountRef = JournalEntryLine.AccountRef = new AccountRef();
                            AccountRef.value = DebitCOAValue;
                            AccountRef.name = DebitCOAName;
                            journalList.Add(JournalEntryDebit);
                            journalEntryCredit.Amount = Math.Abs(item.Amount);
                            journalEntryCredit.Description = CreditDescription;
                            journalEntryCredit.DetailType = "JournalEntryLineDetail";
                            var JournalEntryLine2 = journalEntryCredit.JournalEntryLineDetail = new JournalEntryLineDetail();
                            JournalEntryLine2.PostingType = "Credit";
                            var AccountRef2 = JournalEntryLine2.AccountRef = new AccountRef();
                            AccountRef2.value = CreditCOAValue;
                            AccountRef2.name = CreditCOAName;
                            journalList.Add(journalEntryCredit);
                        }
                    }
                }
                updateJournalReq.Line = journalList;
            }
            if (fault != null && fault.Error != null && fault.Error.Any())
            {
                updateJournalReq.Fault = fault;
                return updateJournalReq;
            }
            return updateJournalReq;
        }
        public CreateJournalReq PrepareCreatePaymentJournalReqObj(SqlTransaction PaymentsTransaction, Payments payments, int journalIndex = 0)
        {
            CreateJournalReq createJournalReq = new CreateJournalReq();
            var journalList = new List<JournalEntryLine>();
            var JournalEntryDebit = new JournalEntryLine();

            JournalEntryDebit.Amount = payments.Amount;
            string DebitCOAValue = null;
            string CreditCOAValue = null;
            string DebitCOAName = null;
            string CreditCOAName = null;
            string DebitDescription = null;
            string CreditDescription = null;
            int? DebitCOA = null;
            int? CreditCOA = null;

            var OperatingBanksCOAList = _LookupRepository.GetAllOperatingBanksChartOfAccounts(_Service_Id, _Client_Id);
            var OperatingBankCOA = OperatingBanksCOAList.FirstOrDefault(x => x.OperatingBank_Id == payments.OperatingBank_Id);

            var OperatingBankCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == OperatingBankCOA?.ChartOfAccounts_Id);

            var PaymentDirectionsCOAList = _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id);
            var PaymentDirectionsCOA = PaymentDirectionsCOAList.FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);
            var PaymentDirectionsCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == PaymentDirectionsCOA?.StandardCOA_Id);
            if (OperatingBankCOA == null || OperatingBankCOAItem == null)
            {
                fault = AddError(fault, "Missing COA Configuration(Operating Bank)");
            }
            if (PaymentDirectionsCOA == null || PaymentDirectionsCOAItem == null)
            {
                fault = AddError(fault, "Missing COA Configuration(Payment direction)");
            }
            if (fault != null && fault.Error != null && fault.Error.Any())
            {
                createJournalReq.Fault = fault;
                return createJournalReq;
            }
            if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming)
            {
                DebitCOAValue = OperatingBankCOAItem?.ReferenceID;
                CreditCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                DebitDescription = $"Record incoming payment - reference number: {payments.ReferenceNumber}, payment made by {payments.EntityName}";
                CreditDescription = $"Applied to Incoming Payments Account";
            }
            else
            {
                DebitCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                CreditCOAValue = OperatingBankCOAItem?.ReferenceID;
                DebitDescription = $"Applied to Outgoing Payments Account";
                CreditDescription = $"Record outgoing payment - reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName}";
            }
            DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id).Name;
            CreditCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id).Name;
            DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id).ChartOfAccounts_Id;
            CreditCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id).ChartOfAccounts_Id;


            JournalEntryDebit.Description = DebitDescription;
            JournalEntryDebit.DetailType = "JournalEntryLineDetail";
            var JournalEntryLine = JournalEntryDebit.JournalEntryLineDetail = new JournalEntryLineDetail();
            JournalEntryLine.PostingType = "Debit";
            var AccountRef = JournalEntryLine.AccountRef = new AccountRef();
            AccountRef.value = DebitCOAValue;
            AccountRef.name = DebitCOAName;

            journalList.Add(JournalEntryDebit);

            var journalEntryCredit = new JournalEntryLine();
            journalEntryCredit.Amount = payments.Amount;
            journalEntryCredit.Description = CreditDescription;
            journalEntryCredit.DetailType = "JournalEntryLineDetail";
            var JournalEntryLine2 = journalEntryCredit.JournalEntryLineDetail = new JournalEntryLineDetail();
            JournalEntryLine2.PostingType = "Credit";
            var AccountRef2 = JournalEntryLine2.AccountRef = new AccountRef();
            AccountRef2.value = CreditCOAValue;
            AccountRef2.name = CreditCOAName;
            journalList.Add(journalEntryCredit);


            createJournalReq.Line = journalList;
            createJournalReq.DocNumber = $"PMT{payments.Payment_Id} ({journalIndex})";
            createJournalReq.TxnDate = payments?.PaymentDate?.ToString("yyyy-MM-dd");
            //createJournalReq.DocNumber = Convert.ToString(payments.Payment_Id);

            return createJournalReq;
        }

        //public UpdateJournalReq PrepareCreatePaymentWriteoffJournalReqObj(SqlTransaction PaymentsTransaction, ReadByIdRes readByIdRes, Payments payments, PaymentApplications paymentApplications, PaymentApplicationLineItems PaymentApplicationLineItems, JournalEntries journalEntries)
        //{
        //    UpdateJournalReq updateJournalReq = new UpdateJournalReq();
        //    var journalList = new List<JournalEntryLine>();

        //    var StandardPaymentDirectionsCOAList = _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id);
        //    var StandardPaymentDirectionsCOA = StandardPaymentDirectionsCOAList.FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);
        //    var StandardPaymentDirectionsCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == StandardPaymentDirectionsCOA.StandardCOA_Id);

        //    var WriteoffPaymentDirectionsCOAList = _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id);
        //    var WriteoffPaymentDirectionsCOA = WriteoffPaymentDirectionsCOAList.FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);
        //    var WriteoffPaymentDirectionsCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == WriteoffPaymentDirectionsCOA.WriteoffCOA_Id);

        //    if (readByIdRes != null && readByIdRes.JournalEntry != null)
        //    {
        //        updateJournalReq.Adjustment = readByIdRes.JournalEntry.Adjustment;
        //        updateJournalReq.domain = readByIdRes.JournalEntry.domain;
        //        updateJournalReq.sparse = readByIdRes.JournalEntry.sparse;
        //        updateJournalReq.Id = readByIdRes.JournalEntry.Id;
        //        updateJournalReq.SyncToken = readByIdRes.JournalEntry.SyncToken;
        //        updateJournalReq.TxnDate = readByIdRes.JournalEntry.TxnDate;
        //        updateJournalReq.DocNumber = readByIdRes.JournalEntry.DocNumber;
        //        updateJournalReq.CurrencyRef = new Entities.External.QBO.Journal.CurrencyRef()
        //        {
        //            value = readByIdRes.JournalEntry.CurrencyRef?.value,
        //            name = readByIdRes.JournalEntry.CurrencyRef?.name
        //        };
        //        updateJournalReq.Line = readByIdRes.JournalEntry.Line;
        //        if (PaymentApplicationLineItems != null)
        //        {
        //            string DebitCOAValue = null;
        //            string CreditCOAValue = null;
        //            string DebitCOAName = null;
        //            string CreditCOAName = null;
        //            string DebitDescription = null;
        //            string CreditDescription = null;
        //            int? DebitCOA = null;
        //            int? CreditCOA = null;

        //            if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming)
        //            {
        //                DebitCOAValue = StandardPaymentDirectionsCOAItem.ReferenceID;
        //                CreditCOAValue = WriteoffPaymentDirectionsCOAItem.ReferenceID;
        //                DebitDescription = $"[Added] - {paymentApplications.Notes}";
        //                CreditDescription = $"[Added] - {paymentApplications.Notes}";
        //                DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id).Name!;
        //                CreditCOAName = WriteoffPaymentDirectionsCOAItem.Name;
        //                DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id).ChartOfAccounts_Id!;
        //                CreditCOA = WriteoffPaymentDirectionsCOAItem.ChartOfAccounts_Id;
        //            }
        //            else
        //            {
        //                DebitCOAValue = WriteoffPaymentDirectionsCOAItem.ReferenceID;
        //                CreditCOAValue = StandardPaymentDirectionsCOAItem.ReferenceID;
        //                DebitDescription = $"[Added] - {paymentApplications.Notes}";
        //                CreditDescription = $"[Added] - {paymentApplications.Notes}";
        //                DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
        //                CreditCOAName = StandardPaymentDirectionsCOAItem.Name;
        //                DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
        //                CreditCOA = StandardPaymentDirectionsCOAItem.ChartOfAccounts_Id;
        //            }

        //            var JournalEntryDebit = new JournalEntryLine();
        //            JournalEntryDebit.Amount = Math.Abs(PaymentApplicationLineItems.AppliedAmount);
        //            JournalEntryDebit.Description = DebitDescription;
        //            JournalEntryDebit.DetailType = "JournalEntryLineDetail";
        //            var JournalEntryLine = JournalEntryDebit.JournalEntryLineDetail = new JournalEntryLineDetail();
        //            JournalEntryLine.PostingType = "Debit";
        //            var AccountRef = JournalEntryLine.AccountRef = new AccountRef();
        //            AccountRef.value = DebitCOAValue;
        //            AccountRef.name = DebitCOAName;
        //            updateJournalReq.Line.Add(JournalEntryDebit);

        //            JournalEntriesLog journalEntriesLogDebit = new JournalEntriesLog();
        //            journalEntriesLogDebit.JournalEntry_Id = journalEntries.JournalEntry_Id;
        //            journalEntriesLogDebit.DebitChartOfAccount_Id = DebitCOA;
        //            journalEntriesLogDebit.CreditChartOfAccount_Id = null;
        //            journalEntriesLogDebit.Description = DebitDescription;
        //            journalEntriesLogDebit.Amount = Math.Abs(PaymentApplicationLineItems.AppliedAmount);
        //            journalEntriesLogDebit.JournalEntryStatus_Id = MasterLookup._JournalEntryStatus.Applied;
        //            journalEntriesLogDebit.CreatedBy = _User_Id;
        //            _JournalEntryLogRepository.InsertJournalEntriesLog(PaymentsTransaction, journalEntriesLogDebit);

        //            var journalEntryCredit = new JournalEntryLine();
        //            journalEntryCredit.Amount = Math.Abs(PaymentApplicationLineItems.AppliedAmount);
        //            journalEntryCredit.Description = CreditDescription;
        //            journalEntryCredit.DetailType = "JournalEntryLineDetail";
        //            var JournalEntryLine2 = journalEntryCredit.JournalEntryLineDetail = new JournalEntryLineDetail();
        //            JournalEntryLine2.PostingType = "Credit";
        //            var AccountRef2 = JournalEntryLine2.AccountRef = new AccountRef();
        //            AccountRef2.value = CreditCOAValue;
        //            AccountRef2.name = CreditCOAName;
        //            updateJournalReq.Line.Add(journalEntryCredit);

        //            JournalEntriesLog journalEntriesLogCredit = new JournalEntriesLog();
        //            journalEntriesLogCredit.JournalEntry_Id = journalEntries.JournalEntry_Id;
        //            journalEntriesLogCredit.DebitChartOfAccount_Id = null;
        //            journalEntriesLogCredit.CreditChartOfAccount_Id = CreditCOA;
        //            journalEntriesLogCredit.Description = CreditDescription;
        //            journalEntriesLogCredit.Amount = Math.Abs(PaymentApplicationLineItems.AppliedAmount);
        //            journalEntriesLogCredit.JournalEntryStatus_Id = MasterLookup._JournalEntryStatus.Applied;
        //            journalEntriesLogCredit.CreatedBy = _User_Id;
        //            _JournalEntryLogRepository.InsertJournalEntriesLog(PaymentsTransaction, journalEntriesLogCredit);
        //        }
        //    }
        //    return updateJournalReq;
        //}

        public UpdateJournalReq PrepareUpdateJournalReqObj(SqlTransaction PaymentsTransaction, ReadByIdRes readByIdRes, Payments payments, List<PaymentApplicationLineItems> PaymentApplicationLineItemsList, JournalEntries journalEntries)
        {
            UpdateJournalReq updateJournalReq = new UpdateJournalReq();
            var journalList = new List<JournalEntryLine>();
            var ChartOfAccountsLineItemsList = _LookupRepository.GetAllLineItems_ChartOfAccounts(_Service_Id, _Client_Id);
            var PaymentDirectionsCOAList = _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id);
            var PaymentDirectionsCOA = PaymentDirectionsCOAList.FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);
            var PaymentDirectionsCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == PaymentDirectionsCOA?.StandardCOA_Id);
            byte _ChartOfAccountsClassificationType_Id = 0;
            if (PaymentDirectionsCOA == null)
            {
                fault = AddError(fault, "Missing COA Configuration(Payment Direction)");
            }
            if (readByIdRes != null && readByIdRes.JournalEntry != null)
            {
                updateJournalReq.Adjustment = readByIdRes.JournalEntry.Adjustment;
                updateJournalReq.domain = readByIdRes.JournalEntry.domain;
                updateJournalReq.sparse = readByIdRes.JournalEntry.sparse;
                updateJournalReq.Id = readByIdRes.JournalEntry.Id;
                updateJournalReq.SyncToken = readByIdRes.JournalEntry.SyncToken;
                updateJournalReq.TxnDate = readByIdRes.JournalEntry.TxnDate;
                updateJournalReq.DocNumber = readByIdRes.JournalEntry.DocNumber;
                updateJournalReq.CurrencyRef = new Entities.External.QBO.Journal.CurrencyRef()
                {
                    value = readByIdRes.JournalEntry.CurrencyRef?.value,
                    name = readByIdRes.JournalEntry.CurrencyRef?.name
                };
                updateJournalReq.Line = readByIdRes.JournalEntry.Line;
                if (PaymentApplicationLineItemsList?.Count() > 0)
                {
                    foreach (var paymentApplicationLineItems in PaymentApplicationLineItemsList)
                    {
                        _AccountingType_Id = paymentApplicationLineItems.AccountingType_Id;
                        if (paymentApplicationLineItems.AccountingType_Id == MasterLookup._AccountingTypes.Cash)
                        {
                            _ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability;
                        }
                        else
                        {
                            _ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.AccountsReceivableAccountsPayable;
                        }
                        string DebitCOAValue = null;
                        string CreditCOAValue = null;
                        string DebitCOAName = null;
                        string CreditCOAName = null;
                        string DebitDescription = null;
                        string CreditDescription = null;
                        int? DebitCOA = null;
                        int? CreditCOA = null;
                        InvoiceDetails invoiceDetails = _InvoiceRepository.GetInvoiceDetailsByInvoiceDetail_Id(paymentApplicationLineItems.InvoiceLineItem_Id);
                        Invoices invoices = _InvoiceRepository.GetInvoicesByInvoice_Id(invoiceDetails.Invoice_Id);
                        PolicyDetails policyDetails = _PolicyDetailsRepository.GetPolicyDetails(invoices.PolicyDetails_Id);


                        if (invoiceDetails != null)
                        {
                            LU_BillingType_LineItems_Directions BillingLineItemsDirections = LookupUtility.GetAllLU_BillingType_LineItems_Directions().FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id);
                            LU_LineItems_Directions LineItemsDirections = LookupUtility.GetAllLineItems_Directions().FirstOrDefault(x => x.LineItems_Directions_Id == BillingLineItemsDirections.LineItems_Directions_Id);
                            LU_LineItems_Clients lineItems_Clients = LookupUtility.GetAllLineItems_Clients().FirstOrDefault(x => x.LineItems_Clients_Id == LineItemsDirections.LineItems_Clients_Id);
                            LU_LineItems LineItems = LookupUtility.GetAllLineItems().FirstOrDefault(c => c.LineItem_Id == lineItems_Clients?.LineItem_Id);
                            LU_LineItemTypes LineType = LookupUtility.GetAllLineItemTypes().FirstOrDefault(c => c.LineItemType_Id == LineItems.LineItemType_Id);
                            LU_LOBs LOB = LookupUtility.GetAllLOBs().FirstOrDefault(x => x.LOB_Id == invoiceDetails.LOB_Id);
                            LOBCarrierGrouping lobcarriergroupingDetails = _LobCarrierGroupingRepository.GetLOBCarrierGroupingDetails(LOB.LOB_Id, policyDetails.CarrierDetail_Id, _Service_Id, _Client_Id);
                            var ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id == _ChartOfAccountsClassificationType_Id);
                            var ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);
                            var errorDetail = BuildErrorDetail(LineItems?.Name, LOB?.Name, policyDetails.BillType);


                            if (LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR)
                            {
                                bool isPositive = paymentApplicationLineItems.AppliedAmount > 0;
                                if (ChartOfAccountsLineItems == null || ChartOfAccounts == null)
                                {
                                    fault = AddError(fault, "Missing COA Configuration(Line-item)", errorDetail);
                                    continue;
                                }
                                DebitCOAValue = isPositive ? PaymentDirectionsCOAItem?.ReferenceID : ChartOfAccounts.ReferenceID;
                                CreditCOAValue = isPositive ? ChartOfAccounts.ReferenceID : PaymentDirectionsCOAItem?.ReferenceID;

                                string prefix = payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing ? "[REFUND] " :
                                                payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && !isPositive ? "[Netting] " : "";

                                DebitDescription = isPositive
                                    ? $"{prefix}Applying incoming payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made by {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}"
                                    : $"{prefix}Applied to Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";

                                CreditDescription = isPositive
                                    ? $"{prefix}Applied to Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}"
                                    : $"{prefix}Applying incoming payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made by {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";

                                DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                                CreditCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.Name!;

                                DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                                CreditCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                            }
                            else if (LineItemsDirections?.Direction_Id == MasterLookup._Directions.AP)
                            {
                                if (lineItems_Clients?.Is_Passthrough == true && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                                {
                                    ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id == MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability);
                                    ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);
                                }
                                bool isPositive = paymentApplicationLineItems.AppliedAmount > 0;
                                if (ChartOfAccountsLineItems == null || ChartOfAccounts == null)
                                {
                                    fault = AddError(fault, "Missing COA Configuration(Line-item)", errorDetail);
                                    continue;
                                }
                                DebitCOAValue = isPositive ? ChartOfAccounts.ReferenceID : PaymentDirectionsCOAItem.ReferenceID;
                                CreditCOAValue = isPositive ? PaymentDirectionsCOAItem.ReferenceID : ChartOfAccounts.ReferenceID;

                                string prefix = payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && !isPositive ? "[RECOVERY] " :
                                                payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && isPositive ? "[Netting] " : "";

                                DebitDescription = isPositive
                                    ? $"{prefix}Applied to Expense Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}"
                                    : $"{prefix}Applying incoming payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made by {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";

                                CreditDescription = isPositive
                                    ? $"{prefix}Applying outgoing payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}"
                                    : $"{prefix}Applied to expense account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";

                                DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                                CreditCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.Name!;

                                DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                                CreditCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                            }
                        }
                        var JournalEntryDebit = new JournalEntryLine();

                        JournalEntryDebit.Amount = Math.Abs(paymentApplicationLineItems.AppliedAmount);
                        JournalEntryDebit.Description = DebitDescription;
                        JournalEntryDebit.DetailType = "JournalEntryLineDetail";
                        var JournalEntryLine = JournalEntryDebit.JournalEntryLineDetail = new JournalEntryLineDetail();
                        JournalEntryLine.PostingType = "Debit";
                        var AccountRef = JournalEntryLine.AccountRef = new AccountRef();
                        AccountRef.value = DebitCOAValue;
                        AccountRef.name = DebitCOAName;
                        updateJournalReq.Line.Add(JournalEntryDebit);


                        //JournalEntriesLog journalEntriesLogDebit = new JournalEntriesLog();
                        ////journalEntriesLogDebit.JournalEntry_Id = journalEntries.JournalEntry_Id;
                        //journalEntriesLogDebit.DebitChartOfAccount_Id = DebitCOA;
                        //journalEntriesLogDebit.CreditChartOfAccount_Id = null;
                        //journalEntriesLogDebit.Description = DebitDescription;
                        //journalEntriesLogDebit.Amount = Math.Abs(paymentApplicationLineItems.AppliedAmount);
                        //journalEntriesLogDebit.JournalEntryStatus_Id = MasterLookup._JournalEntryStatus.Applied;
                        //journalEntriesLogDebit.CreatedBy = _User_Id;
                        //_JournalEntryLogRepository.InsertJournalEntriesLog(PaymentsTransaction, journalEntriesLogDebit);

                        var journalEntryCredit = new JournalEntryLine();
                        journalEntryCredit.Amount = Math.Abs(paymentApplicationLineItems.AppliedAmount);
                        journalEntryCredit.Description = CreditDescription;
                        journalEntryCredit.DetailType = "JournalEntryLineDetail";
                        var JournalEntryLine2 = journalEntryCredit.JournalEntryLineDetail = new JournalEntryLineDetail();
                        JournalEntryLine2.PostingType = "Credit";
                        var AccountRef2 = JournalEntryLine2.AccountRef = new AccountRef();
                        AccountRef2.value = CreditCOAValue;
                        AccountRef2.name = CreditCOAName;
                        updateJournalReq.Line.Add(journalEntryCredit);

                        //JournalEntriesLog journalEntriesLogCredit = new JournalEntriesLog();
                        ////journalEntriesLogCredit.JournalEntry_Id = journalEntries.JournalEntry_Id;
                        //journalEntriesLogCredit.DebitChartOfAccount_Id = null;
                        //journalEntriesLogCredit.CreditChartOfAccount_Id = CreditCOA;
                        //journalEntriesLogCredit.Description = CreditDescription;
                        //journalEntriesLogCredit.Amount = Math.Abs(paymentApplicationLineItems.AppliedAmount);
                        //journalEntriesLogCredit.JournalEntryStatus_Id = MasterLookup._JournalEntryStatus.Applied;
                        //journalEntriesLogCredit.CreatedBy = _User_Id;
                        //_JournalEntryLogRepository.InsertJournalEntriesLog(PaymentsTransaction, journalEntriesLogCredit);
                    }

                }

            }
            if (fault != null && fault.Error != null && fault.Error.Any())
            {
                updateJournalReq.Fault = fault;
                return updateJournalReq;
            }
            return updateJournalReq;
        }
        public CreateJournalReq PrepareCreateJournalReqObj(SqlTransaction PaymentsTransaction, ReadByIdRes readByIdRes, Payments payments, List<PaymentApplicationLineItems> PaymentApplicationLineItemsList, JournalEntries journalEntries)
        {
            CreateJournalReq createJournalReq = new CreateJournalReq();
            var journalList = new List<JournalEntryLine>();
            var ChartOfAccountsLineItemsList = _LookupRepository.GetAllLineItems_ChartOfAccounts(_Service_Id, _Client_Id);
            var PaymentDirectionsCOAList = _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id);
            var PaymentDirectionsCOA = PaymentDirectionsCOAList.FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);
            var PaymentDirectionsCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == PaymentDirectionsCOA?.StandardCOA_Id);
            byte _ChartOfAccountsClassificationType_Id = 0;
            if (PaymentDirectionsCOA == null || PaymentDirectionsCOAItem == null)
            {
                fault = AddError(fault, "Missing COA Configuration(Payment Direction)");
            }
            //createJournalReq.Adjustment = readByIdRes.JournalEntry.Adjustment;
            //createJournalReq.domain = readByIdRes.JournalEntry.domain;
            //createJournalReq.sparse = readByIdRes.JournalEntry.sparse;
            //createJournalReq.Id = readByIdRes.JournalEntry.Id;
            //createJournalReq.SyncToken = readByIdRes.JournalEntry.SyncToken;
            createJournalReq.TxnDate = DateTime.Now.ToString("yyyy-MM-dd");
            string docNumber = CommonUtility.NormalizeDocNumber(readByIdRes.JournalEntry.DocNumber, journalEntries.JournalIndex);
            createJournalReq.DocNumber = docNumber;
            //createJournalReq.CurrencyRef = new Entities.External.QBO.Journal.CurrencyRef()
            //{
            //    value = readByIdRes.JournalEntry.CurrencyRef?.value,
            //    name = readByIdRes.JournalEntry.CurrencyRef?.name
            //};
            //createJournalReq.Line = readByIdRes.JournalEntry.Line;
            if (PaymentApplicationLineItemsList?.Count() > 0)
            {
                foreach (var paymentApplicationLineItems in PaymentApplicationLineItemsList)
                {
                    _AccountingType_Id = paymentApplicationLineItems.AccountingType_Id;
                    if (paymentApplicationLineItems.AccountingType_Id == MasterLookup._AccountingTypes.Cash)
                    {
                        _ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability;
                    }
                    else
                    {
                        _ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.AccountsReceivableAccountsPayable;
                    }
                    string DebitCOAValue = null;
                    string CreditCOAValue = null;
                    string DebitCOAName = null;
                    string CreditCOAName = null;
                    string DebitDescription = null;
                    string CreditDescription = null;
                    int? DebitCOA = null;
                    int? CreditCOA = null;
                    InvoiceDetails invoiceDetails = _InvoiceRepository.GetInvoiceDetailsByInvoiceDetail_Id(paymentApplicationLineItems.InvoiceLineItem_Id);
                    Invoices invoices = _InvoiceRepository.GetInvoicesByInvoice_Id(invoiceDetails.Invoice_Id);
                    PolicyDetails policyDetails = _PolicyDetailsRepository.GetPolicyDetails(invoices.PolicyDetails_Id);
                    if (invoiceDetails != null)
                    {
                        LU_BillingType_LineItems_Directions BillingLineItemsDirections = LookupUtility.GetAllLU_BillingType_LineItems_Directions().FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id);
                        LU_LineItems_Directions LineItemsDirections = LookupUtility.GetAllLineItems_Directions().FirstOrDefault(x => x.LineItems_Directions_Id == BillingLineItemsDirections.LineItems_Directions_Id);
                        LU_LineItems_Clients lineItems_Clients = LookupUtility.GetAllLineItems_Clients().FirstOrDefault(x => x.LineItems_Clients_Id == LineItemsDirections.LineItems_Clients_Id);
                        LU_LineItems LineItems = LookupUtility.GetAllLineItems().FirstOrDefault(c => c.LineItem_Id == lineItems_Clients?.LineItem_Id);
                        LU_LineItemTypes LineType = LookupUtility.GetAllLineItemTypes().FirstOrDefault(c => c.LineItemType_Id == LineItems.LineItemType_Id);
                        LU_LOBs LOB = LookupUtility.GetAllLOBs().FirstOrDefault(x => x.LOB_Id == invoiceDetails.LOB_Id);
                        LOBCarrierGrouping lobcarriergroupingDetails = _LobCarrierGroupingRepository.GetLOBCarrierGroupingDetails(LOB.LOB_Id, policyDetails.CarrierDetail_Id, _Service_Id, _Client_Id);
                        var ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id == _ChartOfAccountsClassificationType_Id);
                        var ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);
                        var errorDetail = BuildErrorDetail(LineItems?.Name, LOB?.Name, policyDetails.BillType);

                        if (ChartOfAccountsLineItems == null || ChartOfAccounts == null)
                        {
                            fault = AddError(fault, "Missing COA Configuration(line-item)", errorDetail);
                            continue;
                        }
                        if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR && paymentApplicationLineItems.AppliedAmount > 0)
                        {
                            DebitCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                            CreditCOAValue = ChartOfAccounts?.ReferenceID;
                            DebitDescription = $"Applying incoming payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made by {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                            CreditDescription = $"Applied to Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                            DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                            CreditCOAName = ChartOfAccounts?.Name;
                            DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                            CreditCOA = ChartOfAccounts?.ChartOfAccounts_Id;
                        }
                        else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && LineItemsDirections.Direction_Id == MasterLookup._Directions.AP && paymentApplicationLineItems.AppliedAmount > 0)
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
                            DebitDescription = $"[RECOVERY] Applying incoming payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made by {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
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
                            CreditDescription = $"[Netting] Applying incoming payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made by {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
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
                            DebitDescription = $"[Netting] Applying incoming payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made by {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
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
                    }
                    var JournalEntryDebit = new JournalEntryLine();

                    JournalEntryDebit.Amount = Math.Abs(paymentApplicationLineItems.AppliedAmount);
                    JournalEntryDebit.Description = DebitDescription;
                    JournalEntryDebit.DetailType = "JournalEntryLineDetail";
                    var JournalEntryLine = JournalEntryDebit.JournalEntryLineDetail = new JournalEntryLineDetail();
                    JournalEntryLine.PostingType = "Debit";
                    var AccountRef = JournalEntryLine.AccountRef = new AccountRef();
                    AccountRef.value = DebitCOAValue;
                    AccountRef.name = DebitCOAName;
                    journalList.Add(JournalEntryDebit);

                    //JournalEntriesLog journalEntriesLogDebit = new JournalEntriesLog();
                    ////journalEntriesLogDebit.JournalEntry_Id = journalEntries.JournalEntry_Id;
                    //journalEntriesLogDebit.DebitChartOfAccount_Id = DebitCOA;
                    //journalEntriesLogDebit.CreditChartOfAccount_Id = null;
                    //journalEntriesLogDebit.Description = DebitDescription;
                    //journalEntriesLogDebit.Amount = Math.Abs(paymentApplicationLineItems.AppliedAmount);
                    //journalEntriesLogDebit.JournalEntryStatus_Id = MasterLookup._JournalEntryStatus.Applied;
                    //journalEntriesLogDebit.CreatedBy = _User_Id;
                    //_JournalEntryLogRepository.InsertJournalEntriesLog(PaymentsTransaction, journalEntriesLogDebit);

                    var journalEntryCredit = new JournalEntryLine();
                    journalEntryCredit.Amount = Math.Abs(paymentApplicationLineItems.AppliedAmount);
                    journalEntryCredit.Description = CreditDescription;
                    journalEntryCredit.DetailType = "JournalEntryLineDetail";
                    var JournalEntryLine2 = journalEntryCredit.JournalEntryLineDetail = new JournalEntryLineDetail();
                    JournalEntryLine2.PostingType = "Credit";
                    var AccountRef2 = JournalEntryLine2.AccountRef = new AccountRef();
                    AccountRef2.value = CreditCOAValue;
                    AccountRef2.name = CreditCOAName;
                    journalList.Add(journalEntryCredit);

                    createJournalReq.Line = journalList;
                    //JournalEntriesLog journalEntriesLogCredit = new JournalEntriesLog();
                    ////journalEntriesLogCredit.JournalEntry_Id = journalEntries.JournalEntry_Id;
                    //journalEntriesLogCredit.DebitChartOfAccount_Id = null;
                    //journalEntriesLogCredit.CreditChartOfAccount_Id = CreditCOA;
                    //journalEntriesLogCredit.Description = CreditDescription;
                    //journalEntriesLogCredit.Amount = Math.Abs(paymentApplicationLineItems.AppliedAmount);
                    //journalEntriesLogCredit.JournalEntryStatus_Id = MasterLookup._JournalEntryStatus.Applied;
                    //journalEntriesLogCredit.CreatedBy = _User_Id;
                    //_JournalEntryLogRepository.InsertJournalEntriesLog(PaymentsTransaction, journalEntriesLogCredit);
                }

            }

            if (fault != null && fault.Error != null && fault.Error.Any())
            {
                createJournalReq.Fault = fault;
                return createJournalReq;
            }
            return createJournalReq;
        }
        public CreateJournalReq PreparePaymentWriteoffUpdateJournalReqObj(SqlTransaction PaymentsTransaction, ReadByIdRes readByIdRes, Payments payments, PaymentApplications paymentApplications, PaymentApplicationLineItems PaymentApplicationLineItems, JournalEntries journalEntries)
        {
            CreateJournalReq createJournalReq = new CreateJournalReq();
            var journalList = new List<JournalEntryLine>();

            var StandardPaymentDirectionsCOAList = _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id);
            var StandardPaymentDirectionsCOA = StandardPaymentDirectionsCOAList.FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);
            var StandardPaymentDirectionsCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == StandardPaymentDirectionsCOA?.StandardCOA_Id);

            var WriteoffPaymentDirectionsCOAList = _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id);
            var WriteoffPaymentDirectionsCOA = WriteoffPaymentDirectionsCOAList.FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);
            var WriteoffPaymentDirectionsCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == WriteoffPaymentDirectionsCOA?.WriteoffCOA_Id);
            if (StandardPaymentDirectionsCOA == null || StandardPaymentDirectionsCOAItem == null)
            {
                fault = AddError(fault, "Missing COA Configuration(Standard Payment Direction");
            }
            if (WriteoffPaymentDirectionsCOA == null || WriteoffPaymentDirectionsCOAItem == null)
            {
                fault = AddError(fault, "Missing COA Configuration(Write-Off Payment Direction");
            }
            if (fault != null && fault.Error != null && fault.Error.Any())
            {
                createJournalReq.Fault = fault;
                return createJournalReq;
            }
            if (readByIdRes != null && readByIdRes.JournalEntry != null)
            {
                createJournalReq.TxnDate = DateTime.Now.ToString("yyyy-MM-dd");
                string docNumber = CommonUtility.NormalizeDocNumber(readByIdRes.JournalEntry.DocNumber, journalEntries.JournalIndex);
                createJournalReq.DocNumber = docNumber;

                if (PaymentApplicationLineItems != null)
                {
                    string DebitCOAValue = null;
                    string CreditCOAValue = null;
                    string DebitCOAName = null;
                    string CreditCOAName = null;
                    string DebitDescription = null;
                    string CreditDescription = null;
                    int? DebitCOA = null;
                    int? CreditCOA = null;

                    if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming)
                    {
                        DebitCOAValue = StandardPaymentDirectionsCOAItem?.ReferenceID;
                        CreditCOAValue = WriteoffPaymentDirectionsCOAItem?.ReferenceID;
                        DebitDescription = $"[Updated] - {paymentApplications.Notes}";
                        CreditDescription = $"[Updated] - {paymentApplications.Notes}";
                        DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                        CreditCOAName = WriteoffPaymentDirectionsCOAItem?.Name;
                        DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                        CreditCOA = WriteoffPaymentDirectionsCOAItem?.ChartOfAccounts_Id;
                    }
                    else
                    {
                        DebitCOAValue = WriteoffPaymentDirectionsCOAItem?.ReferenceID;
                        CreditCOAValue = StandardPaymentDirectionsCOAItem?.ReferenceID;
                        DebitDescription = $"[Updated] - {paymentApplications.Notes}";
                        CreditDescription = $"[Updated] - {paymentApplications.Notes}";
                        DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                        CreditCOAName = StandardPaymentDirectionsCOAItem?.Name;
                        DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                        CreditCOA = StandardPaymentDirectionsCOAItem?.ChartOfAccounts_Id;
                    }

                    var JournalEntryDebit = new JournalEntryLine();
                    JournalEntryDebit.Amount = Math.Abs(PaymentApplicationLineItems.AppliedAmount);
                    JournalEntryDebit.Description = DebitDescription;
                    JournalEntryDebit.DetailType = "JournalEntryLineDetail";
                    var JournalEntryLine = JournalEntryDebit.JournalEntryLineDetail = new JournalEntryLineDetail();
                    JournalEntryLine.PostingType = "Debit";
                    var AccountRef = JournalEntryLine.AccountRef = new AccountRef();
                    AccountRef.value = DebitCOAValue;
                    AccountRef.name = DebitCOAName;
                    journalList.Add(JournalEntryDebit);

                    //JournalEntriesLog journalEntriesLogDebit = new JournalEntriesLog();
                    ////journalEntriesLogDebit.JournalEntry_Id = journalEntries.JournalEntry_Id;
                    //journalEntriesLogDebit.DebitChartOfAccount_Id = DebitCOA;
                    //journalEntriesLogDebit.CreditChartOfAccount_Id = null;
                    //journalEntriesLogDebit.Description = DebitDescription;
                    //journalEntriesLogDebit.Amount = Math.Abs(PaymentApplicationLineItems.AppliedAmount);
                    //journalEntriesLogDebit.JournalEntryStatus_Id = MasterLookup._JournalEntryStatus.Applied;
                    //journalEntriesLogDebit.CreatedBy = _User_Id;
                    //_JournalEntryLogRepository.InsertJournalEntriesLog(PaymentsTransaction, journalEntriesLogDebit);

                    var journalEntryCredit = new JournalEntryLine();
                    journalEntryCredit.Amount = Math.Abs(PaymentApplicationLineItems.AppliedAmount);
                    journalEntryCredit.Description = CreditDescription;
                    journalEntryCredit.DetailType = "JournalEntryLineDetail";
                    var JournalEntryLine2 = journalEntryCredit.JournalEntryLineDetail = new JournalEntryLineDetail();
                    JournalEntryLine2.PostingType = "Credit";
                    var AccountRef2 = JournalEntryLine2.AccountRef = new AccountRef();
                    AccountRef2.value = CreditCOAValue;
                    AccountRef2.name = CreditCOAName;
                    journalList.Add(journalEntryCredit);

                    createJournalReq.Line = journalList;
                    //JournalEntriesLog journalEntriesLogCredit = new JournalEntriesLog();
                    ////journalEntriesLogCredit.JournalEntry_Id = journalEntries.JournalEntry_Id;
                    //journalEntriesLogCredit.DebitChartOfAccount_Id = null;
                    //journalEntriesLogCredit.CreditChartOfAccount_Id = CreditCOA;
                    //journalEntriesLogCredit.Description = CreditDescription;
                    //journalEntriesLogCredit.Amount = Math.Abs(PaymentApplicationLineItems.AppliedAmount);
                    //journalEntriesLogCredit.JournalEntryStatus_Id = MasterLookup._JournalEntryStatus.Applied;
                    //journalEntriesLogCredit.CreatedBy = _User_Id;
                    //_JournalEntryLogRepository.InsertJournalEntriesLog(PaymentsTransaction, journalEntriesLogCredit);
                }
            }

            return createJournalReq;
        }
        public CreateJournalReq PrepareCreatePaymentWriteoffJournalReqObj(SqlTransaction PaymentsTransaction, ReadByIdRes readByIdRes, Payments payments, PaymentApplications paymentApplications, PaymentApplicationLineItems PaymentApplicationLineItems, JournalEntries journalEntries)
        {
            CreateJournalReq createJournalReq = new CreateJournalReq();
            var journalList = new List<JournalEntryLine>();

            var StandardPaymentDirectionsCOAList = _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id);
            var StandardPaymentDirectionsCOA = StandardPaymentDirectionsCOAList.FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);
            var StandardPaymentDirectionsCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == StandardPaymentDirectionsCOA?.StandardCOA_Id);

            var WriteoffPaymentDirectionsCOAList = _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id);
            var WriteoffPaymentDirectionsCOA = WriteoffPaymentDirectionsCOAList.FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);
            var WriteoffPaymentDirectionsCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == WriteoffPaymentDirectionsCOA?.WriteoffCOA_Id);
            if (StandardPaymentDirectionsCOA == null || StandardPaymentDirectionsCOAItem == null)
            {
                fault = AddError(fault, "Missing COA Configuration(Standard Payment Direction");
            }

            if (WriteoffPaymentDirectionsCOA == null || WriteoffPaymentDirectionsCOAItem == null)
                fault = AddError(fault, "Missing COA Configuration(Write-off Payment Direction");
            if (fault != null && fault.Error != null && fault.Error.Any())
            {
                createJournalReq.Fault = fault;
                return createJournalReq;
            }
            if (readByIdRes != null && readByIdRes.JournalEntry != null)
            {
                createJournalReq.TxnDate = DateTime.Now.ToString("yyyy-MM-dd");
                string docNumber = CommonUtility.NormalizeDocNumber(readByIdRes.JournalEntry.DocNumber, journalEntries.JournalIndex);
                createJournalReq.DocNumber = docNumber;

                if (PaymentApplicationLineItems != null)
                {
                    string DebitCOAValue = null;
                    string CreditCOAValue = null;
                    string DebitCOAName = null;
                    string CreditCOAName = null;
                    string DebitDescription = null;
                    string CreditDescription = null;
                    int? DebitCOA = null;
                    int? CreditCOA = null;

                    if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming)
                    {


                        DebitCOAValue = StandardPaymentDirectionsCOAItem?.ReferenceID;
                        CreditCOAValue = WriteoffPaymentDirectionsCOAItem?.ReferenceID;
                        DebitDescription = $"[Added] - {paymentApplications.Notes}";
                        CreditDescription = $"[Added] - {paymentApplications.Notes}";
                        DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id).Name!;
                        CreditCOAName = WriteoffPaymentDirectionsCOAItem?.Name;
                        DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id).ChartOfAccounts_Id!;
                        CreditCOA = WriteoffPaymentDirectionsCOAItem?.ChartOfAccounts_Id;
                    }
                    else
                    {
                        DebitCOAValue = WriteoffPaymentDirectionsCOAItem?.ReferenceID;
                        CreditCOAValue = StandardPaymentDirectionsCOAItem?.ReferenceID;
                        DebitDescription = $"[Added] - {paymentApplications.Notes}";
                        CreditDescription = $"[Added] - {paymentApplications.Notes}";
                        DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                        CreditCOAName = StandardPaymentDirectionsCOAItem?.Name;
                        DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                        CreditCOA = StandardPaymentDirectionsCOAItem?.ChartOfAccounts_Id;
                    }

                    var JournalEntryDebit = new JournalEntryLine();
                    JournalEntryDebit.Amount = Math.Abs(PaymentApplicationLineItems.AppliedAmount);
                    JournalEntryDebit.Description = DebitDescription;
                    JournalEntryDebit.DetailType = "JournalEntryLineDetail";
                    var JournalEntryLine = JournalEntryDebit.JournalEntryLineDetail = new JournalEntryLineDetail();
                    JournalEntryLine.PostingType = "Debit";
                    var AccountRef = JournalEntryLine.AccountRef = new AccountRef();
                    AccountRef.value = DebitCOAValue;
                    AccountRef.name = DebitCOAName;
                    journalList.Add(JournalEntryDebit);

                    var journalEntryCredit = new JournalEntryLine();
                    journalEntryCredit.Amount = Math.Abs(PaymentApplicationLineItems.AppliedAmount);
                    journalEntryCredit.Description = CreditDescription;
                    journalEntryCredit.DetailType = "JournalEntryLineDetail";
                    var JournalEntryLine2 = journalEntryCredit.JournalEntryLineDetail = new JournalEntryLineDetail();
                    JournalEntryLine2.PostingType = "Credit";
                    var AccountRef2 = JournalEntryLine2.AccountRef = new AccountRef();
                    AccountRef2.value = CreditCOAValue;
                    AccountRef2.name = CreditCOAName;
                    journalList.Add(journalEntryCredit);

                    createJournalReq.Line = journalList;
                }
            }

            return createJournalReq;
        }

        public UpdateJournalReq PrepareUnApplyUpdateJournalReqObj(SqlTransaction PaymentsTransaction, PaymentApplicationLineItems paymentApplicationLineItems, JournalEntries journalEntries)
        {
            var ChartOfAccountsLineItemsList = _LookupRepository.GetAllLineItems_ChartOfAccounts(_Service_Id, _Client_Id);
            UpdateJournalReq updateJournalReq = new UpdateJournalReq();
            PaymentApplications paymentApplications = _PaymentsRepository.GetPaymentApplicationsByPaymentApplication_Id(paymentApplicationLineItems.PaymentApplication_Id);
            Payments payments = _PaymentsRepository.GetPaymentDetails(paymentApplications.Payment_Id);
            byte _ChartOfAccountsClassificationType_Id = 0;
            _AccountingType_Id = paymentApplicationLineItems.AccountingType_Id;
            if (paymentApplicationLineItems.AccountingType_Id == MasterLookup._AccountingTypes.Cash)
            {
                _ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability;
            }
            else
            {
                _ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.AccountsReceivableAccountsPayable;
            }

            ReadByIdRes readByIdRes = GetJournalEntryById(journalEntries.ExternalJournalEntryIdentifier);
            var journalList = new List<JournalEntryLine>();
            updateJournalReq.Adjustment = readByIdRes.JournalEntry.Adjustment;
            updateJournalReq.domain = readByIdRes.JournalEntry.domain;
            updateJournalReq.sparse = readByIdRes.JournalEntry.sparse;
            updateJournalReq.Id = readByIdRes.JournalEntry.Id;
            updateJournalReq.SyncToken = readByIdRes.JournalEntry.SyncToken;
            updateJournalReq.TxnDate = readByIdRes.JournalEntry.TxnDate;
            updateJournalReq.DocNumber = readByIdRes.JournalEntry.DocNumber;
            updateJournalReq.CurrencyRef = new Entities.External.QBO.Journal.CurrencyRef()
            {
                value = readByIdRes.JournalEntry.CurrencyRef?.value,
                name = readByIdRes.JournalEntry.CurrencyRef?.name
            };
            updateJournalReq.Line = readByIdRes.JournalEntry.Line;
            if (readByIdRes != null && readByIdRes.JournalEntry != null)
            {

                string DebitCOAValue = null;
                string CreditCOAValue = null;
                string DebitCOAName = null;
                string CreditCOAName = null;
                string DebitDescription = null;
                string CreditDescription = null;
                int? DebitCOA = null;
                int? CreditCOA = null;
                InvoiceDetails invoiceDetails = _InvoiceRepository.GetInvoiceDetailsByInvoiceDetail_Id(paymentApplicationLineItems.InvoiceLineItem_Id);
                Invoices invoices = _InvoiceRepository.GetInvoicesByInvoice_Id(invoiceDetails.Invoice_Id);
                PolicyDetails policyDetails = _PolicyDetailsRepository.GetPolicyDetails(invoices.PolicyDetails_Id);
                if (invoiceDetails != null)
                {
                    LU_BillingType_LineItems_Directions BillingLineItemsDirections = LookupUtility.GetAllLU_BillingType_LineItems_Directions().FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id);
                    LU_LineItems_Directions LineItemsDirections = LookupUtility.GetAllLineItems_Directions().FirstOrDefault(x => x.LineItems_Directions_Id == BillingLineItemsDirections?.LineItems_Directions_Id);
                    LU_LineItems_Clients lineItems_Clients = LookupUtility.GetAllLineItems_Clients().FirstOrDefault(x => x.LineItems_Clients_Id == LineItemsDirections?.LineItems_Clients_Id);
                    LU_LineItems LineItems = LookupUtility.GetAllLineItems().FirstOrDefault(c => c.LineItem_Id == lineItems_Clients?.LineItem_Id);
                    LU_LineItemTypes LineType = LookupUtility.GetAllLineItemTypes().FirstOrDefault(c => c.LineItemType_Id == LineItems?.LineItemType_Id);
                    LU_LOBs LOB = LookupUtility.GetAllLOBs().FirstOrDefault(x => x.LOB_Id == invoiceDetails.LOB_Id);
                    LOBCarrierGrouping lobcarriergroupingDetails = _LobCarrierGroupingRepository.GetLOBCarrierGroupingDetails(LOB.LOB_Id, policyDetails.CarrierDetail_Id, _Service_Id, _Client_Id);
                    var ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id == _ChartOfAccountsClassificationType_Id);
                    var ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);

                    var PaymentDirectionsCOAList = _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id);
                    var PaymentDirectionsCOA = PaymentDirectionsCOAList.FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);
                    var PaymentDirectionsCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == PaymentDirectionsCOA?.StandardCOA_Id);

                    var errorDetail = BuildErrorDetail(LineItems?.Name, LOB?.Name, policyDetails.BillType);
                    if (PaymentDirectionsCOA == null || PaymentDirectionsCOAItem == null)
                    {
                        fault = AddError(fault, "Missing COA Configuration(Standard Payment Direction", errorDetail);
                    }
                    if (ChartOfAccountsLineItems == null || ChartOfAccounts == null)
                        fault = AddError(fault, "Missing COA Configuration(Line-Item", errorDetail);

                    if (fault != null && fault.Error != null && fault.Error.Any())
                    {
                        updateJournalReq.Fault = fault;
                        return updateJournalReq;
                    }
                    bool isPositive = paymentApplicationLineItems.AppliedAmount > 0;
                    bool isAR = LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR;

                    string prefix = payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && isAR && !isPositive ? "[REFUND] " :
                                    payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && !isAR && !isPositive ? "[RECOVERY] " :
                                    !isPositive ? "[Netting] " : "";

                    if (!isAR)
                    {
                        if (lineItems_Clients?.Is_Passthrough == true && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                        {
                            ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id == MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability);
                            ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);
                        }
                    }

                    if (isAR)
                    {
                        DebitCOAValue = isPositive ? ChartOfAccounts?.ReferenceID : PaymentDirectionsCOAItem?.ReferenceID;
                        CreditCOAValue = isPositive ? PaymentDirectionsCOAItem?.ReferenceID : ChartOfAccounts?.ReferenceID;

                        DebitDescription = isPositive
                            ? $"Un Applied from Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}"
                            : $"{prefix}Un Applying incoming payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made by {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";

                        CreditDescription = isPositive
                            ? $"Un Applying incoming payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made by {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}"
                            : $"{prefix}Un Applied to Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    }
                    else
                    {
                        DebitCOAValue = isPositive ? PaymentDirectionsCOAItem?.ReferenceID : ChartOfAccounts?.ReferenceID;
                        CreditCOAValue = isPositive ? ChartOfAccounts?.ReferenceID : PaymentDirectionsCOAItem?.ReferenceID;

                        DebitDescription = isPositive
                            ? $"Un Applying outgoing payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}"
                            : $"{prefix}Un Applied to Expense Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";

                        CreditDescription = isPositive
                            ? $"Un Applied to Expense Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}"
                            : $"{prefix}Un Applying outgoing payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                    }

                    DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                    CreditCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.Name!;

                    DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                    CreditCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                }
                var JournalEntryDebit = new JournalEntryLine();

                JournalEntryDebit.Amount = Math.Abs(paymentApplicationLineItems.AppliedAmount);
                JournalEntryDebit.Description = DebitDescription;
                JournalEntryDebit.DetailType = "JournalEntryLineDetail";
                var JournalEntryLine = JournalEntryDebit.JournalEntryLineDetail = new JournalEntryLineDetail();
                JournalEntryLine.PostingType = "Debit";
                var AccountRef = JournalEntryLine.AccountRef = new AccountRef();
                AccountRef.value = DebitCOAValue;
                AccountRef.name = DebitCOAName;
                updateJournalReq.Line.Add(JournalEntryDebit);

                JournalEntriesLog journalEntriesLogDebit = new JournalEntriesLog();
                //journalEntriesLogDebit.JournalEntry_Id = journalEntries.JournalEntry_Id;
                journalEntriesLogDebit.DebitChartOfAccount_Id = DebitCOA;
                journalEntriesLogDebit.CreditChartOfAccount_Id = null;
                journalEntriesLogDebit.Description = DebitDescription;
                journalEntriesLogDebit.Amount = Math.Abs(paymentApplicationLineItems.AppliedAmount);
                journalEntriesLogDebit.JournalEntryStatus_Id = MasterLookup._JournalEntryStatus.Reversed;
                journalEntriesLogDebit.CreatedBy = _User_Id;
                _JournalEntryLogRepository.InsertJournalEntriesLog(PaymentsTransaction, journalEntriesLogDebit);

                var journalEntryCredit = new JournalEntryLine();
                journalEntryCredit.Amount = Math.Abs(paymentApplicationLineItems.AppliedAmount);
                journalEntryCredit.Description = CreditDescription;
                journalEntryCredit.DetailType = "JournalEntryLineDetail";
                var JournalEntryLine2 = journalEntryCredit.JournalEntryLineDetail = new JournalEntryLineDetail();
                JournalEntryLine2.PostingType = "Credit";
                var AccountRef2 = JournalEntryLine2.AccountRef = new AccountRef();
                AccountRef2.value = CreditCOAValue;
                AccountRef2.name = CreditCOAName;
                updateJournalReq.Line.Add(journalEntryCredit);

                JournalEntriesLog journalEntriesLogCredit = new JournalEntriesLog();
                //journalEntriesLogCredit.JournalEntry_Id = journalEntries.JournalEntry_Id;
                journalEntriesLogCredit.DebitChartOfAccount_Id = null;
                journalEntriesLogCredit.CreditChartOfAccount_Id = CreditCOA;
                journalEntriesLogCredit.Description = CreditDescription;
                journalEntriesLogCredit.Amount = Math.Abs(paymentApplicationLineItems.AppliedAmount);
                journalEntriesLogCredit.JournalEntryStatus_Id = MasterLookup._JournalEntryStatus.Reversed;
                journalEntriesLogCredit.CreatedBy = _User_Id;
                _JournalEntryLogRepository.InsertJournalEntriesLog(PaymentsTransaction, journalEntriesLogCredit);
            }

            return updateJournalReq;
        }
        public CreateJournalReq PrepareUnApplyCreateJournalReqObj(SqlTransaction PaymentsTransaction, PaymentApplicationLineItems paymentApplicationLineItems, JournalEntries journalEntries)
        {
            var ChartOfAccountsLineItemsList = _LookupRepository.GetAllLineItems_ChartOfAccounts(_Service_Id, _Client_Id);
            CreateJournalReq createJournalReq = new CreateJournalReq();
            PaymentApplications paymentApplications = _PaymentsRepository.GetPaymentApplicationsByPaymentApplication_Id(paymentApplicationLineItems.PaymentApplication_Id);
            Payments payments = _PaymentsRepository.GetPaymentDetails(paymentApplications.Payment_Id);
            byte _ChartOfAccountsClassificationType_Id = 0;
            _AccountingType_Id = paymentApplicationLineItems.AccountingType_Id;
            if (paymentApplicationLineItems.AccountingType_Id == MasterLookup._AccountingTypes.Cash)
            {
                _ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability;
            }
            else
            {
                _ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.AccountsReceivableAccountsPayable;
            }

            ReadByIdRes readByIdRes = GetJournalEntryById(journalEntries.ExternalJournalEntryIdentifier);
            var journalList = new List<JournalEntryLine>();
            //createJournalReq.Adjustment = readByIdRes.JournalEntry.Adjustment;
            //createJournalReq.domain = readByIdRes.JournalEntry.domain;
            //createJournalReq.sparse = readByIdRes.JournalEntry.sparse;
            //createJournalReq.Id = readByIdRes.JournalEntry.Id;
            //createJournalReq.SyncToken = readByIdRes.JournalEntry.SyncToken;
            createJournalReq.TxnDate = DateTime.Now.ToString("yyyy-MM-dd");

            string docNumber = CommonUtility.NormalizeDocNumber(readByIdRes.JournalEntry.DocNumber, journalEntries.JournalIndex);
            createJournalReq.DocNumber = docNumber;
            //createJournalReq.CurrencyRef = new Entities.External.QBO.Journal.CurrencyRef()
            //{
            //    value = readByIdRes.JournalEntry.CurrencyRef?.value,
            //    name = readByIdRes.JournalEntry.CurrencyRef?.name
            //};
            //createJournalReq.Line = readByIdRes.JournalEntry.Line;
            if (readByIdRes != null && readByIdRes.JournalEntry != null)
            {

                string DebitCOAValue = null;
                string CreditCOAValue = null;
                string DebitCOAName = null;
                string CreditCOAName = null;
                string DebitDescription = null;
                string CreditDescription = null;
                int? DebitCOA = null;
                int? CreditCOA = null;
                InvoiceDetails invoiceDetails = _InvoiceRepository.GetInvoiceDetailsByInvoiceDetail_Id(paymentApplicationLineItems.InvoiceLineItem_Id);
                Invoices invoices = _InvoiceRepository.GetInvoicesByInvoice_Id(invoiceDetails.Invoice_Id);
                PolicyDetails policyDetails = _PolicyDetailsRepository.GetPolicyDetails(invoices.PolicyDetails_Id);
                if (invoiceDetails != null)
                {
                    LU_BillingType_LineItems_Directions BillingLineItemsDirections = LookupUtility.GetAllLU_BillingType_LineItems_Directions().FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id);
                    LU_LineItems_Directions LineItemsDirections = LookupUtility.GetAllLineItems_Directions().FirstOrDefault(x => x.LineItems_Directions_Id == BillingLineItemsDirections?.LineItems_Directions_Id);
                    LU_LineItems_Clients lineItems_Clients = LookupUtility.GetAllLineItems_Clients().FirstOrDefault(x => x.LineItems_Clients_Id == LineItemsDirections?.LineItems_Clients_Id);
                    LU_LineItems LineItems = LookupUtility.GetAllLineItems().FirstOrDefault(c => c.LineItem_Id == lineItems_Clients?.LineItem_Id);
                    LU_LineItemTypes LineType = LookupUtility.GetAllLineItemTypes().FirstOrDefault(c => c.LineItemType_Id == LineItems?.LineItemType_Id);
                    LU_LOBs LOB = LookupUtility.GetAllLOBs().FirstOrDefault(x => x.LOB_Id == invoiceDetails.LOB_Id);
                    LOBCarrierGrouping lobcarriergroupingDetails = _LobCarrierGroupingRepository.GetLOBCarrierGroupingDetails(LOB.LOB_Id, policyDetails.CarrierDetail_Id, _Service_Id, _Client_Id);
                    var ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id == _ChartOfAccountsClassificationType_Id);
                    var ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);

                    var PaymentDirectionsCOAList = _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id);
                    var PaymentDirectionsCOA = PaymentDirectionsCOAList.FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);
                    var PaymentDirectionsCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == PaymentDirectionsCOA?.StandardCOA_Id);

                    var errorDetail = BuildErrorDetail(PaymentDirectionsCOA.Name, LineItems?.Name, LOB?.Name, policyDetails.BillType);
                    if (PaymentDirectionsCOA == null || PaymentDirectionsCOAItem == null)
                        fault = AddError(fault, "Missing COA Configuration(Standard Payment Direction", errorDetail);

                    if (ChartOfAccountsLineItems == null || ChartOfAccounts == null)
                        fault = AddError(fault, "Missing COA Configuration(Line-Item", errorDetail);
                    if (fault != null && fault.Error != null && fault.Error.Any())
                    {
                        createJournalReq.Fault = fault;
                        return createJournalReq;
                    }
                    if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && LineItemsDirections.Direction_Id == MasterLookup._Directions.AR && paymentApplicationLineItems.AppliedAmount > 0)
                    {
                        CreditCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                        DebitCOAValue = ChartOfAccounts?.ReferenceID;
                        DebitDescription = $"Un Applied from Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                        CreditDescription = $"Un Applying incoming payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made by {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                        DebitCOAName = ChartOfAccounts?.Name;
                        CreditCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.Name!;
                        DebitCOA = ChartOfAccounts.ChartOfAccounts_Id!;
                        CreditCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
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
                        DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                        CreditCOAName = ChartOfAccounts?.Name;
                        DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                        CreditCOA = ChartOfAccounts?.ChartOfAccounts_Id!;
                    }
                    else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && LineItemsDirections.Direction_Id == MasterLookup._Directions.AR && paymentApplicationLineItems.AppliedAmount < 0)
                    {
                        DebitCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                        CreditCOAValue = ChartOfAccounts?.ReferenceID;
                        DebitDescription = $"[REFUND] Un Applying outgoing payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                        CreditDescription = $"[REFUND] Un Applying to Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                        DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                        CreditCOAName = ChartOfAccounts?.Name;
                        DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                        CreditCOA = ChartOfAccounts?.ChartOfAccounts_Id!;
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
                        CreditDescription = $"[RECOVERY] Un Applying incoming payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made by {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                        DebitCOAName = ChartOfAccounts?.Name;
                        CreditCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.Name!;
                        DebitCOA = ChartOfAccounts?.ChartOfAccounts_Id!;
                        CreditCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                    }
                    else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && LineItemsDirections.Direction_Id == MasterLookup._Directions.AR && paymentApplicationLineItems.AppliedAmount < 0)
                    {
                        DebitCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                        CreditCOAValue = ChartOfAccounts?.ReferenceID;
                        DebitDescription = $"[Netting] Un Applying incoming payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made by {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                        CreditDescription = $"[Netting] Un Applied to Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                        DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                        CreditCOAName = ChartOfAccounts?.Name;
                        DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                        CreditCOA = ChartOfAccounts?.ChartOfAccounts_Id!;
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
                        DebitCOAName = ChartOfAccounts?.Name;
                        CreditCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.Name!;
                        DebitCOA = ChartOfAccounts?.ChartOfAccounts_Id!;
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
                        DebitCOAValue = ChartOfAccounts?.ReferenceID;
                        CreditCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                        DebitDescription = $"[Netting] Applied to Expense Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                        CreditDescription = $"[Netting] Applying outgoing payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                        DebitCOAName = ChartOfAccounts?.Name;
                        CreditCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.Name!;
                        DebitCOA = ChartOfAccounts?.ChartOfAccounts_Id;
                        CreditCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                    }

                    else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && LineItemsDirections.Direction_Id == MasterLookup._Directions.AR && paymentApplicationLineItems.AppliedAmount > 0)
                    {
                        CreditCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                        DebitCOAValue = ChartOfAccounts?.ReferenceID;
                        DebitDescription = $"[Netting] Un Applied to Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                        CreditDescription = $"[Netting] Un Applying incoming payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made by {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                        DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                        CreditCOAName = ChartOfAccounts?.Name;
                        DebitCOA = ChartOfAccounts?.ChartOfAccounts_Id!;
                        CreditCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                    }
                }
                var JournalEntryDebit = new JournalEntryLine();

                JournalEntryDebit.Amount = Math.Abs(paymentApplicationLineItems.AppliedAmount);
                JournalEntryDebit.Description = DebitDescription;
                JournalEntryDebit.DetailType = "JournalEntryLineDetail";
                var JournalEntryLine = JournalEntryDebit.JournalEntryLineDetail = new JournalEntryLineDetail();
                JournalEntryLine.PostingType = "Debit";
                var AccountRef = JournalEntryLine.AccountRef = new AccountRef();
                AccountRef.value = DebitCOAValue;
                AccountRef.name = DebitCOAName;
                //createJournalReq.Line.Add(JournalEntryDebit);
                journalList.Add(JournalEntryDebit);
                //JournalEntriesLog journalEntriesLogDebit = new JournalEntriesLog();
                ////journalEntriesLogDebit.JournalEntry_Id = journalEntries.JournalEntry_Id;
                //journalEntriesLogDebit.DebitChartOfAccount_Id = DebitCOA;
                //journalEntriesLogDebit.CreditChartOfAccount_Id = null;
                //journalEntriesLogDebit.Description = DebitDescription;
                //journalEntriesLogDebit.Amount = Math.Abs(paymentApplicationLineItems.AppliedAmount);
                //journalEntriesLogDebit.JournalEntryStatus_Id = MasterLookup._JournalEntryStatus.Reversed;
                //journalEntriesLogDebit.CreatedBy = _User_Id;
                //_JournalEntryLogRepository.InsertJournalEntriesLog(PaymentsTransaction, journalEntriesLogDebit);

                var journalEntryCredit = new JournalEntryLine();
                journalEntryCredit.Amount = Math.Abs(paymentApplicationLineItems.AppliedAmount);
                journalEntryCredit.Description = CreditDescription;
                journalEntryCredit.DetailType = "JournalEntryLineDetail";
                var JournalEntryLine2 = journalEntryCredit.JournalEntryLineDetail = new JournalEntryLineDetail();
                JournalEntryLine2.PostingType = "Credit";
                var AccountRef2 = JournalEntryLine2.AccountRef = new AccountRef();
                AccountRef2.value = CreditCOAValue;
                AccountRef2.name = CreditCOAName;
                journalList.Add(journalEntryCredit);
                createJournalReq.Line = journalList;

                //JournalEntriesLog journalEntriesLogCredit = new JournalEntriesLog();
                ////journalEntriesLogCredit.JournalEntry_Id = journalEntries.JournalEntry_Id;
                //journalEntriesLogCredit.DebitChartOfAccount_Id = null;
                //journalEntriesLogCredit.CreditChartOfAccount_Id = CreditCOA;
                //journalEntriesLogCredit.Description = CreditDescription;
                //journalEntriesLogCredit.Amount = Math.Abs(paymentApplicationLineItems.AppliedAmount);
                //journalEntriesLogCredit.JournalEntryStatus_Id = MasterLookup._JournalEntryStatus.Reversed;
                //journalEntriesLogCredit.CreatedBy = _User_Id;
                //_JournalEntryLogRepository.InsertJournalEntriesLog(PaymentsTransaction, journalEntriesLogCredit);
            }

            return createJournalReq;
        }
        //public UpdateJournalReq PrepareUnApplyPaymentWriteoffUpdateJournalReqObj(SqlTransaction PaymentsTransaction, Payments payments, PaymentApplications paymentApplications, PaymentApplicationLineItems PaymentApplicationLineItems, JournalEntries journalEntries, bool Is_Deleted)
        //{
        //    UpdateJournalReq updateJournalReq = new UpdateJournalReq();
        //    ReadByIdRes readByIdRes = GetJournalEntryById(journalEntries.ExternalJournalEntryIdentifier);
        //    var journalList = new List<JournalEntryLine>();

        //    var StandardPaymentDirectionsCOAList = _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id);
        //    var StandardPaymentDirectionsCOA = StandardPaymentDirectionsCOAList.FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);
        //    var StandardPaymentDirectionsCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == StandardPaymentDirectionsCOA?.StandardCOA_Id);

        //    var WriteoffPaymentDirectionsCOAList = _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id);
        //    var WriteoffPaymentDirectionsCOA = WriteoffPaymentDirectionsCOAList.FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);
        //    var WriteoffPaymentDirectionsCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == WriteoffPaymentDirectionsCOA?.WriteoffCOA_Id);

        //    if (readByIdRes != null && readByIdRes.JournalEntry != null)
        //    {
        //        updateJournalReq.Adjustment = readByIdRes.JournalEntry.Adjustment;
        //        updateJournalReq.domain = readByIdRes.JournalEntry.domain;
        //        updateJournalReq.sparse = readByIdRes.JournalEntry.sparse;
        //        updateJournalReq.Id = readByIdRes.JournalEntry.Id;
        //        updateJournalReq.SyncToken = readByIdRes.JournalEntry.SyncToken;
        //        updateJournalReq.TxnDate = readByIdRes.JournalEntry.TxnDate;
        //        updateJournalReq.DocNumber = readByIdRes.JournalEntry.DocNumber;
        //        updateJournalReq.CurrencyRef = new Entities.External.QBO.Journal.CurrencyRef()
        //        {
        //            value = readByIdRes.JournalEntry.CurrencyRef?.value,
        //            name = readByIdRes.JournalEntry.CurrencyRef?.name
        //        };
        //        updateJournalReq.Line = readByIdRes.JournalEntry.Line;
        //        if (PaymentApplicationLineItems != null)
        //        {
        //            string DebitCOAValue = null;
        //            string CreditCOAValue = null;
        //            string DebitCOAName = null;
        //            string CreditCOAName = null;
        //            string DebitDescription = null;
        //            string CreditDescription = null;
        //            int? DebitCOA = null;
        //            int? CreditCOA = null;

        //            if (Is_Deleted)
        //            {
        //                if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming)
        //                {
        //                    DebitCOAValue = WriteoffPaymentDirectionsCOAItem.ReferenceID;
        //                    CreditCOAValue = StandardPaymentDirectionsCOAItem.ReferenceID;
        //                    DebitDescription = $"[Deleted] - {paymentApplications.Notes}";
        //                    CreditDescription = $"[Deleted] - {paymentApplications.Notes}";
        //                    DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
        //                    CreditCOAName = StandardPaymentDirectionsCOAItem.Name;
        //                    DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
        //                    CreditCOA = StandardPaymentDirectionsCOAItem.ChartOfAccounts_Id;
        //                }
        //                else
        //                {
        //                    DebitCOAValue = StandardPaymentDirectionsCOAItem.ReferenceID;
        //                    CreditCOAValue = WriteoffPaymentDirectionsCOAItem.ReferenceID;
        //                    DebitDescription = $"[Deleted] - {paymentApplications.Notes}";
        //                    CreditDescription = $"[Deleted] - {paymentApplications.Notes}";
        //                    DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
        //                    CreditCOAName = WriteoffPaymentDirectionsCOAItem.Name;
        //                    DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
        //                    CreditCOA = WriteoffPaymentDirectionsCOAItem.ChartOfAccounts_Id;
        //                }
        //            }
        //            else
        //            {
        //                if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming)
        //                {
        //                    DebitCOAValue = WriteoffPaymentDirectionsCOAItem.ReferenceID;
        //                    CreditCOAValue = StandardPaymentDirectionsCOAItem.ReferenceID;
        //                    DebitDescription = $"[Unapply] - {paymentApplications.Notes}";
        //                    CreditDescription = $"[Unapply] - {paymentApplications.Notes}";
        //                    DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
        //                    CreditCOAName = StandardPaymentDirectionsCOAItem.Name;
        //                    DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
        //                    CreditCOA = StandardPaymentDirectionsCOAItem.ChartOfAccounts_Id;
        //                }
        //                else
        //                {
        //                    DebitCOAValue = StandardPaymentDirectionsCOAItem.ReferenceID;
        //                    CreditCOAValue = WriteoffPaymentDirectionsCOAItem.ReferenceID;
        //                    DebitDescription = $"[Unapply] - {paymentApplications.Notes}";
        //                    CreditDescription = $"[Unapply] - {paymentApplications.Notes}";
        //                    DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
        //                    CreditCOAName = WriteoffPaymentDirectionsCOAItem.Name;
        //                    DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
        //                    CreditCOA = WriteoffPaymentDirectionsCOAItem.ChartOfAccounts_Id;
        //                }
        //            }

        //            var JournalEntryDebit = new JournalEntryLine();
        //            JournalEntryDebit.Amount = Math.Abs(PaymentApplicationLineItems.AppliedAmount);
        //            JournalEntryDebit.Description = DebitDescription;
        //            JournalEntryDebit.DetailType = "JournalEntryLineDetail";
        //            var JournalEntryLine = JournalEntryDebit.JournalEntryLineDetail = new JournalEntryLineDetail();
        //            JournalEntryLine.PostingType = "Debit";
        //            var AccountRef = JournalEntryLine.AccountRef = new AccountRef();
        //            AccountRef.value = DebitCOAValue;
        //            AccountRef.name = DebitCOAName;
        //            updateJournalReq.Line.Add(JournalEntryDebit);

        //            JournalEntriesLog journalEntriesLogDebit = new JournalEntriesLog();
        //            journalEntriesLogDebit.JournalEntry_Id = journalEntries.JournalEntry_Id;
        //            journalEntriesLogDebit.DebitChartOfAccount_Id = DebitCOA;
        //            journalEntriesLogDebit.CreditChartOfAccount_Id = null;
        //            journalEntriesLogDebit.Description = DebitDescription;
        //            journalEntriesLogDebit.Amount = Math.Abs(PaymentApplicationLineItems.AppliedAmount);
        //            journalEntriesLogDebit.JournalEntryStatus_Id = MasterLookup._JournalEntryStatus.Reversed;
        //            journalEntriesLogDebit.CreatedBy = _User_Id;
        //            _JournalEntryLogRepository.InsertJournalEntriesLog(PaymentsTransaction, journalEntriesLogDebit);

        //            var journalEntryCredit = new JournalEntryLine();
        //            journalEntryCredit.Amount = Math.Abs(PaymentApplicationLineItems.AppliedAmount);
        //            journalEntryCredit.Description = CreditDescription;
        //            journalEntryCredit.DetailType = "JournalEntryLineDetail";
        //            var JournalEntryLine2 = journalEntryCredit.JournalEntryLineDetail = new JournalEntryLineDetail();
        //            JournalEntryLine2.PostingType = "Credit";
        //            var AccountRef2 = JournalEntryLine2.AccountRef = new AccountRef();
        //            AccountRef2.value = CreditCOAValue;
        //            AccountRef2.name = CreditCOAName;
        //            updateJournalReq.Line.Add(journalEntryCredit);

        //            JournalEntriesLog journalEntriesLogCredit = new JournalEntriesLog();
        //            journalEntriesLogCredit.JournalEntry_Id = journalEntries.JournalEntry_Id;
        //            journalEntriesLogCredit.DebitChartOfAccount_Id = null;
        //            journalEntriesLogCredit.CreditChartOfAccount_Id = CreditCOA;
        //            journalEntriesLogCredit.Description = CreditDescription;
        //            journalEntriesLogCredit.Amount = Math.Abs(PaymentApplicationLineItems.AppliedAmount);
        //            journalEntriesLogCredit.JournalEntryStatus_Id = MasterLookup._JournalEntryStatus.Reversed;
        //            journalEntriesLogCredit.CreatedBy = _User_Id;
        //            _JournalEntryLogRepository.InsertJournalEntriesLog(PaymentsTransaction, journalEntriesLogCredit);
        //        }
        //    }
        //    return updateJournalReq;
        //}
        public CreateJournalReq PrepareUnApplyPaymentWriteoffUpdateJournalReqObj(SqlTransaction PaymentsTransaction, Payments payments, PaymentApplications paymentApplications, PaymentApplicationLineItems PaymentApplicationLineItems, JournalEntries journalEntries, bool Is_Deleted)
        {
            CreateJournalReq createJournalReq = new CreateJournalReq();
            ReadByIdRes readByIdRes = GetJournalEntryById(journalEntries.ExternalJournalEntryIdentifier);
            var journalList = new List<JournalEntryLine>();

            var StandardPaymentDirectionsCOAList = _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id);
            var StandardPaymentDirectionsCOA = StandardPaymentDirectionsCOAList.FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);
            var StandardPaymentDirectionsCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == StandardPaymentDirectionsCOA?.StandardCOA_Id);

            var WriteoffPaymentDirectionsCOAList = _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id);
            var WriteoffPaymentDirectionsCOA = WriteoffPaymentDirectionsCOAList.FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);
            var WriteoffPaymentDirectionsCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == WriteoffPaymentDirectionsCOA?.WriteoffCOA_Id);
            if (WriteoffPaymentDirectionsCOA == null || WriteoffPaymentDirectionsCOAItem == null)
            {
                fault = AddError(fault, "Missing COA Configuration(Write off Payment Direction");
            }
            if (StandardPaymentDirectionsCOA == null || StandardPaymentDirectionsCOAItem == null)
                fault = AddError(fault, "Missing COA Configuration(Standard Payment Direction");
            if (fault != null && fault.Error != null && fault.Error.Any())
            {
                createJournalReq.Fault = fault;
                return createJournalReq;
            }
            if (readByIdRes != null && readByIdRes.JournalEntry != null)
            {
                createJournalReq.TxnDate = readByIdRes.JournalEntry.TxnDate;
                createJournalReq.DocNumber = CommonUtility.NormalizeDocNumber(readByIdRes.JournalEntry.DocNumber, journalEntries.JournalIndex);

                if (PaymentApplicationLineItems != null)
                {
                    string DebitCOAValue = null;
                    string CreditCOAValue = null;
                    string DebitCOAName = null;
                    string CreditCOAName = null;
                    string DebitDescription = null;
                    string CreditDescription = null;
                    int? DebitCOA = null;
                    int? CreditCOA = null;

                    if (Is_Deleted)
                    {
                        if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming)
                        {
                            DebitCOAValue = WriteoffPaymentDirectionsCOAItem?.ReferenceID;
                            CreditCOAValue = StandardPaymentDirectionsCOAItem?.ReferenceID;
                            DebitDescription = $"[Deleted] - {paymentApplications.Notes}";
                            CreditDescription = $"[Deleted] - {paymentApplications.Notes}";
                            DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                            CreditCOAName = StandardPaymentDirectionsCOAItem?.Name;
                            DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                            CreditCOA = StandardPaymentDirectionsCOAItem?.ChartOfAccounts_Id;
                        }
                        else
                        {
                            DebitCOAValue = StandardPaymentDirectionsCOAItem?.ReferenceID;
                            CreditCOAValue = WriteoffPaymentDirectionsCOAItem?.ReferenceID;
                            DebitDescription = $"[Deleted] - {paymentApplications.Notes}";
                            CreditDescription = $"[Deleted] - {paymentApplications.Notes}";
                            DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                            CreditCOAName = WriteoffPaymentDirectionsCOAItem?.Name;
                            DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                            CreditCOA = WriteoffPaymentDirectionsCOAItem?.ChartOfAccounts_Id;
                        }
                    }
                    else
                    {
                        if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming)
                        {
                            DebitCOAValue = WriteoffPaymentDirectionsCOAItem?.ReferenceID;
                            CreditCOAValue = StandardPaymentDirectionsCOAItem?.ReferenceID;
                            DebitDescription = $"[Unapply] - {paymentApplications.Notes}";
                            CreditDescription = $"[Unapply] - {paymentApplications.Notes}";
                            DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                            CreditCOAName = StandardPaymentDirectionsCOAItem?.Name;
                            DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                            CreditCOA = StandardPaymentDirectionsCOAItem?.ChartOfAccounts_Id;
                        }
                        else
                        {
                            DebitCOAValue = StandardPaymentDirectionsCOAItem?.ReferenceID;
                            CreditCOAValue = WriteoffPaymentDirectionsCOAItem?.ReferenceID;
                            DebitDescription = $"[Unapply] - {paymentApplications.Notes}";
                            CreditDescription = $"[Unapply] - {paymentApplications.Notes}";
                            DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                            CreditCOAName = WriteoffPaymentDirectionsCOAItem?.Name;
                            DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                            CreditCOA = WriteoffPaymentDirectionsCOAItem?.ChartOfAccounts_Id;
                        }
                    }

                    var JournalEntryDebit = new JournalEntryLine();
                    JournalEntryDebit.Amount = Math.Abs(PaymentApplicationLineItems.AppliedAmount);
                    JournalEntryDebit.Description = DebitDescription;
                    JournalEntryDebit.DetailType = "JournalEntryLineDetail";
                    var JournalEntryLine = JournalEntryDebit.JournalEntryLineDetail = new JournalEntryLineDetail();
                    JournalEntryLine.PostingType = "Debit";
                    var AccountRef = JournalEntryLine.AccountRef = new AccountRef();
                    AccountRef.value = DebitCOAValue;
                    AccountRef.name = DebitCOAName;
                    journalList.Add(JournalEntryDebit);

                    //JournalEntriesLog journalEntriesLogDebit = new JournalEntriesLog();
                    ////journalEntriesLogDebit.JournalEntry_Id = journalEntries.JournalEntry_Id;
                    //journalEntriesLogDebit.DebitChartOfAccount_Id = DebitCOA;
                    //journalEntriesLogDebit.CreditChartOfAccount_Id = null;
                    //journalEntriesLogDebit.Description = DebitDescription;
                    //journalEntriesLogDebit.Amount = Math.Abs(PaymentApplicationLineItems.AppliedAmount);
                    //journalEntriesLogDebit.JournalEntryStatus_Id = MasterLookup._JournalEntryStatus.Reversed;
                    //journalEntriesLogDebit.CreatedBy = _User_Id;
                    //_JournalEntryLogRepository.InsertJournalEntriesLog(PaymentsTransaction, journalEntriesLogDebit);

                    var journalEntryCredit = new JournalEntryLine();
                    journalEntryCredit.Amount = Math.Abs(PaymentApplicationLineItems.AppliedAmount);
                    journalEntryCredit.Description = CreditDescription;
                    journalEntryCredit.DetailType = "JournalEntryLineDetail";
                    var JournalEntryLine2 = journalEntryCredit.JournalEntryLineDetail = new JournalEntryLineDetail();
                    JournalEntryLine2.PostingType = "Credit";
                    var AccountRef2 = JournalEntryLine2.AccountRef = new AccountRef();
                    AccountRef2.value = CreditCOAValue;
                    AccountRef2.name = CreditCOAName;
                    journalList.Add(journalEntryCredit);

                    //JournalEntriesLog journalEntriesLogCredit = new JournalEntriesLog();
                    ////journalEntriesLogCredit.JournalEntry_Id = journalEntries.JournalEntry_Id;
                    //journalEntriesLogCredit.DebitChartOfAccount_Id = null;
                    //journalEntriesLogCredit.CreditChartOfAccount_Id = CreditCOA;
                    //journalEntriesLogCredit.Description = CreditDescription;
                    //journalEntriesLogCredit.Amount = Math.Abs(PaymentApplicationLineItems.AppliedAmount);
                    //journalEntriesLogCredit.JournalEntryStatus_Id = MasterLookup._JournalEntryStatus.Reversed;
                    //journalEntriesLogCredit.CreatedBy = _User_Id;
                    //_JournalEntryLogRepository.InsertJournalEntriesLog(PaymentsTransaction, journalEntriesLogCredit);

                    createJournalReq.Line = journalList;
                }
            }

            return createJournalReq;
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
        public void InsertJournalEntriesLog(SqlTransaction Transaction, List<JournalEntryLine> JournalEntryLines, JournalEntries journalEntries)
        {
            string DebitCOAValue = null;
            string CreditCOAValue = null;
            string DebitCOAName = null;
            string CreditCOAName = null;
            string DebitDescription = null;
            string CreditDescription = null;
            int? DebitCOA = null;
            int? CreditCOA = null;
            double Amount = 0;

            if (JournalEntryLines?.Count() > 0)
            {
                foreach (var item in JournalEntryLines)
                {
                    if (item.JournalEntryLineDetail.PostingType == "Debit")
                    {
                        DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == item.JournalEntryLineDetail.AccountRef.value && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id;
                        Amount = item.Amount;
                        JournalEntriesLog JournalEntriesLogDebit = new JournalEntriesLog();
                        JournalEntriesLogDebit.JournalEntry_Id = journalEntries.JournalEntry_Id;
                        JournalEntriesLogDebit.DebitChartOfAccount_Id = DebitCOA;
                        JournalEntriesLogDebit.CreditChartOfAccount_Id = null;
                        JournalEntriesLogDebit.Description = item.Description;
                        JournalEntriesLogDebit.Amount = Amount;
                        JournalEntriesLogDebit.JournalEntryStatus_Id = MasterLookup._JournalEntryStatus.Created;
                        JournalEntriesLogDebit.CreatedBy = _User_Id;
                        _JournalEntryLogRepository.InsertJournalEntriesLog(Transaction, JournalEntriesLogDebit);
                    }
                    else if (item.JournalEntryLineDetail.PostingType == "Credit")
                    {
                        CreditCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == item.JournalEntryLineDetail.AccountRef.value && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id;
                        JournalEntriesLog JournalEntriesLogCredit = new JournalEntriesLog();
                        JournalEntriesLogCredit.JournalEntry_Id = journalEntries.JournalEntry_Id;
                        JournalEntriesLogCredit.DebitChartOfAccount_Id = null;
                        JournalEntriesLogCredit.CreditChartOfAccount_Id = CreditCOA;
                        JournalEntriesLogCredit.Description = item.Description;
                        JournalEntriesLogCredit.Amount = Amount;
                        JournalEntriesLogCredit.JournalEntryStatus_Id = MasterLookup._JournalEntryStatus.Created;
                        JournalEntriesLogCredit.CreatedBy = _User_Id;
                        _JournalEntryLogRepository.InsertJournalEntriesLog(Transaction, JournalEntriesLogCredit);
                    }
                }
            }
        }
        public Entities.External.QBO.Vendor.CreateVendorRes CreateVendor(byte EntityType_Id, long Entity_Id, string EntityName, long Client_Id)
        {
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.CreateVendor
            };
            Entities.External.QBO.Vendor.CreateVendorReq createVendorReq = new Entities.External.QBO.Vendor.CreateVendorReq();
            Entities.External.QBO.Vendor.CreateVendorRes createVendorRes = new Entities.External.QBO.Vendor.CreateVendorRes();
            try
            {
                VendorUtility vendorUtility = new VendorUtility();
                createVendorReq = vendorUtility.PrepareCreateVendorReqObject(EntityType_Id, Entity_Id, EntityName, Client_Id);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error while preparing the create vendor request object.\n" + ex.Message, ex);
            }
            string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/vendor?minorversion={_QBOParameters.MinorVersion}";
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            if (_Token == null)
                _Token = RefreshAccessToken();

            Headers.Add("Authorization", "Bearer " + _Token.AccessToken);
            try
            {
                createVendorRes = (Entities.External.QBO.Vendor.CreateVendorRes)HttpServiceUtility.ExecuteJsonPostRequest(_ServiceLogInfo, Service_Url, createVendorReq, typeof(Entities.External.QBO.Vendor.CreateVendorRes), false, Headers);
            }
            catch (Exception ex)
            {
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }
            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);

            return createVendorRes;
        }
        public Entities.External.QBO.Vendor.ReadByIdRes ReadVendorById(string QBORefId)
        {
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.ReadVendorById
            };
            Entities.External.QBO.Vendor.ReadByIdRes readByIdRes = new Entities.External.QBO.Vendor.ReadByIdRes();
            string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/vendor/{QBORefId}";
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            if (_Token == null)
                _Token = RefreshAccessToken();

            Headers.Add("Authorization", "Bearer " + _Token.AccessToken);
            try
            {
                readByIdRes = (Entities.External.QBO.Vendor.ReadByIdRes)HttpServiceUtility.ExecuteJsonGetRequest(_ServiceLogInfo, Service_Url, typeof(Entities.External.QBO.Vendor.ReadByIdRes), false, Headers);
            }
            catch (Exception ex)
            {
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }
            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);

            return readByIdRes;
        }
        public Entities.External.QBO.Vendor.ReadByQueryRes ReadVendorByDisplayName(string name)
        {
            var query = $"SELECT * FROM Vendor WHERE DisplayName = '{name}'";
            var encodedQuery = Uri.EscapeDataString(query);
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.ReadVendorById
            };
            Entities.External.QBO.Vendor.ReadByQueryRes readByIdRes = new Entities.External.QBO.Vendor.ReadByQueryRes();
            string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/query?query={encodedQuery}";
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            if (_Token == null)
                _Token = RefreshAccessToken();

            Headers.Add("Authorization", "Bearer " + _Token.AccessToken);
            try
            {
                readByIdRes = (Entities.External.QBO.Vendor.ReadByQueryRes)HttpServiceUtility.ExecuteJsonGetRequest(_ServiceLogInfo, Service_Url, typeof(Entities.External.QBO.Vendor.ReadByQueryRes), false, Headers);
            }
            catch (Exception ex)
            {
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }
            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);

            return readByIdRes;
        }
        public Entities.External.QBO.Vendor.UpdateVendorRes UpdateVendor(string QBOVendorRef_Id, byte EntityType_Id, long Entity_Id, string EntityName, long Client_Id)
        {
            Entities.External.QBO.Vendor.ReadByIdRes readByIdRes = ReadVendorById(QBOVendorRef_Id);
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.UpdateVendor
            };
            Entities.External.QBO.Vendor.UpdateVendorReq updateVendorReq = new Entities.External.QBO.Vendor.UpdateVendorReq();
            Entities.External.QBO.Vendor.UpdateVendorRes updateVendorRes = new Entities.External.QBO.Vendor.UpdateVendorRes();
            try
            {
                VendorUtility vendorUtility = new VendorUtility();
                updateVendorReq = vendorUtility.PrepareUpdateVendorReqObject(readByIdRes, EntityType_Id, Entity_Id, EntityName, Client_Id);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error while preparing the 'update' vendor request object.\n" + ex.Message, ex);
            }
            string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/vendor?minorversion={_QBOParameters.MinorVersion}";
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            if (_Token == null)
                _Token = RefreshAccessToken();

            Headers.Add("Authorization", "Bearer " + _Token.AccessToken);
            try
            {
                updateVendorRes = (Entities.External.QBO.Vendor.UpdateVendorRes)HttpServiceUtility.ExecuteJsonPostRequest(_ServiceLogInfo, Service_Url, updateVendorReq, typeof(Entities.External.QBO.Vendor.UpdateVendorRes), false, Headers);
            }
            catch (Exception ex)
            {
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }
            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);

            return updateVendorRes;
        }
        public Entities.External.QBO.Bill.CreateBillRes CreateBill(Payments payments, string ExternalVendorIdentifier, byte Direction_Id)
        {
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.CreateBill
            };
            Entities.External.QBO.Bill.CreateBillReq createBillReq = new Entities.External.QBO.Bill.CreateBillReq();
            Entities.External.QBO.Bill.CreateBillRes createBillRes = new Entities.External.QBO.Bill.CreateBillRes();
            try
            {
                BillUtility billUtility = new BillUtility(_Service_Id, _Client_Id);
                createBillReq = billUtility.PrepareCreateBillReqObject(payments, ExternalVendorIdentifier, Direction_Id);
                decimal billAmount = Math.Round(createBillReq?.Line?.Sum(x => x.Amount) ?? 0m, 2);
                decimal paymentAmount = Math.Round((decimal)payments.Amount, 2);
                if (billAmount != paymentAmount)
                {
                    throw new ApplicationException($"Payment amount doesn't match the bill amount. BillAmount:{billAmount} PaymentAmount:{paymentAmount}");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error while preparing the 'create' bill request object.\n" + ex.Message, ex);
            }
            string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/bill?minorversion={_QBOParameters.MinorVersion}";
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            if (_Token == null)
                _Token = RefreshAccessToken();

            Headers.Add("Authorization", "Bearer " + _Token.AccessToken);
            try
            {
                createBillRes = (Entities.External.QBO.Bill.CreateBillRes)HttpServiceUtility.ExecuteJsonPostRequest(_ServiceLogInfo, Service_Url, createBillReq, typeof(Entities.External.QBO.Bill.CreateBillRes), false, Headers);
            }
            catch (Exception ex)
            {
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }
            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);

            return createBillRes;
        }
        public Entities.External.QBO.Bill.ReadByIdRes ReadBillById(string QBORefId)
        {
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.ReadBillById
            };
            Entities.External.QBO.Bill.ReadByIdRes readByIdRes = new Entities.External.QBO.Bill.ReadByIdRes();
            string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/bill/{QBORefId}";
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            if (_Token == null)
                _Token = RefreshAccessToken();

            Headers.Add("Authorization", "Bearer " + _Token.AccessToken);
            try
            {
                readByIdRes = (Entities.External.QBO.Bill.ReadByIdRes)HttpServiceUtility.ExecuteJsonGetRequest(_ServiceLogInfo, Service_Url, typeof(Entities.External.QBO.Bill.ReadByIdRes), false, Headers);
            }
            catch (Exception ex)
            {
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }
            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);
            return readByIdRes;
        }
        public Entities.External.QBO.Bill.ReadByIdRes DeleteBill(Entities.External.QBO.Bill.ReadByIdRes readByIdRes)
        {
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.DeleteBill
            };
            Entities.External.QBO.Bill.DeleteBillReq deleteBillReq = new Entities.External.QBO.Bill.DeleteBillReq();
            Entities.External.QBO.Bill.DeleteBillRes deleteBillRes = new Entities.External.QBO.Bill.DeleteBillRes();
            deleteBillReq.Id = readByIdRes.Bill.Id;
            deleteBillReq.SyncToken = readByIdRes.Bill.SyncToken;
            string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/bill?operation=delete";
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            if (_Token == null)
                _Token = RefreshAccessToken();

            Headers.Add("Authorization", "Bearer " + _Token.AccessToken);
            try
            {
                deleteBillRes = (Entities.External.QBO.Bill.DeleteBillRes)HttpServiceUtility.ExecuteJsonPostRequest(_ServiceLogInfo, Service_Url, deleteBillReq, typeof(Entities.External.QBO.Bill.DeleteBillRes), false, Headers);
            }
            catch (Exception ex)
            {
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }
            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);
            return readByIdRes;
        }
        public CreateJournalRes UpdatePaymentJournalEntry(SqlTransaction PaymentsTransaction, Payments payments, JournalEntries journalEntries)
        {
            ReadByIdRes readByIdRes = GetJournalEntryById(journalEntries.ExternalJournalEntryIdentifier);

            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.UpdateJournalEntry
            };

            string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/journalentry?minorversion={_QBOParameters.MinorVersion}";

            Dictionary<string, string> Headers = new Dictionary<string, string>();

            if (_Token == null)
                _Token = RefreshAccessToken();

            Headers.Add("Authorization", "Bearer " + _Token.AccessToken);

            CreateJournalRes updateJournalResponse = new CreateJournalRes();

            if (readByIdRes != null)
            {
                UpdateJournalReq updateJournalRequest = new UpdateJournalReq();

                try
                {
                    updateJournalRequest = PrepareUpdatePaymentJournalReqObj(payments, readByIdRes);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Error while preparing the payment journal entry request.\n" + ex.Message, ex);
                }

                try
                {
                    updateJournalResponse = (CreateJournalRes)HttpServiceUtility.ExecuteJsonPostRequest(_ServiceLogInfo, Service_Url, updateJournalRequest, typeof(CreateJournalRes), false, Headers);

                    if (!string.IsNullOrEmpty(updateJournalResponse?.JournalEntry?.Id))
                    {
                        if (journalEntries != null)
                        {
                            _JournalEntryLogRepository.InactiveJournalEntryLogsByJournalEntry_Id(PaymentsTransaction, journalEntries.JournalEntry_Id);
                            InsertJournalEntriesLog(PaymentsTransaction, updateJournalResponse?.JournalEntry?.Line, journalEntries);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _ServiceLogInfo.Is_Success = false;
                    _ServiceLogInfo.Message = ex.Message;
                }

                _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);
            }

            return updateJournalResponse;
        }
        public UpdateJournalReq PrepareUpdatePaymentJournalReqObj(Payments payments, ReadByIdRes readByIdRes)
        {
            UpdateJournalReq updateJournalReq = new UpdateJournalReq();
            var journalList = new List<JournalEntryLine>();

            if (readByIdRes != null && readByIdRes.JournalEntry != null)
            {
                updateJournalReq.Adjustment = readByIdRes.JournalEntry.Adjustment;
                updateJournalReq.domain = readByIdRes.JournalEntry.domain;
                updateJournalReq.sparse = readByIdRes.JournalEntry.sparse;
                updateJournalReq.Id = readByIdRes.JournalEntry.Id;
                updateJournalReq.SyncToken = readByIdRes.JournalEntry.SyncToken;
                updateJournalReq.TxnDate = payments?.PaymentDate?.ToString("yyyy-MM-dd");
                updateJournalReq.DocNumber = readByIdRes.JournalEntry.DocNumber;

                updateJournalReq.CurrencyRef = new Entities.External.QBO.Journal.CurrencyRef()
                {
                    value = readByIdRes.JournalEntry.CurrencyRef?.value,
                    name = readByIdRes.JournalEntry.CurrencyRef?.name
                };

                string DebitCOAValue = null;
                string CreditCOAValue = null;
                string DebitCOAName = null;
                string CreditCOAName = null;
                string DebitDescription = null;
                string CreditDescription = null;

                var OperatingBanksCOAList = _LookupRepository.GetAllOperatingBanksChartOfAccounts(_Service_Id, _Client_Id);
                var OperatingBankCOA = OperatingBanksCOAList.FirstOrDefault(x => x.OperatingBank_Id == payments.OperatingBank_Id);
                var OperatingBankCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == OperatingBankCOA?.ChartOfAccounts_Id);

                var PaymentDirectionsCOAList = _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id);
                var PaymentDirectionsCOA = PaymentDirectionsCOAList.FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);
                var PaymentDirectionsCOAItem = LookupUtility.GetAllChartofAccounts()
                    .FirstOrDefault(c => c.ChartOfAccounts_Id == PaymentDirectionsCOA?.StandardCOA_Id);
                if (OperatingBankCOA == null || OperatingBankCOAItem == null)
                {
                    fault = AddError(fault, "Missing COA Configuration(Operating Bank");
                }
                if (PaymentDirectionsCOA == null || PaymentDirectionsCOAItem == null)
                    fault = AddError(fault, "Missing COA Configuration(Standard Payment Direction");
                if (fault != null && fault.Error != null && fault?.Error?.Any() == true)
                {
                    updateJournalReq.Fault = fault;
                    return updateJournalReq;
                }
                if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming)
                {
                    DebitCOAValue = OperatingBankCOAItem.ReferenceID;
                    CreditCOAValue = PaymentDirectionsCOAItem.ReferenceID;

                    DebitDescription = $"Record incoming payment - reference number: {payments.ReferenceNumber}, payment made by {payments.EntityName}";
                    CreditDescription = $"Applied to Incoming Payments Account";
                }
                else
                {
                    DebitCOAValue = PaymentDirectionsCOAItem.ReferenceID;
                    CreditCOAValue = OperatingBankCOAItem.ReferenceID;

                    DebitDescription = $"Applied to Outgoing Payments Account";
                    CreditDescription = $"Record outgoing payment - reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName}";
                }

                DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name;

                CreditCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.Name;

                var debitLine = new JournalEntryLine();
                debitLine.Amount = payments.Amount;
                debitLine.Description = DebitDescription;
                debitLine.DetailType = "JournalEntryLineDetail";

                var debitDetail = debitLine.JournalEntryLineDetail = new JournalEntryLineDetail();
                debitDetail.PostingType = "Debit";

                debitDetail.AccountRef = new AccountRef()
                {
                    value = DebitCOAValue,
                    name = DebitCOAName
                };

                journalList.Add(debitLine);

                var creditLine = new JournalEntryLine();
                creditLine.Amount = payments.Amount;
                creditLine.Description = CreditDescription;
                creditLine.DetailType = "JournalEntryLineDetail";

                var creditDetail = creditLine.JournalEntryLineDetail = new JournalEntryLineDetail();
                creditDetail.PostingType = "Credit";

                creditDetail.AccountRef = new AccountRef()
                {
                    value = CreditCOAValue,
                    name = CreditCOAName
                };

                journalList.Add(creditLine);

                updateJournalReq.Line = journalList;
            }

            return updateJournalReq;
        }
        public CreateJournalRes CreateUnapplyPaymentJournalEntry(SqlTransaction PaymentsTransaction, Payments payments, List<PaymentApplicationLineItems> PaymentApplicationLineItemsList, JournalEntries journalEntries, bool Is_Reversal = false)
        {
            _ServiceLogInfo = new ServiceLogInfo()
            {
                Client_Id = _Client_Id,
                Service_Id = _Service_Id,
                ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.CreateJournalEntry,
            };
            string Service_Url = $"{_QBOParameters.BaseURL}/v3/company/{_QBOParameters.Company_Id}/journalentry?minorversion={_QBOParameters.MinorVersion}";
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            if (_Token == null)
                _Token = RefreshAccessToken();

            Headers.Add("Authorization", "Bearer " + _Token.AccessToken);
            CreateJournalRes createJournalResponse = new CreateJournalRes();
            CreateJournalReq createJournalRequest = new CreateJournalReq();
            try
            {

                createJournalRequest = PrepareUnApplyPaymentJournalReqObj(PaymentsTransaction, PaymentApplicationLineItemsList, journalEntries, Is_Reversal);
                if (createJournalRequest.Fault?.Error?.Any() == true)
                {
                    var errorMessages = string.Join("\n", createJournalRequest.Fault.Error.Select(e => $"{e.Message} ({e.Detail})"));

                    throw new ApplicationException("Error while preparing the invoice journal entry request:\n\n" + errorMessages);
                }

            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }


            try
            {
                createJournalResponse = (CreateJournalRes)HttpServiceUtility.ExecuteJsonPostRequest(_ServiceLogInfo, Service_Url, createJournalRequest, typeof(CreateJournalRes), false, Headers);
                if (!string.IsNullOrEmpty(createJournalResponse?.JournalEntry?.Id))
                {
                    int updatedJournalIndex = (journalEntries?.JournalIndex ?? 0) + 1;
                    JournalEntries entries = new JournalEntries();
                    entries.JournalIndex = updatedJournalIndex;
                    entries.ExternalJournalEntryIdentifier = createJournalResponse.JournalEntry?.Id;
                    entries.Source_Id = payments.Payment_Id;
                    entries.SourceType_Id = MasterLookup._Sources.Payment;
                    entries.JournalEntryName = createJournalResponse.JournalEntry?.DocNumber;
                    JournalEntries journalEntry = InsertJournalEntries(PaymentsTransaction, MasterLookup._Sources.Payment, payments.Payment_Id, createJournalResponse.JournalEntry.Id, entries.JournalIndex, createJournalResponse.JournalEntry.DocNumber);
                    if (journalEntries != null)
                    {
                        InsertJournalEntriesLog(PaymentsTransaction, createJournalResponse?.JournalEntry?.Line, journalEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                _ServiceLogInfo.Is_Success = false;
                _ServiceLogInfo.Message = ex.Message;
            }
            _ServiceLogInfoRepository.InsertServiceLogInfo(_ServiceLogInfo);
            return createJournalResponse;
        }

        public CreateJournalReq PrepareUnApplyPaymentJournalReqObj(SqlTransaction PaymentsTransaction, List<PaymentApplicationLineItems> paymentApplicationLineItemsList, JournalEntries journalEntries, bool Is_Reversal = false)
        {
            var ChartOfAccountsLineItemsList = _LookupRepository.GetAllLineItems_ChartOfAccounts(_Service_Id, _Client_Id);
            var firstItem = paymentApplicationLineItemsList.First();
            CreateJournalReq createJournalReq = new CreateJournalReq();
            PaymentApplications paymentApplications = _PaymentsRepository.GetPaymentApplicationsByPaymentApplication_Id(firstItem.PaymentApplication_Id);
            Payments payments = _PaymentsRepository.GetPaymentDetails(paymentApplications.Payment_Id);
            byte _ChartOfAccountsClassificationType_Id = 0;
            _AccountingType_Id = firstItem.AccountingType_Id;
            if (firstItem.AccountingType_Id == MasterLookup._AccountingTypes.Cash)
            {
                _ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability;
            }
            else
            {
                _ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.AccountsReceivableAccountsPayable;
            }

            ReadByIdRes readByIdRes = GetJournalEntryById(journalEntries.ExternalJournalEntryIdentifier);
            var journalList = new List<JournalEntryLine>();
            createJournalReq.TxnDate = DateTime.Now.ToString("yyyy-MM-dd");

            string docNumber = CommonUtility.NormalizeDocNumber(readByIdRes.JournalEntry.DocNumber, journalEntries.JournalIndex);
            createJournalReq.DocNumber = docNumber;
            if (paymentApplicationLineItemsList == null || !paymentApplicationLineItemsList.Any())
            {
                return createJournalReq;
            }
            if (readByIdRes != null && readByIdRes.JournalEntry != null)
            {
                foreach (var paymentApplicationLineItems in paymentApplicationLineItemsList)
                {
                    string DebitCOAValue = null;
                    string CreditCOAValue = null;
                    string DebitCOAName = null;
                    string CreditCOAName = null;
                    string DebitDescription = null;
                    string CreditDescription = null;
                    int? DebitCOA = null;
                    int? CreditCOA = null;
                    InvoiceDetails invoiceDetails = _InvoiceRepository.GetInvoiceDetailsByInvoiceDetail_Id(paymentApplicationLineItems.InvoiceLineItem_Id);
                    Invoices invoices = _InvoiceRepository.GetInvoicesByInvoice_Id(invoiceDetails.Invoice_Id);
                    PolicyDetails policyDetails = _PolicyDetailsRepository.GetPolicyDetails(invoices.PolicyDetails_Id);
                    if (invoiceDetails != null)
                    {
                        LU_BillingType_LineItems_Directions BillingLineItemsDirections = LookupUtility.GetAllLU_BillingType_LineItems_Directions().FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id);
                        LU_LineItems_Directions LineItemsDirections = LookupUtility.GetAllLineItems_Directions().FirstOrDefault(x => x.LineItems_Directions_Id == BillingLineItemsDirections?.LineItems_Directions_Id);
                        LU_LineItems_Clients lineItems_Clients = LookupUtility.GetAllLineItems_Clients().FirstOrDefault(x => x.LineItems_Clients_Id == LineItemsDirections?.LineItems_Clients_Id);
                        LU_LineItems LineItems = LookupUtility.GetAllLineItems().FirstOrDefault(c => c.LineItem_Id == lineItems_Clients?.LineItem_Id);
                        LU_LineItemTypes LineType = LookupUtility.GetAllLineItemTypes().FirstOrDefault(c => c.LineItemType_Id == LineItems?.LineItemType_Id);
                        LU_LOBs LOB = LookupUtility.GetAllLOBs().FirstOrDefault(x => x.LOB_Id == invoiceDetails.LOB_Id);
                        LOBCarrierGrouping lobcarriergroupingDetails = _LobCarrierGroupingRepository.GetLOBCarrierGroupingDetails(LOB.LOB_Id, policyDetails.CarrierDetail_Id, _Service_Id, _Client_Id);
                        var ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id == _ChartOfAccountsClassificationType_Id);
                        var ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);

                        var PaymentDirectionsCOAList = _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id);
                        var PaymentDirectionsCOA = PaymentDirectionsCOAList.FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);
                        var PaymentDirectionsCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == PaymentDirectionsCOA?.StandardCOA_Id);

                        var errorDetail = BuildErrorDetail(PaymentDirectionsCOA.Name, LineItems?.Name, LOB?.Name, policyDetails.BillType);
                        if (PaymentDirectionsCOA == null || PaymentDirectionsCOAItem == null)
                            fault = AddError(fault, "Missing COA Configuration(Standard Payment Direction", errorDetail);

                        if (ChartOfAccountsLineItems == null || ChartOfAccounts == null)
                            fault = AddError(fault, "Missing COA Configuration(Line-Item", errorDetail);
                        if (fault != null && fault.Error != null && fault.Error.Any())
                        {
                            createJournalReq.Fault = fault;
                            return createJournalReq;
                        }
                        if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && LineItemsDirections.Direction_Id == MasterLookup._Directions.AR && paymentApplicationLineItems.AppliedAmount > 0)
                        {
                            CreditCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                            DebitCOAValue = ChartOfAccounts?.ReferenceID;
                            DebitDescription = $"Un Applied from Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                            CreditDescription = $"Un Applying incoming payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made by {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                            DebitCOAName = ChartOfAccounts?.Name;
                            CreditCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.Name!;
                            DebitCOA = ChartOfAccounts.ChartOfAccounts_Id!;
                            CreditCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                        }
                        else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && LineItemsDirections?.Direction_Id == MasterLookup._Directions.AP && paymentApplicationLineItems.AppliedAmount > 0)
                        {
                            if (lineItems_Clients?.Is_Passthrough == true && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                            {
                                ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id == MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability);
                                ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);
                            }
                            DebitCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                            CreditCOAValue = ChartOfAccounts?.ReferenceID;
                            CreditDescription = $"Un Applied to Expense Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                            DebitDescription = $"Un Applying outgoing payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                            DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                            CreditCOAName = ChartOfAccounts?.Name;
                            DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                            CreditCOA = ChartOfAccounts?.ChartOfAccounts_Id!;
                        }
                        else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && LineItemsDirections.Direction_Id == MasterLookup._Directions.AR && paymentApplicationLineItems.AppliedAmount < 0)
                        {
                            DebitCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                            CreditCOAValue = ChartOfAccounts?.ReferenceID;
                            DebitDescription = $"[REFUND] Un Applying outgoing payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                            CreditDescription = $"[REFUND] Un Applying to Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                            DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                            CreditCOAName = ChartOfAccounts?.Name;
                            DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                            CreditCOA = ChartOfAccounts?.ChartOfAccounts_Id!;
                        }
                        else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && LineItemsDirections.Direction_Id == MasterLookup._Directions.AP && paymentApplicationLineItems.AppliedAmount < 0)
                        {
                            if (lineItems_Clients?.Is_Passthrough == true && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                            {
                                ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id == MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability);
                                ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);
                            }
                            CreditCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                            DebitCOAValue = ChartOfAccounts?.ReferenceID;
                            DebitDescription = $"[RECOVERY] Un Applied to expense account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                            CreditDescription = $"[RECOVERY] Un Applying incoming payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made by {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                            DebitCOAName = ChartOfAccounts?.Name;
                            CreditCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.Name!;
                            DebitCOA = ChartOfAccounts?.ChartOfAccounts_Id!;
                            CreditCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                        }
                        else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && LineItemsDirections.Direction_Id == MasterLookup._Directions.AR && paymentApplicationLineItems.AppliedAmount < 0)
                        {
                            DebitCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                            CreditCOAValue = ChartOfAccounts?.ReferenceID;
                            DebitDescription = $"[Netting] Un Applying incoming payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made by {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                            CreditDescription = $"[Netting] Un Applied to Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                            DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                            CreditCOAName = ChartOfAccounts?.Name;
                            DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                            CreditCOA = ChartOfAccounts?.ChartOfAccounts_Id!;
                        }
                        else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && LineItemsDirections.Direction_Id == MasterLookup._Directions.AP && paymentApplicationLineItems.AppliedAmount < 0)
                        {
                            if (lineItems_Clients?.Is_Passthrough == true && _AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                            {
                                ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails?.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id == MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability);
                                ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);
                            }
                            DebitCOAValue = ChartOfAccounts?.ReferenceID;
                            CreditCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                            DebitDescription = $"[Netting] Un Applied to Expense Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                            CreditDescription = $"[Netting] Un Applying outgoing payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                            DebitCOAName = ChartOfAccounts?.Name;
                            CreditCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.Name!;
                            DebitCOA = ChartOfAccounts?.ChartOfAccounts_Id!;
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
                            DebitCOAValue = ChartOfAccounts?.ReferenceID;
                            CreditCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                            DebitDescription = $"[Netting] Applied to Expense Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                            CreditDescription = $"[Netting] Applying outgoing payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                            DebitCOAName = ChartOfAccounts?.Name;
                            CreditCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.Name!;
                            DebitCOA = ChartOfAccounts?.ChartOfAccounts_Id;
                            CreditCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                        }

                        else if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && LineItemsDirections.Direction_Id == MasterLookup._Directions.AR && paymentApplicationLineItems.AppliedAmount > 0)
                        {
                            CreditCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                            DebitCOAValue = ChartOfAccounts?.ReferenceID;
                            DebitDescription = $"[Netting] Un Applied to Income Account for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                            CreditDescription = $"[Netting] Un Applying incoming payment of Id - {payments.Payment_Id}, reference number: {payments.ReferenceNumber}, payment made by {payments.EntityName} for Invoice {invoices.InvoiceNo}, line item - {LOB?.Code} - {LineItems.Name}";
                            DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id)?.Name!;
                            CreditCOAName = ChartOfAccounts?.Name;
                            DebitCOA = ChartOfAccounts?.ChartOfAccounts_Id!;
                            CreditCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id)?.ChartOfAccounts_Id!;
                        }
                    }
                    var JournalEntryDebit = new JournalEntryLine();

                    JournalEntryDebit.Amount = Math.Abs(paymentApplicationLineItems.AppliedAmount);
                    JournalEntryDebit.Description = DebitDescription;
                    JournalEntryDebit.DetailType = "JournalEntryLineDetail";
                    var JournalEntryLine = JournalEntryDebit.JournalEntryLineDetail = new JournalEntryLineDetail();
                    JournalEntryLine.PostingType = "Debit";
                    var AccountRef = JournalEntryLine.AccountRef = new AccountRef();
                    AccountRef.value = DebitCOAValue;
                    AccountRef.name = DebitCOAName;
                    journalList.Add(JournalEntryDebit);

                    var journalEntryCredit = new JournalEntryLine();
                    journalEntryCredit.Amount = Math.Abs(paymentApplicationLineItems.AppliedAmount);
                    journalEntryCredit.Description = CreditDescription;
                    journalEntryCredit.DetailType = "JournalEntryLineDetail";
                    var JournalEntryLine2 = journalEntryCredit.JournalEntryLineDetail = new JournalEntryLineDetail();
                    JournalEntryLine2.PostingType = "Credit";
                    var AccountRef2 = JournalEntryLine2.AccountRef = new AccountRef();
                    AccountRef2.value = CreditCOAValue;
                    AccountRef2.name = CreditCOAName;
                    journalList.Add(journalEntryCredit);
                    //createJournalReq.Line = journalList;
                }
                createJournalReq.Line = journalList;

                if (Is_Reversal)
                {
                    var JournalEntryDebit = new JournalEntryLine();

                    JournalEntryDebit.Amount = payments.Amount;
                    string DebitCOAValue = null;
                    string CreditCOAValue = null;
                    string DebitCOAName = null;
                    string CreditCOAName = null;
                    string DebitDescription = null;
                    string CreditDescription = null;
                    int? DebitCOA = null;
                    int? CreditCOA = null;

                    var OperatingBanksCOAList = _LookupRepository.GetAllOperatingBanksChartOfAccounts(_Service_Id, _Client_Id);
                    var OperatingBankCOA = OperatingBanksCOAList.FirstOrDefault(x => x.OperatingBank_Id == payments.OperatingBank_Id);

                    var OperatingBankCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == OperatingBankCOA?.ChartOfAccounts_Id);

                    var PaymentDirectionsCOAList = _LookupRepository.GetAllPaymentDirectionsChartOfAccounts(_Service_Id, _Client_Id);
                    var PaymentDirectionsCOA = PaymentDirectionsCOAList.FirstOrDefault(x => x.PaymentDirection_Id == payments.PaymentDirection_Id);
                    var PaymentDirectionsCOAItem = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ChartOfAccounts_Id == PaymentDirectionsCOA?.StandardCOA_Id);
                    if (OperatingBankCOA == null || OperatingBankCOAItem == null)
                    {
                        fault = AddError(fault, "Missing COA Configuration(Operating Bank)");
                    }
                    if (PaymentDirectionsCOA == null || PaymentDirectionsCOAItem == null)
                    {
                        fault = AddError(fault, "Missing COA Configuration(Payment direction)");
                    }
                    if (fault != null && fault.Error != null && fault.Error.Any())
                    {
                        createJournalReq.Fault = fault;
                        return createJournalReq;
                    }
                    if (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming)
                    {

                        DebitCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                        CreditCOAValue = OperatingBankCOAItem?.ReferenceID;
                        DebitDescription = $"Record incoming payment - reference number: {payments.ReferenceNumber}, payment made by {payments.EntityName}";
                        CreditDescription = $"Applied to Incoming Payments Account";

                    }
                    else
                    {

                        DebitCOAValue = OperatingBankCOAItem?.ReferenceID;
                        CreditCOAValue = PaymentDirectionsCOAItem?.ReferenceID;
                        DebitDescription = $"Applied to Outgoing Payments Account";
                        CreditDescription = $"Record outgoing payment - reference number: {payments.ReferenceNumber}, payment made to {payments.EntityName}";

                    }
                    DebitCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id).Name;
                    CreditCOAName = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id).Name;
                    DebitCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == DebitCOAValue && c.Client_Id == _Client_Id).ChartOfAccounts_Id;
                    CreditCOA = LookupUtility.GetAllChartofAccounts().FirstOrDefault(c => c.ReferenceID == CreditCOAValue && c.Client_Id == _Client_Id).ChartOfAccounts_Id;

                    JournalEntryDebit.Amount = payments.Amount;
                    JournalEntryDebit.Description = DebitDescription;
                    JournalEntryDebit.DetailType = "JournalEntryLineDetail";
                    var JournalEntryLine = JournalEntryDebit.JournalEntryLineDetail = new JournalEntryLineDetail();
                    JournalEntryLine.PostingType = "Debit";
                    var AccountRef = JournalEntryLine.AccountRef = new AccountRef();
                    AccountRef.value = DebitCOAValue;
                    AccountRef.name = DebitCOAName;

                    journalList.Add(JournalEntryDebit);

                    var journalEntryCredit = new JournalEntryLine();
                    journalEntryCredit.Amount = payments.Amount;
                    journalEntryCredit.Description = CreditDescription;
                    journalEntryCredit.DetailType = "JournalEntryLineDetail";
                    var JournalEntryLine2 = journalEntryCredit.JournalEntryLineDetail = new JournalEntryLineDetail();
                    JournalEntryLine2.PostingType = "Credit";
                    var AccountRef2 = JournalEntryLine2.AccountRef = new AccountRef();
                    AccountRef2.value = CreditCOAValue;
                    AccountRef2.name = CreditCOAName;
                    journalList.Add(journalEntryCredit);


                    createJournalReq.Line = journalList;
                    createJournalReq.DocNumber = $"{docNumber} Rev";
                    createJournalReq.TxnDate = payments?.PaymentDate?.ToString("yyyy-MM-dd");
                }
            }
            return createJournalReq;
        }
    }
}
