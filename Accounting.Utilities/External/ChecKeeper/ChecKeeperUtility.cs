using Accounting.DBUtilities.Implementations.Transactions;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.External.ChecKeeper;
using Accounting.Entities.Internal;
using Accounting.Utilities.Internal;
using Newtonsoft.Json;

namespace Accounting.Utilities.External.PrintCheck
{
    public class ChecKeeperUtility
    {
        ChecKeeperParameters _ChecKeeperParameters = new ChecKeeperParameters();
        private readonly IPaymentsRepository _PaymentsRepository = new PaymentsRepository();
        private readonly IServiceLogInfoRepository _ServiceLogInfoRepository = new ServiceLogInfoRepository();
        public long _Client_Id { get; set; }
        public ChecKeeperUtility(long Client_Id)
        {
            var ServiceLevelParameters = LookupUtility.GetAllServiceLevelParameters().Where(x => x.Service_Id == MasterLookup._Services.ChecKeeper);
            _ChecKeeperParameters.BaseURL = ServiceLevelParameters.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.BaseURL)?.DefaultValue;
            _ChecKeeperParameters.AccessToken = ServiceLevelParameters.FirstOrDefault(x => x.Parameter_Id == MasterLookup._Parameters.AccessToken)?.DefaultValue;
            _Client_Id = Client_Id;
        }
        private CreateCheckReq BuildCreateCheckRequest(string AccountNumber, string RoutingNumber, string SignerValue, int CheckNumber, PrintCheckDetails firstRecord, List<List<string>> invoiceRows, string SignerType)
        {
            Delivery delivery = new Delivery();
            delivery.method = "pdf";

            Bank bank = new Bank();
            bank.routing = RoutingNumber;
            bank.account = AccountNumber;

            Signer signer = new Signer();
            signer.type = SignerType;
            signer.value = SignerValue;

            Payer payer = new Payer();
            payer.line1 = firstRecord.CompanyNameOnCheck == null ? SafeString(firstRecord.FromCompanyName, 35) : SafeString(firstRecord.CompanyNameOnCheck, 35);
            payer.line2 = SafeString(firstRecord.FromAddress, 35);


            Payee payee = new Payee();
            payee.line1 = firstRecord.PayeeNameOnCheck == null ? SafeString(firstRecord.EntityName, 35) : SafeString(firstRecord.PayeeNameOnCheck, 35);

            FromAddress fromAddress = new FromAddress();
            fromAddress.name = firstRecord.CompanyNameOnCheck == null ? SafeString(firstRecord.FromCompanyName, 35) : SafeString(firstRecord.CompanyNameOnCheck, 35);
            fromAddress.line1 = SafeString(firstRecord.FromAddress, 35);
            fromAddress.city = SafeString(firstRecord.FromCity, 30);
            fromAddress.state = SafeString(firstRecord.FromState, 2);
            fromAddress.zip = SafeString(firstRecord.FromZipCode, 10);

            ToAddress toAddress = new ToAddress();
            toAddress.name = firstRecord.PayeeNameOnCheck == null ? SafeString(firstRecord.EntityName, 35) : SafeString(firstRecord.PayeeNameOnCheck, 35);
            toAddress.line1 = SafeString(firstRecord.EntityAddress, 35, "Address Not Available");
            toAddress.city = SafeString(firstRecord.EntityCity, 30, "NA");
            toAddress.state = SafeString(firstRecord.EntityState, 2, "NA");
            toAddress.zip = SafeString(firstRecord.EntityZipCode, 10, "00000");

            InvoiceTable invoiceTable = new InvoiceTable();
            invoiceTable.headings = GetInvoiceHeaders();
            invoiceTable.rows = invoiceRows;

            CheckModel check = new CheckModel();
            check.bank = bank;
            check.signer = signer;
            check.payer = payer;
            check.payee = payee;
            check.amount = (int)Math.Round(firstRecord.Amount * 100);
            check.number = CheckNumber;
            check.date = Convert.ToDateTime(firstRecord.PaymentDate).ToString("yyyy-MM-dd");
            check.from_address = fromAddress;
            check.to_address = toAddress;
            check.invoice_table = invoiceTable;
            check.template = "default";
            check.ExcludeTemplate = ShouldExcludeTemplateFromCheckKeeper();
            CreateCheckReq request = new CreateCheckReq();
            request.delivery = delivery;
            request.checks = new List<CheckModel>();
            request.checks.Add(check);

            return request;
        }
        public ServiceResponse CreateCheck(long paymentId, string AccountNumber, string RoutingNumber, string SignerValue, int CheckNumber, string SignerType)
        {
            ServiceResponse serviceResponse = new ServiceResponse();

            try
            {
                var result = _PaymentsRepository.GetPrintCheckDetails(paymentId);

                if (result == null || !result.Any())
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = "Payment or InvoiceDetails not found, or the payment has not been applied to any invoice";
                    return serviceResponse;
                }

                var firstRecord = result.FirstOrDefault();

                List<List<string>> invoiceRows = new List<List<string>>();
                foreach (var x in result)
                {
                    List<string> row = new List<string>();
                    row.Add(x.InvoiceNo);
                    row.Add(x.PolicyNumber);
                    row.Add(x.AgencyName);
                    row.Add(x.InsuredsName);
                    row.Add(x.LineItemName);
                    row.Add("$" + x.AppliedAmount.ToString("0.00"));
                    invoiceRows.Add(row);
                }
                CreateCheckReq request = BuildCreateCheckRequest(AccountNumber, RoutingNumber, SignerValue, CheckNumber, firstRecord, invoiceRows, SignerType);
                string Service_Url = $"{_ChecKeeperParameters.BaseURL}/v3/check";
                string token = _ChecKeeperParameters.AccessToken;
                Dictionary<string, string> Headers = new Dictionary<string, string>();
                Headers.Add("Authorization", "Bearer " + token);
                ServiceLogInfo serviceLogInfo = new ServiceLogInfo();
                serviceLogInfo.Service_Id = MasterLookup._Services.ChecKeeper;
                serviceLogInfo.Client_Id = _Client_Id;
                serviceLogInfo.ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.PrintCheck;

                string requestJson = JsonConvert.SerializeObject(request);
                var apiResponse = HttpServiceUtility.ExecuteJsonPostRequest(serviceLogInfo, Service_Url, requestJson, typeof(object), false, Headers);
                string jsonResponse = JsonConvert.SerializeObject(apiResponse);
                CreateCheckRes checkResponse = JsonConvert.DeserializeObject<CreateCheckRes>(jsonResponse);
                CreateCheckErrorRes errorResponse = null;

                if (checkResponse != null && checkResponse?.data != null)
                {
                    serviceResponse.Status = true;
                    serviceResponse.Data = checkResponse?.data.pdf;
                }
                else
                {
                    errorResponse = JsonConvert.DeserializeObject<CreateCheckErrorRes>(jsonResponse);
                    serviceResponse.Status = false;

                    if (errorResponse?.errors != null && errorResponse.errors.Any())
                    {
                        serviceResponse.Message = string.Join(Environment.NewLine, errorResponse.errors.SelectMany(x => x.Value));
                    }
                    else
                    {
                        serviceResponse.Message = errorResponse?.message ?? "An unknown error occurred while creating the check.";
                    }
                }
                _ServiceLogInfoRepository.InsertServiceLogInfo(serviceLogInfo);
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }
        public List<Heading> GetInvoiceHeaders()
        {
            List<Heading> headers = new List<Heading>();

            Heading h1 = new Heading();
            h1.label = "Invoice No";
            h1.size = 10;
            headers.Add(h1);

            Heading h2 = new Heading();
            h2.label = "Policy No";
            h2.size = 15;
            headers.Add(h2);

            Heading h3 = new Heading();
            h3.label = "Agency Name";
            h3.size = 15;
            headers.Add(h3);

            Heading h4 = new Heading();
            h4.label = "Insured Name";
            h4.size = 10;
            headers.Add(h4);

            Heading h5 = new Heading();
            h5.label = "Lineitem Name";
            h5.size = 10;
            headers.Add(h5);

            Heading h6 = new Heading();
            h6.label = "Applied Amount";
            h6.size = 10;
            headers.Add(h6);

            return headers;
        }
        public string SafeString(string value, int maxLength, string defaultValue = "NA")
        {
            value = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
            return value.Length > maxLength ? value.Substring(0, maxLength) : value;
        }
        public ServiceResponse CreateChecks(List<long> paymentIds, string AccountNumber, string RoutingNumber, string SignerValue, int StartingCheckNumber, string SignerType)
        {
            ServiceResponse serviceResponse = new ServiceResponse();

            try
            {
                CreateCheckReq request = new CreateCheckReq();

                request.delivery = new Delivery();
                request.delivery.method = "pdf";

                request.checks = new List<CheckModel>();

                int checkNumber = StartingCheckNumber;

                foreach (long paymentId in paymentIds)
                {
                    var result = _PaymentsRepository.GetPrintCheckDetails(paymentId);

                    if (result == null || !result.Any())
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Message = $"Payment or invoice details not found, or the payment has not been applied to any invoice for Payment Id: {paymentId}.";
                        return serviceResponse;
                    }

                    var firstRecord = result.FirstOrDefault();

                    List<List<string>> invoiceRows = new List<List<string>>();

                    foreach (var x in result)
                    {
                        List<string> row = new List<string>();

                        row.Add(x.InvoiceNo);
                        row.Add(x.PolicyNumber);
                        row.Add(x.AgencyName);
                        row.Add(x.InsuredsName);
                        row.Add(x.LineItemName);
                        row.Add("$" + x.AppliedAmount.ToString("0.00"));

                        invoiceRows.Add(row);
                    }

                    CheckModel check = BuildCheckModel(AccountNumber, RoutingNumber, SignerValue, checkNumber, firstRecord, invoiceRows, SignerType);
                    request.checks.Add(check);
                    checkNumber++;
                }

                string Service_Url = $"{_ChecKeeperParameters.BaseURL}/v3/check";
                string token = _ChecKeeperParameters.AccessToken;

                Dictionary<string, string> Headers = new Dictionary<string, string>();

                Headers.Add("Authorization", "Bearer " + token);

                ServiceLogInfo serviceLogInfo = new ServiceLogInfo();

                serviceLogInfo.Service_Id = MasterLookup._Services.ChecKeeper;

                serviceLogInfo.Client_Id = _Client_Id;

                serviceLogInfo.ServiceRequestType_Id = MasterLookup._ServiceRequestTypes.PrintCheck;

                string requestJson = JsonConvert.SerializeObject(request);

                var apiResponse = HttpServiceUtility.ExecuteJsonPostRequest(serviceLogInfo, Service_Url, requestJson, typeof(object), false, Headers);

                string jsonResponse = JsonConvert.SerializeObject(apiResponse);

                CreateCheckRes checkResponse = JsonConvert.DeserializeObject<CreateCheckRes>(jsonResponse);

                CreateCheckErrorRes errorResponse = null;

                if (checkResponse != null && checkResponse.data != null)
                {
                    serviceResponse.Status = true;
                    serviceResponse.Data = checkResponse.data.pdf;
                }
                else
                {
                    errorResponse = JsonConvert.DeserializeObject<CreateCheckErrorRes>(jsonResponse);

                    serviceResponse.Status = false;

                    if (errorResponse?.errors != null &&
                        errorResponse.errors.Any())
                    {
                        serviceResponse.Message = string.Join(Environment.NewLine, errorResponse.errors.SelectMany(x => x.Value));
                    }
                    else
                    {
                        serviceResponse.Message = errorResponse?.message ?? "An unknown error occurred while creating the checks.";
                    }
                }

                _ServiceLogInfoRepository.InsertServiceLogInfo(serviceLogInfo);
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }
        private CheckModel BuildCheckModel(string AccountNumber, string RoutingNumber, string SignerValue, int CheckNumber, PrintCheckDetails firstRecord, List<List<string>> invoiceRows, string SignerType)
        {
            Bank bank = new Bank();
            bank.routing = RoutingNumber;
            bank.account = AccountNumber;

            Signer signer = new Signer();
            signer.type = SignerType;
            signer.value = SignerValue;

            Payer payer = new Payer();
            payer.line1 = firstRecord.CompanyNameOnCheck == null ? SafeString(firstRecord.FromCompanyName, 35) : SafeString(firstRecord.CompanyNameOnCheck, 35);

            payer.line2 = SafeString(firstRecord.FromAddress, 35);

            Payee payee = new Payee();
            payee.line1 = firstRecord.PayeeNameOnCheck == null ? SafeString(firstRecord.EntityName, 35) : SafeString(firstRecord.PayeeNameOnCheck, 35);

            FromAddress fromAddress = new FromAddress();

            fromAddress.name = firstRecord.CompanyNameOnCheck == null ? SafeString(firstRecord.FromCompanyName, 35) : SafeString(firstRecord.CompanyNameOnCheck, 35);

            fromAddress.line1 = SafeString(firstRecord.FromAddress, 35);
            fromAddress.city = SafeString(firstRecord.FromCity, 30);
            fromAddress.state = SafeString(firstRecord.FromState, 2);
            fromAddress.zip = SafeString(firstRecord.FromZipCode, 10);

            ToAddress toAddress = new ToAddress();

            toAddress.name = firstRecord.PayeeNameOnCheck == null ? SafeString(firstRecord.EntityName, 35) : SafeString(firstRecord.PayeeNameOnCheck, 35);

            toAddress.line1 = SafeString(firstRecord.EntityAddress, 35, "Address Not Available");

            toAddress.city = SafeString(firstRecord.EntityCity, 30, "NA");

            toAddress.state = SafeString(firstRecord.EntityState, 2, "NA");

            toAddress.zip = SafeString(firstRecord.EntityZipCode, 10, "00000");

            InvoiceTable invoiceTable = new InvoiceTable();

            invoiceTable.headings = GetInvoiceHeaders();
            invoiceTable.rows = invoiceRows;

            CheckModel check = new CheckModel();

            check.bank = bank;
            check.signer = signer;
            check.payer = payer;
            check.payee = payee;

            check.amount = (int)Math.Round(firstRecord.Amount * 100);

            check.number = CheckNumber;

            check.date = Convert.ToDateTime(firstRecord.PaymentDate).ToString("yyyy-MM-dd");

            check.from_address = fromAddress;
            check.to_address = toAddress;
            check.invoice_table = invoiceTable;
            check.template = "default";
            check.ExcludeTemplate = ShouldExcludeTemplateFromCheckKeeper();

            return check;
        }
        private bool ShouldExcludeTemplateFromCheckKeeper()
        {
            if (string.IsNullOrEmpty(LookupUtility.GetClientSettings(_Client_Id, MasterLookup._ClientSettings.ExcludeTemplateFromCheckKeeperRequest)))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}