using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.External.QBO.Journal;
using Accounting.Entities.Internal;
using Accounting.SchedulerJobs.Abstract;
using Accounting.SchedulerJobs.Models.MakePayments;
using Accounting.SchedulerJobs.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Globalization;
using System.Net.Http;

namespace Accounting.SchedulerJobs.Services
{
    public class MakePaymentService : IMakePaymentService
    {
        private readonly IMakePaymentRepositoryService makePaymentRepositoryService;
        public MakePaymentService(IMakePaymentRepositoryService makePaymentRepositoryService)
        {
            this.makePaymentRepositoryService = makePaymentRepositoryService;
        }
        public async Task<List<BillProcessQueue>> GetPendingBillProcessQueue()
        {
            return await makePaymentRepositoryService.GetPendingBillProcessQueue();
        }

        public async Task<BillProcessQueue> GetBillProcessQueue(long billProcess_Id, long client_Id)
        {
            return await makePaymentRepositoryService.GetBillProcessQueue(billProcess_Id, client_Id);
        }

        public async Task<List<BillProcessQueueEvents>> GetPendingBillProcessQueueEvents(long billProcessId, long clientId, DateTime? transactionDate = null)
        {
            return await makePaymentRepositoryService.GetPendingBillProcessQueueEvents(billProcessId, clientId, transactionDate);
        }

        public async Task<List<BillProcessQueueEvents>> GetCompletedBillProcessQueueEvents(long billProcessId, long clientId, DateTime? transactionDate = null)
        {
            return await makePaymentRepositoryService.GetCompletedBillProcessQueueEvents(billProcessId, clientId, transactionDate);
        }

        public async Task<string> UpdateBillProcessWorkFlowEvent(long billProcessId, long clientId, byte workFlowStatusId, long workFlowEventId, byte workFlowEventTypeId, string? serviceResponseMessage = null, string? processedBy = null)
        {
            return await makePaymentRepositoryService.UpdateBillProcessWorkFlowEvent(billProcessId, clientId, workFlowStatusId, workFlowEventId, workFlowEventTypeId, serviceResponseMessage, processedBy);
        }

        public async Task<string> MoveToNextWorkFlowEvent(long billProcessId, long clientId, long? payment_Id, string? processedBy = null)
        {
            return await makePaymentRepositoryService.MoveToNextWorkFlowEvent(billProcessId, clientId, payment_Id, processedBy);
        }

        public async Task<string> UpdateBillProcessQueue(long billProcessId, long clientId, byte billProcessStatusId, byte currentWorkFlowEventTypeId, string? errorMessage = null, string? processedBy = null)
        {
            return await makePaymentRepositoryService.UpdateBillProcessQueue(billProcessId, clientId, billProcessStatusId, currentWorkFlowEventTypeId, errorMessage, processedBy);
        }

        public async Task<List<BillProcessRetryConfiguration>> GetBillProcessRetryConfiguration()
        {
            return await makePaymentRepositoryService.GetBillProcessRetryConfiguration();
        }

        public async Task<string> SetBillProcessEventRetry(long billProcessId, long clientId, byte workFlowStatusId, long workFlowEventId, byte workFlowEventTypeId, byte delayMinutes, string? serviceResponseMessage = null, string? processedBy = null)
        {
            return await makePaymentRepositoryService.SetBillProcessEventRetry(billProcessId, clientId, workFlowStatusId, workFlowEventId, workFlowEventTypeId, delayMinutes, serviceResponseMessage, processedBy);
        }

        public async Task<string> FailBillProcess(long billProcessId, long clientId, long workFlowEventId, byte workFlowEventTypeId, string? serviceResponseMessage = null, string? processedBy = null)
        {
            return await makePaymentRepositoryService.FailBillProcess(billProcessId, clientId, workFlowEventId, workFlowEventTypeId , serviceResponseMessage, processedBy);
        }

        public async Task<ServiceResponse> CheckDuplicateMapping(long client_Id, byte direction_Id, List<SelectedInvoice> selectedInvoices, Bill billDetail)
        {
            var response = new ServiceResponse
            {
                Status = true,
                Message = "Success"
            };

            try
            {
                if (billDetail == null)
                {
                    response.Status = false;
                    response.Message = "Bill details are missing.";
                    return response;
                }

                if (selectedInvoices == null || !selectedInvoices.Any())
                {
                    return response;
                }

                var invoiceLineItemPairs = selectedInvoices?.Select(i => new { i.Invoice_Id, i.InvoiceLineItem_Id, i.LineItem_Id }).Distinct() ?? Enumerable.Empty<dynamic>();
                foreach (var item in invoiceLineItemPairs)
                {
                    var result = await makePaymentRepositoryService.GetExternalReferenceLineItems(billDetail.Entity_Id, billDetail.EntityType_Id, item.Invoice_Id, item.InvoiceLineItem_Id, item.LineItem_Id, direction_Id, client_Id);
                    if (result?.IsBillFullyApplied == true)
                    {
                        response.Status = false;
                        response.Message = $"A bill has already been fully applied for Invoice Id: {item.Invoice_Id}, Invoice Line Item Id: {item.InvoiceLineItem_Id}.";
                        return response;
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MakePaymentService: CheckDuplicateMapping Client_Id: {client_Id} Exception: {ex} Message: {ex.Message}");
                response.Status = false;
                response.Message = ex.Message;
                return response;
            }
        }

        public async Task<ServiceResponse> GenerateAndUploadCommissionDocuments(long report_Id, byte fileType_Id, long client_Id, List<SelectedInvoice> selectedInvoices, Bill billDetail, HttpClient httpClient)
        {
            var response = new ServiceResponse
            {
                Status = true,
                Message = "Success"
            };

            try
            {
                if (!billDetail.PostToAgentPortal)
                {
                    response.Message = "Commission statement upload was skipped as 'Post to Agent Portal' was not selected.";
                    return response;
                }

                var url = $"{ConfigurationHelper.Accounting_Base_URL}api/CommonService/GenerateAndUploadCommissionDocuments?Report_Id={report_Id}&Client_Id={client_Id}&FileType_Id={fileType_Id}";
                var apiResponse = await RestExtension.PostAsync(httpClient, url, selectedInvoices, ConfigurationHelper.SchedulerJobAuthToken, "Authorization", false);

                FileLogger.LogInformation($"MakePaymentService: GenerateAndUploadCommissionDocuments Client_Id: {client_Id} URL: {url} Authorization: {ConfigurationHelper.SchedulerJobAuthToken} SelectedInvoices: {JsonConvert.SerializeObject(selectedInvoices)}");
                if (apiResponse?.StatusCode != 200)
                {
                    response.Status = false;
                    response.Message = "Failed to generate/upload commission documents.";
                    return response;
                }

                var result = apiResponse.Result as JObject;
                response.Status = result?.Value<bool>("Status") ?? false;
                response.Message = result?.Value<string>("Message") ?? string.Empty;
                return response;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MakePaymentService: GenerateAndUploadCommissionDocuments Client_Id: {client_Id} Exception: {ex} Message: {ex.Message}");
                response.Status = false;
                response.Message = ex.Message;
                return response;
            }
        }

        public async Task<ServiceResponse> CreateBillAndPayment(long client_Id, byte direction_Id, List<SelectedInvoice> selectedInvoices, Bill billDetail, long billProcess_Id)
        {
            var response = new ServiceResponse
            {
                Status = true,
                Message = "Success"
            };

            var sqlTransaction = await SQLHelper.GetNewTransactionAsync();
            try
            {
                if (billDetail == null)
                {
                    response.Status = false;
                    response.Message = "Bill details are missing.";
                    return response;
                }

                if (selectedInvoices == null || !selectedInvoices.Any())
                {
                    return response;
                }

                var payments_BillsMapping = new Payments_BillsMapping();
                var externalVendorEntityReference = new ExternalVendorEntityReference();

                if (billDetail.EntityType_Id == ConfigurationHelper.MasterLookup_EntityTypes_State)
                {
                    var stateDetails = await makePaymentRepositoryService.GetStateDetailsByName($"State of {billDetail.EntityName}", client_Id);
                    if (stateDetails != null)
                    {
                        billDetail.Entity_Id = stateDetails.StateDetail_Id;
                    }
                    else
                    {
                        stateDetails = new StateDetails();
                        stateDetails.Name = $"State of {billDetail.EntityName}"; //billDetail.EntityName;
                        stateDetails.Client_Id = client_Id;
                        stateDetails = await makePaymentRepositoryService.InsertUpdateStateDetails(sqlTransaction, stateDetails);
                        billDetail.Entity_Id = stateDetails.StateDetail_Id;
                    }
                }
                billDetail.AmountApplied = Math.Round(billDetail.AmountApplied, 2);
                if ((billDetail.EntityType_Id > 0 && billDetail.Entity_Id > 0) || !string.IsNullOrEmpty(billDetail.EntityName))
                {
                    var InsertBill = await makePaymentRepositoryService.InsertUpdateBill(sqlTransaction, billDetail);
                    if (InsertBill.Bill_Id > 0)
                    {
                        LineItemBill billLineItem = new LineItemBill();
                        foreach (var item in selectedInvoices)
                        {
                            billLineItem.Bill_Id = InsertBill.Bill_Id;
                            billLineItem.Invoice_Id = item.Invoice_Id;
                            billLineItem.LineItem_Id = item.LineItem_Id;
                            billLineItem.InvoiceLineItem_Id = item.InvoiceLineItem_Id;
                            billLineItem.AmountApplied = item.AppliedAmount;
                            LineItemBill lineItemsBill = await makePaymentRepositoryService.InsertUpdateBillLineItems(sqlTransaction, billLineItem);
                        }

                        var payments = await PaymentDetails(sqlTransaction, billDetail, client_Id);
                        if (payments?.Payment_Id > 0)
                        {
                            payments_BillsMapping = await makePaymentRepositoryService.GetPayments_BillsMapping(payments.Payment_Id);
                            if (payments_BillsMapping == null)
                            {
                                payments_BillsMapping = new Payments_BillsMapping();
                                payments_BillsMapping.Payment_Id = payments.Payment_Id;
                                payments_BillsMapping.Bill_Id = InsertBill.Bill_Id;
                                payments_BillsMapping = await makePaymentRepositoryService.InsertPayments_BillsMapping(sqlTransaction, payments_BillsMapping);
                            }

                            response.Status = true;
                            response.Message = $"Create Bill and Payment record Successfully - {InsertBill.Bill_Id}";
                            var billNumber = billDetail.PaymentType == ConfigurationHelper.MasterLookup_PaymentTypes_Check ? $"Check{billDetail.Bill_Id}" : $"Bill{billDetail.Bill_Id}";
                            await makePaymentRepositoryService.UpdateBillDetailsForBillProcessQueue(sqlTransaction, client_Id, billProcess_Id, billDetail.Bill_Id, billNumber);
                            response.Data = payments;
                            sqlTransaction.Commit();
                        }
                        else
                        {
                            if (sqlTransaction?.Connection != null)
                            {
                                sqlTransaction.Rollback();
                            }

                            response.Status = false;
                            response.Message = "Failed to Create Bill and Payment.";
                            response.Data = null;
                        }
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                if (sqlTransaction?.Connection != null)
                {
                    sqlTransaction.Rollback();
                }

                FileLogger.LogError($"MakePaymentService: CheckDuplicateMapping Client_Id: {client_Id} Exception: {ex} Message: {ex.Message}");
                response.Status = false;
                response.Message = ex.Message;
                return response;
            }
        }

        public async Task<ServiceResponse> ApplyPayment(long client_Id, byte direction_Id, List<SelectedInvoice> selectedInvoices, Bill billDetail, ServiceResponse createBillAndPaymentResponse)
        {
            var response = new ServiceResponse
            {
                Status = true,
                Message = "Success"
            };

            try
            {
                long payment_Id = 0;
                if (createBillAndPaymentResponse?.Data != null)
                {
                    var paymentData = JsonConvert.DeserializeObject<Payments>(createBillAndPaymentResponse.Data.ToString());
                    payment_Id = paymentData?.Payment_Id ?? 0;
                }

                if (payment_Id == 0)
                {
                    response.Status = false;
                    response.Message = $"No payments were associated.";
                    return response;
                }

                response = await ApplyPayments(client_Id, billDetail, selectedInvoices, direction_Id, payment_Id);
                return response;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MakePaymentService: ApplyPayment Client_Id: {client_Id} Exception: {ex} Message: {ex.Message}");
                response.Status = false;
                response.Message = ex.Message;
                return response;
            }
        }

        public async Task<ServiceResponse> CreateVendorAndExternalBill(long client_Id, string User_Id, long Direction_Id, Bill billDetail, ServiceResponse createBillAndPaymentResponse, ServiceResponse applyPaymentResponse, HttpClient httpClient)
        {
            var response = new ServiceResponse
            {
                Status = true,
                Message = "Success"
            };

            try
            {
                long Payment_Id = 0;
                long Bill_Id = 0;
                //if (billDetail.PaymentType > 0 && billDetail.PaymentType == ConfigurationHelper.MasterLookup_PaymentTypes_Check)
                //{
                //    response.Message = "Payment Created for Check";
                //    return response;
                //}
                // Get Bill_Id from Message
                if (!string.IsNullOrWhiteSpace(createBillAndPaymentResponse?.Message))
                {
                    var messageParts = createBillAndPaymentResponse.Message.Split('-');
                    if (messageParts.Length > 1)
                    {
                        long.TryParse(messageParts[^1].Trim(), out Bill_Id);
                    }
                }

                // Get Payment_Id from Data
                if (createBillAndPaymentResponse?.Data != null)
                {
                    var paymentData = JsonConvert.DeserializeObject<Payments>(createBillAndPaymentResponse.Data.ToString());
                    Payment_Id = paymentData?.Payment_Id ?? 0;
                }

                var url = $"{ConfigurationHelper.Accounting_Base_URL}api/CommonService/CreateVendorAndExternalBill?Client_Id={client_Id}&PaymentToDate={billDetail.PaymentToDate}" +
                    $"&MemoDescription={billDetail.MemoDescription}&EntityName={billDetail.EntityName}&Payment_Id={Payment_Id}&User_Id={User_Id}" +
                    $"&EntityType_Id={billDetail.EntityType_Id}&Entity_Id={billDetail.Entity_Id}&Bill_Id={Bill_Id}&Direction_Id={Direction_Id}";

                var apiResponse = await RestExtension.PostAsync(httpClient, url, applyPaymentResponse.Data, ConfigurationHelper.SchedulerJobAuthToken, "Authorization", false);
                FileLogger.LogInformation($"MakePaymentService: CreateVendorAndExternalBill Client_Id: {client_Id} URL: {url} Authorization: {ConfigurationHelper.SchedulerJobAuthToken} ApplyPaymentResponse: {JsonConvert.SerializeObject(applyPaymentResponse.Data)}");
                if (apiResponse?.StatusCode != 200)
                {
                    response.Status = false;
                    response.Message = $"Failed to Create Vendor and ExternalBill. Message - {apiResponse?.Message ?? string.Empty}";
                    return response;
                }

                var result = apiResponse.Result as JObject;
                response.Status = result?.Value<bool>("Status") ?? false;
                response.Message = result?.Value<string>("Message") ?? string.Empty;
                return response;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MakePaymentService: CreateVendorAndExternalBill Client_Id: {client_Id} Exception: {ex} Message: {ex.Message}");
                response.Status = false;
                response.Message = ex.Message;
                return response;
            }
        }
        public async Task<ServiceResponse> UnapplyPayment(long paymentId, long Client_Id, long User_Id,  HttpClient httpClient)
        {
            var response = new ServiceResponse
            {
                Status = true,
                Message = "Success"
            };
            try
            {
                var url = $"{ConfigurationHelper.Accounting_Base_URL}api/CommonService/UnapplyPayment?paymentId={paymentId}&Client_Id={Client_Id}&User_Id={User_Id}";
                var apiResponse = await RestExtension.PostAsync(httpClient, url, null, ConfigurationHelper.SchedulerJobAuthToken, "Authorization", false);
                FileLogger.LogInformation($"MakePaymentService: UnapplyPayment Client_Id: {Client_Id} URL: {url} Authorization: {ConfigurationHelper.SchedulerJobAuthToken}");
                if (apiResponse?.StatusCode != 200)
                {
                    response.Status = false;
                    response.Message = $"Failed to unapply Payment. Message - {apiResponse?.Message ?? string.Empty}";
                    return response;
                }

                var result = apiResponse.Result as JObject;
                response.Status = result?.Value<bool>("Status") ?? false;
                response.Message = result?.Value<string>("Message") ?? string.Empty;
                return response;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MakePaymentService: UnapplyPayment Client_Id: {Client_Id} Exception: {ex} Message: {ex.Message}");
                response.Status = false;
                response.Message = ex.Message;
                return response;
            }


        }

        private async Task<Payments> PaymentDetails(SqlTransaction sqlTransaction, Bill makePayments, long client_Id)
        {
            try
            {
                var Client_Id = client_Id;
                Payments paymentDetails = new Payments();

                paymentDetails.EntityType_Id = makePayments.EntityType_Id;
                paymentDetails.PaymentDirection_Id = 2;
                paymentDetails.Entity_Id = (long)makePayments.Entity_Id;
                paymentDetails.PaymentDate = makePayments.PaymentToDate;
                paymentDetails.Amount = makePayments.AmountApplied;
                paymentDetails.PaymentStatus_Id = ConfigurationHelper.MasterLookup_PaymentStatus_Pending;
                paymentDetails.ReferenceNumber = makePayments.PaymentType == ConfigurationHelper.MasterLookup_PaymentTypes_Check ? $"Check{makePayments.Bill_Id}" : $"Bill{makePayments.Bill_Id}";
                paymentDetails.PaymentType_Id = makePayments.PaymentType == ConfigurationHelper.MasterLookup_PaymentTypes_Check ? ConfigurationHelper.MasterLookup_PaymentTypes_Check : ConfigurationHelper.MasterLookup_PaymentTypes_Bill;
                paymentDetails.OperatingBank_Id = makePayments.Operating_Bank_ID == 0 ? null : makePayments.Operating_Bank_ID;
                paymentDetails.Client_Id = Client_Id;
                paymentDetails.PayeeNameOnCheck = makePayments?.PayeeNameOnCheck;

                if (paymentDetails.Payment_Id == 0)
                {
                    paymentDetails.RemainingAmount = paymentDetails.Amount;
                }
                paymentDetails.SourceType_Id = ConfigurationHelper.MasterLookup_Sources_Payment;
                paymentDetails = await makePaymentRepositoryService.InsertUpdatePayments(sqlTransaction, paymentDetails);
                return paymentDetails;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MakePaymentService: PaymentDetails Client_Id: {client_Id} Exception: {ex} Message: {ex.Message}");
                throw;
            }
        }

        private async Task<ServiceResponse> ApplyPayments(long client_Id, Bill bill, List<SelectedInvoice> selectedInvoices, byte direction_Id, long payment_Id)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            double RemainingBalance = 0;
            bool Paid = false;
            DateTime? PaidOn = null;
            var clients = await makePaymentRepositoryService.GetClients(client_Id);
            List<PaymentApplicationLineItems> PaymentApplicationLineItemsList = new List<PaymentApplicationLineItems>();
            var PaymentsTransaction = await SQLHelper.GetNewTransactionAsync();
            bool IsOverride = true;
            if (PaymentsTransaction == null)
            {
                serviceResponse.Status = false;
                serviceResponse.Data = "Unable to initiate a Transaction for the process.";
                return serviceResponse;
            }
            try
            {
                var checkedInvoiceDetails = new List<SelectedInvoiceDetailDTO>();
                foreach (var item in selectedInvoices)
                {
                    bool isPartial = item.AppliedAmount != item.RemainingBalance;
                    int ManipulateSign = direction_Id == ConfigurationHelper.MasterLookup_Directions_AR ? -1 : 1;
                    SelectedInvoiceDetailDTO dto = new SelectedInvoiceDetailDTO
                    {
                        Invoice_Id = item.Invoice_Id,
                        Amount = item.AppliedAmount * ManipulateSign,
                        RemainingBalance = item.RemainingBalance * ManipulateSign,
                        IsPartial = isPartial
                    };

                    if (isPartial)
                    {
                        dto.InvoiceDetail_Id = 0;
                        dto.InvoiceDetail_Ids = new List<long> { item.InvoiceLineItem_Id };
                    }
                    else
                    {
                        dto.InvoiceDetail_Id = item.InvoiceLineItem_Id;
                        dto.InvoiceDetail_Ids = null;
                    }

                    checkedInvoiceDetails.Add(dto);
                }

                var payments = await makePaymentRepositoryService.GetPaymentDetails(payment_Id);
                if (payments == null || payments?.Payment_Id == 0)
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = $"No payments were associated.";
                    return serviceResponse;
                }
                else
                {
                    payment_Id = payments.Payment_Id;
                    List<PolicyDetails> policyDetailsList = new List<PolicyDetails>();
                    double PaymentsRemainingAmount = payments.RemainingAmount;
                    bool hasPositive = checkedInvoiceDetails.Any(x => x.Amount > 0);
                    bool hasNegative = checkedInvoiceDetails.Any(x => x.Amount < 0);
                    bool allPositive = hasPositive && !hasNegative;
                    bool allNegative = hasNegative && !hasPositive;
                    bool hasMixed = hasPositive && hasNegative;


                    bool isAP = direction_Id == ConfigurationHelper.MasterLookup_Directions_AP;
                    bool isIncoming = payments.PaymentDirection_Id == ConfigurationHelper.MasterLookup_PaymentDirections_Incoming;
                    bool isOutgoing = payments.PaymentDirection_Id == ConfigurationHelper.MasterLookup_PaymentDirections_Outgoing;
                    if (

                        (allPositive && isAP && isIncoming) ||    // Case 4 - All positives and direction = AP and if paymentdirection is Incoming
                        (allNegative && isAP && isOutgoing) ||    // Case 5 -  All negatives and direction = AP and if paymentdirection is outgoing
                        (hasMixed && isAP && isIncoming)          // Case 6 - any negatives any positives and direction = AP and if paymentdirection is incoming
                      )
                    {
                        serviceResponse.Status = false;

                        if (isAP)
                        {
                            if (isIncoming)
                                serviceResponse.Message = "In AP, incoming payments cannot have positive or mixed amounts.";
                            else
                                serviceResponse.Message = "In AP, outgoing payments cannot have negative amounts.";
                        }
                        return serviceResponse;
                    }
                    if ((payments.PaymentDirection_Id == ConfigurationHelper.MasterLookup_PaymentDirections_Incoming && checkedInvoiceDetails.Any(x => x.Amount > 0) && direction_Id == ConfigurationHelper.MasterLookup_Directions_AP))
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Message = "Incoming payments cannot have positive amounts selected.";

                        return serviceResponse;
                    }
                    if (checkedInvoiceDetails?.Count() == 0)
                    {
                        serviceResponse.Status = false;
                        //serviceResponse.Data = "Please select at least one line item.";
                        serviceResponse.Data = "Please select at least one full or partial invoice to apply payment";
                        return serviceResponse;
                    }
                    else
                    {
                        decimal ValidatePaymentremainingAmount = 0;
                        if ((payments.PaymentDirection_Id == ConfigurationHelper.MasterLookup_PaymentDirections_Incoming && checkedInvoiceDetails.All(x => x.Amount < 0) && direction_Id == ConfigurationHelper.MasterLookup_Directions_AP)
                            ||
                                (payments.PaymentDirection_Id == ConfigurationHelper.MasterLookup_PaymentDirections_Outgoing && checkedInvoiceDetails.All(x => x.Amount < 0) && direction_Id == ConfigurationHelper.MasterLookup_Directions_AR))
                        {
                            ValidatePaymentremainingAmount = (decimal)checkedInvoiceDetails.Sum(x => Math.Abs(x.Amount));
                        }
                        else
                        {
                            ValidatePaymentremainingAmount = (decimal)checkedInvoiceDetails.Sum(x => x.Amount);
                        }
                        if ((decimal)payments.RemainingAmount < ValidatePaymentremainingAmount || (ValidatePaymentremainingAmount < 0))
                        {
                            serviceResponse.Status = false;
                            serviceResponse.Message = "The amount selected exceeded the payment amount";
                            return serviceResponse;
                        }
                        //var groupedInvoiceList = checkedInvoiceDetails?.GroupBy(c => c.Invoice_Id)?.ToList();
                        var partialpaymentInvoiceList = checkedInvoiceDetails?.Where(c => c.IsPartial == true).GroupBy(c => c.Invoice_Id).ToList();
                        var fullpaymentInvoiceList = checkedInvoiceDetails?.Where(c => c.IsPartial == false).GroupBy(c => c.Invoice_Id).ToList();
                        if (partialpaymentInvoiceList?.Count() > 0)
                        {
                            byte _AccountingType_Id = 0;
                            foreach (var item in partialpaymentInvoiceList)
                            {
                                long invoiceId = item.Key;
                                Invoices invoices = await makePaymentRepositoryService.GetInvoicesByInvoice_Id(invoiceId);
                                PolicyDetails policyDetails = await makePaymentRepositoryService.GetPolicyDetails(invoices.PolicyDetails_Id);
                                if (policyDetails != null && payments.EntityType_Id == ConfigurationHelper.MasterLookup_EntityTypes_Agency)
                                {
                                    AgentDetails agentDetails = await makePaymentRepositoryService.GetAgentDetails(policyDetails.AgentDetail_Id);
                                    if (agentDetails != null)
                                    {
                                        policyDetails.BankName = agentDetails.BankName;
                                        policyDetails.AccountTypeName = agentDetails.AccountTypeName;
                                        policyDetails.RoutingNumber = agentDetails.RoutingNumber;
                                        policyDetails.AccountNumber = agentDetails.AccountNumber;
                                    }
                                }

                                var allBillingTypes = await makePaymentRepositoryService.GetAllBillingTypes();
                                byte BillingType_Id = allBillingTypes.FirstOrDefault(x => x.Code == policyDetails.BillType)?.BillingType_Id ?? 0;
                                LU_Clients_AccountingTypes_BillingTypes _AccountingTypes_BillingTypes = await makePaymentRepositoryService.GetClients_AccountingTypes_BillingTypes(clients.Client_Id, BillingType_Id);
                                if (_AccountingTypes_BillingTypes?.AccountingType_Id > 0)
                                {
                                    _AccountingType_Id = _AccountingTypes_BillingTypes.AccountingType_Id;
                                }
                                else
                                {
                                    _AccountingType_Id = clients.AccountingType_Id;
                                }
                                policyDetails.AccountingType_Id = _AccountingType_Id;
                                var invoiceLineItems = checkedInvoiceDetails?.Where(x => x.Invoice_Id == invoiceId)?.ToList();
                                var selectedAmountMap = new Dictionary<long, double>();

                                if (invoiceLineItems != null)
                                {
                                    foreach (var selectedLineItem in invoiceLineItems)
                                    {
                                        if (selectedLineItem.InvoiceDetail_Ids != null)
                                        {
                                            foreach (var id in selectedLineItem.InvoiceDetail_Ids)
                                            {
                                                selectedAmountMap[id] = selectedLineItem.Amount;
                                            }
                                        }

                                        if (selectedLineItem.InvoiceDetail_Id > 0)
                                        {
                                            selectedAmountMap[selectedLineItem.InvoiceDetail_Id] = selectedLineItem.Amount;
                                        }
                                    }
                                }
                                decimal TotalAmountChecked = 0;
                                decimal ActualAmount = 0;
                                if ((payments.PaymentDirection_Id == ConfigurationHelper.MasterLookup_PaymentDirections_Incoming && invoiceLineItems.All(x => x.Amount < 0) && direction_Id == ConfigurationHelper.MasterLookup_Directions_AP)
                                    ||
                                (payments.PaymentDirection_Id == ConfigurationHelper.MasterLookup_PaymentDirections_Outgoing && checkedInvoiceDetails.All(x => x.Amount < 0) && direction_Id == ConfigurationHelper.MasterLookup_Directions_AR))
                                {
                                    TotalAmountChecked = (decimal)invoiceLineItems.Sum(x => Math.Abs(x.Amount));
                                }
                                else
                                {
                                    TotalAmountChecked = (decimal)invoiceLineItems.Sum(x => x.Amount);
                                }
                                ActualAmount = (decimal)invoiceLineItems.Sum(x => x.Amount);
                                PaymentApplications paymentApplications = new PaymentApplications();
                                paymentApplications.Payment_Id = payment_Id;
                                paymentApplications.Invoice_Id = invoiceId;
                                paymentApplications.AppliedDate = DateTime.Now;
                                paymentApplications.TotalAppliedAmount = Convert.ToDouble(ActualAmount);
                                paymentApplications = await makePaymentRepositoryService.InsertUpdatePaymentApplications(PaymentsTransaction, paymentApplications);
                                if (paymentApplications.Payment_Id > 0)
                                {
                                    double AllocatedRemainingAmount = (double)ActualAmount;
                                    var invoiceDetailsList = await makePaymentRepositoryService.GetInvoiceDetailsByDirection_Id(invoiceId, direction_Id);
                                    // Collect all selected detail IDs (partial OR full)
                                    var selectedDetailIds = invoiceLineItems.SelectMany(x => x.InvoiceDetail_Ids != null && x.InvoiceDetail_Ids.Count > 0 ? x.InvoiceDetail_Ids : new List<long> { x.InvoiceDetail_Id }).ToList();

                                    if (selectedDetailIds.Count > 0)
                                    {
                                        invoiceDetailsList = invoiceDetailsList.Where(x => selectedDetailIds.Contains(x.Id)).ToList();
                                    }
                                    foreach (var lineItem in invoiceDetailsList)
                                    {
                                        double _ApplyAmount = 0;

                                        if (selectedAmountMap != null && selectedAmountMap.ContainsKey(lineItem.Id))
                                        {
                                            _ApplyAmount = selectedAmountMap[lineItem.Id];
                                        }
                                        if (_ApplyAmount != 0)
                                        {
                                            _ApplyAmount = Math.Sign(lineItem.RemainingBalance) * Math.Abs(_ApplyAmount);
                                            PaymentApplicationLineItems paymentApplicationLineItems = new PaymentApplicationLineItems();
                                            paymentApplicationLineItems.PaymentApplication_Id = paymentApplications.PaymentApplication_Id;
                                            paymentApplicationLineItems.InvoiceLineItem_Id = lineItem.Id;
                                            paymentApplicationLineItems.AppliedAmount = _ApplyAmount;
                                            paymentApplicationLineItems.Is_SplitPayment = false;
                                            paymentApplicationLineItems.SubType_Id = null;
                                            paymentApplicationLineItems.AccountingType_Id = _AccountingType_Id;
                                            await makePaymentRepositoryService.InsertUpdatePaymentApplicationLineItems(PaymentsTransaction, paymentApplicationLineItems);
                                            RemainingBalance = Math.Round(lineItem.RemainingBalance - Convert.ToDouble(Math.Sign(lineItem.RemainingBalance) * Math.Abs(_ApplyAmount)), 2);
                                            Paid = RemainingBalance == 0;
                                            PaidOn = Paid == true ? DateTime.Now : null;
                                            await makePaymentRepositoryService.UpdateBalanceInInvoiceDetails(PaymentsTransaction, lineItem.Id, RemainingBalance, Paid);
                                            PaymentApplicationLineItemsList.Add(paymentApplicationLineItems);
                                            var policyClone = new PolicyDetails
                                            {
                                                // copy required fields manually
                                                PolicyNumber = policyDetails.PolicyNumber,
                                                AccountingType_Id = policyDetails.AccountingType_Id,
                                                CarrierDetail_Id = policyDetails.CarrierDetail_Id,
                                                Client_Id = policyDetails.Client_Id,
                                                BankName = policyDetails.BankName,
                                                AccountTypeName = policyDetails.AccountTypeName,
                                                RoutingNumber = policyDetails.RoutingNumber,
                                                AccountNumber = policyDetails.AccountNumber
                                                // copy only what you need
                                            };

                                            var invoiceClone = new Invoices
                                            {
                                                InvoiceNo = invoices.InvoiceNo,
                                                Id = invoices.Id,
                                            };

                                            var invoiceDetailsClone = new InvoiceDetails
                                            {
                                                Id = lineItem.Id,
                                                Amount = lineItem.Amount,
                                                RemainingBalance = lineItem.RemainingBalance,
                                                LOB_Id = lineItem.LOB_Id,
                                                BillingType_LineItems_Directions_Id = lineItem.BillingType_LineItems_Directions_Id,
                                                Invoice_Id = lineItem.Invoice_Id,
                                            };

                                            invoiceDetailsClone.PaymentApplicationLineItems = new PaymentApplicationLineItems
                                            {
                                                PaymentApplication_Id = paymentApplicationLineItems.PaymentApplication_Id,
                                                AppliedAmount = paymentApplicationLineItems.AppliedAmount,
                                                InvoiceLineItem_Id = paymentApplicationLineItems.InvoiceLineItem_Id
                                            };

                                            invoiceClone.InvoiceDetails = invoiceDetailsClone;
                                            policyClone.Invoices = invoiceClone;

                                            policyDetailsList.Add(policyClone);
                                        }
                                    }
                                    if (direction_Id == ConfigurationHelper.MasterLookup_Directions_AR)
                                    {
                                        TotalAmountChecked = TotalAmountChecked * -1;
                                    }
                                    PaymentsRemainingAmount = Math.Round(PaymentsRemainingAmount - Convert.ToDouble(TotalAmountChecked), 2);
                                    await makePaymentRepositoryService.UpdatePaymentRemainingAmount(PaymentsTransaction, payment_Id, PaymentsRemainingAmount);
                                }
                            }
                        }
                        if (fullpaymentInvoiceList?.Count() > 0)
                        {
                            byte _AccountingType_Id = 0;
                            foreach (var item in fullpaymentInvoiceList)
                            {
                                long invoiceId = item.Key;
                                Invoices invoices = await makePaymentRepositoryService.GetInvoicesByInvoice_Id(invoiceId);
                                PolicyDetails policyDetails = await makePaymentRepositoryService.GetPolicyDetails(invoices.PolicyDetails_Id);
                                if (policyDetails != null && payments.EntityType_Id == ConfigurationHelper.MasterLookup_EntityTypes_Agency)
                                {
                                    AgentDetails agentDetails = await makePaymentRepositoryService.GetAgentDetails(policyDetails.AgentDetail_Id);
                                    if (agentDetails != null)
                                    {
                                        policyDetails.BankName = agentDetails.BankName;
                                        policyDetails.AccountTypeName = agentDetails.AccountTypeName;
                                        policyDetails.RoutingNumber = agentDetails.RoutingNumber;
                                        policyDetails.AccountNumber = agentDetails.AccountNumber;
                                    }
                                }
                                var allBillingTypes = await makePaymentRepositoryService.GetAllBillingTypes();
                                byte BillingType_Id = allBillingTypes.FirstOrDefault(x => x.Code == policyDetails.BillType)?.BillingType_Id ?? 0;
                                LU_Clients_AccountingTypes_BillingTypes _AccountingTypes_BillingTypes = await makePaymentRepositoryService.GetClients_AccountingTypes_BillingTypes(clients.Client_Id, BillingType_Id);
                                if (_AccountingTypes_BillingTypes?.AccountingType_Id > 0)
                                {
                                    _AccountingType_Id = _AccountingTypes_BillingTypes.AccountingType_Id;
                                }
                                else
                                {
                                    _AccountingType_Id = clients.AccountingType_Id;
                                }
                                policyDetails.AccountingType_Id = _AccountingType_Id;
                                var invoiceLineItems = checkedInvoiceDetails?.Where(x => x.Invoice_Id == invoiceId)?.ToList();
                                decimal TotalAmountChecked = 0;
                                decimal ActualAmount = 0;
                                if ((payments.PaymentDirection_Id == ConfigurationHelper.MasterLookup_PaymentDirections_Incoming && invoiceLineItems.All(x => x.Amount < 0) && direction_Id == ConfigurationHelper.MasterLookup_Directions_AP))
                                {
                                    TotalAmountChecked = (decimal)invoiceLineItems.Sum(x => Math.Abs(x.Amount));
                                }
                                else
                                {
                                    TotalAmountChecked = (decimal)invoiceLineItems.Sum(x => x.Amount);
                                }
                                ActualAmount = (decimal)invoiceLineItems.Sum(x => x.Amount);
                                PaymentApplications paymentApplications = new PaymentApplications();
                                paymentApplications.Payment_Id = payment_Id;
                                paymentApplications.Invoice_Id = invoiceId;
                                paymentApplications.AppliedDate = DateTime.Now;
                                paymentApplications.TotalAppliedAmount = Convert.ToDouble(ActualAmount);

                                if (direction_Id == ConfigurationHelper.MasterLookup_Directions_AR)
                                {
                                    RemainingBalance = Math.Round(invoices.RemainingBalance - Convert.ToDouble(Math.Sign(invoices.RemainingBalance) * Math.Abs(TotalAmountChecked)), 2);
                                    Paid = RemainingBalance == 0;
                                    await makePaymentRepositoryService.UpdateBalanceInInvoices(PaymentsTransaction, invoiceId, RemainingBalance, Paid);
                                }
                                paymentApplications = await makePaymentRepositoryService.InsertUpdatePaymentApplications(PaymentsTransaction, paymentApplications);
                                if (paymentApplications.Payment_Id > 0)
                                {
                                    var invoiceDetailsList = checkedInvoiceDetails?.Where(x => x.Invoice_Id == invoiceId);
                                    if (invoiceDetailsList?.Count() > 0)
                                    {
                                        foreach (var invoice in invoiceDetailsList)
                                        {
                                            if (invoice.RemainingBalance != 0)
                                            {
                                                InvoiceDetails invoiceDetails = await makePaymentRepositoryService.GetInvoiceDetailsByInvoiceDetail_Id(invoice.InvoiceDetail_Id);
                                                //if ((invoiceDetails.EntityType_Id != payments.EntityType_Id || invoiceDetails.Entity_Id != payments.Entity_Id) && !IsOverride)
                                                if (!IsOverride && payments != null && invoiceDetails != null && payments.Entity_Id > 0 && payments.EntityType_Id > 0 && invoiceDetails.Entity_Id > 0 && invoiceDetails.EntityType_Id > 0 && (invoiceDetails.EntityType_Id != payments.EntityType_Id || invoiceDetails.Entity_Id != payments.Entity_Id))
                                                {
                                                    if (PaymentsTransaction.Connection != null)
                                                    {
                                                        PaymentsTransaction.Rollback();
                                                    }
                                                    serviceResponse.Status = false;
                                                    serviceResponse.Message = "Payment entity type must match the invoice entity type.";
                                                    serviceResponse.Data = new { EntityMismatch = true };
                                                    return serviceResponse;
                                                }
                                                PaymentApplicationLineItems paymentApplicationLineItems = new PaymentApplicationLineItems();
                                                paymentApplicationLineItems.PaymentApplication_Id = paymentApplications.PaymentApplication_Id;
                                                paymentApplicationLineItems.InvoiceLineItem_Id = invoice.InvoiceDetail_Id;
                                                paymentApplicationLineItems.AppliedAmount = invoice.Amount;
                                                paymentApplicationLineItems.Is_SplitPayment = false;
                                                paymentApplicationLineItems.AccountingType_Id = _AccountingType_Id;
                                                await makePaymentRepositoryService.InsertUpdatePaymentApplicationLineItems(PaymentsTransaction, paymentApplicationLineItems);
                                                RemainingBalance = Math.Round(invoice.RemainingBalance - Convert.ToDouble(Math.Sign(invoice.RemainingBalance) * Math.Abs(invoice.Amount)), 2);
                                                Paid = RemainingBalance == 0;
                                                PaidOn = Paid == true ? DateTime.Now : null;
                                                await makePaymentRepositoryService.UpdateBalanceInInvoiceDetails(PaymentsTransaction, invoice.InvoiceDetail_Id, RemainingBalance, Paid);
                                                PaymentApplicationLineItemsList.Add(paymentApplicationLineItems);
                                                var policyClone = new PolicyDetails
                                                {
                                                    // copy required fields manually
                                                    PolicyNumber = policyDetails.PolicyNumber,
                                                    AccountingType_Id = policyDetails.AccountingType_Id,
                                                    CarrierDetail_Id = policyDetails.CarrierDetail_Id,
                                                    Client_Id = policyDetails.Client_Id,
                                                    BankName = policyDetails.BankName,
                                                    AccountTypeName = policyDetails.AccountTypeName,
                                                    RoutingNumber = policyDetails.RoutingNumber,
                                                    AccountNumber = policyDetails.AccountNumber,
                                                    // copy only what you need
                                                };

                                                var invoiceClone = new Invoices
                                                {
                                                    InvoiceNo = invoices.InvoiceNo,
                                                    Id = invoices.Id,
                                                };

                                                var invoiceDetailsClone = new InvoiceDetails
                                                {
                                                    Id = invoiceDetails.Id,
                                                    Amount = invoiceDetails.Amount,
                                                    RemainingBalance = invoiceDetails.RemainingBalance,
                                                    LOB_Id = invoiceDetails.LOB_Id,
                                                    BillingType_LineItems_Directions_Id = invoiceDetails.BillingType_LineItems_Directions_Id,
                                                };

                                                invoiceDetailsClone.PaymentApplicationLineItems = new PaymentApplicationLineItems
                                                {
                                                    AppliedAmount = paymentApplicationLineItems.AppliedAmount,
                                                    InvoiceLineItem_Id = paymentApplicationLineItems.InvoiceLineItem_Id
                                                };

                                                invoiceClone.InvoiceDetails = invoiceDetailsClone;
                                                policyClone.Invoices = invoiceClone;

                                                policyDetailsList.Add(policyClone);
                                                //policyDetails.Invoices = invoices;
                                                //policyDetails.Invoices.InvoiceDetails = invoiceDetails;
                                                //policyDetails.Invoices.InvoiceDetails.PaymentApplicationLineItems = paymentApplicationLineItems;
                                                //policyDetailsList.Add(policyDetails);
                                            }
                                        }
                                    }
                                }
                                if (direction_Id == ConfigurationHelper.MasterLookup_Directions_AR)
                                {
                                    TotalAmountChecked = TotalAmountChecked * -1;
                                }
                                PaymentsRemainingAmount = Math.Round(PaymentsRemainingAmount - Convert.ToDouble(TotalAmountChecked), 2);
                                await makePaymentRepositoryService.UpdatePaymentRemainingAmount(PaymentsTransaction, payment_Id, PaymentsRemainingAmount);
                            }
                        }
                        if (PaymentApplicationLineItemsList.Count() > 0 && policyDetailsList?.Count > 0)
                        {
                            serviceResponse.Data = policyDetailsList;
                            //if (clients.AccountingService_Id == MasterLookup._Services.Intuit)
                            //{
                            //    JournalEntries journalEntries = _journalEntryLogRepository.GetJournalEntry(MasterLookup._Sources.Payment, payments.Payment_Id);
                            //    QBOUtility qBOUtility = new QBOUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                            //    UpdateJournalRes updateJournalRes = qBOUtility.UpdateJournalEntry(PaymentsTransaction, payments, PaymentApplicationLineItemsList, journalEntries, true);
                            //    if (updateJournalRes != null && updateJournalRes.JournalEntry?.Id != null)
                            //    {
                            //        serviceResponse.Status = true;
                            //        serviceResponse.Message = "Applied payment successfully.";
                            //        PaymentsTransaction.Commit();
                            //        return serviceResponse;
                            //    }
                            //    else
                            //    {
                            //        if (PaymentsTransaction.Connection != null)
                            //        {
                            //            PaymentsTransaction.Rollback();
                            //        }
                            //        serviceResponse.Status = false;
                            //        serviceResponse.Message = updateJournalRes?.Fault?.Error?[0]?.Detail != null ? $"[QBO Error]: {updateJournalRes.Fault.Error[0].Detail}" : "Unable to process request";
                            //        return serviceResponse;
                            //    }
                            //}
                            //else
                            //{
                            //    PaymentsTransaction.Commit();
                            //}
                            PaymentsTransaction.Commit();
                        }
                    }
                    serviceResponse.Status = true;
                    serviceResponse.Message = "Applied payment successfully.";
                    return serviceResponse;
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"MakePaymentService: CreateVendorAndExternalBill Client_Id: {client_Id} Exception: {ex} Message: {ex.Message}");
                if (PaymentsTransaction.Connection != null)
                {
                    PaymentsTransaction.Rollback();
                }
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;

        }   
    }
}
