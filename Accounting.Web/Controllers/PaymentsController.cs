using Accounting.DBUtilities.Implementations.Transactions;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.External.QBO.Journal;
using Accounting.Entities.External.Sage;
using Accounting.Entities.External.Sage.Journal;
using Accounting.Entities.Internal;
using Accounting.Utilities.External.PrintCheck;
using Accounting.Utilities.External.QBO;
using Accounting.Utilities.External.Sage;
using Accounting.Utilities.Internal;
using Accounting.Utilities.Internal.Interfaces;
using Accounting.Web.Models;
using ClosedXML.Excel;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Accounting.Web.Controllers
{
    public class PaymentsController : BaseController
    {
        private readonly IPDFOptions _IPDFOptions;
        private readonly ViewRender view;
        private readonly IExcelLogRepository _IExcelLogRepository = new ExcelLogRepository();

        public PaymentsController(IPDFOptions iPDFOptions, ViewRender view)
        {
            _IPDFOptions = iPDFOptions;
            this.view = view;
        }

        public IActionResult Index()
        {
            if (!Is_AjaxRequest)
                return View();
            return PartialView();
        }
        public IActionResult PaymentsGrid()
        {
            return PartialView(_PaymentsList);

            //List<Payments> List = _paymentsRepository.GetAllPayments(userSession.Client_Id);
            //return PartialView(List);
        }
        public IActionResult PaymentDetails(long? Payment_Id)
        {
            Payments payments = _paymentsRepository.GetPaymentDetails(Payment_Id ?? 0);
            if (Payment_Id > 0)
            {
                payments.IsEdit = true;
            }
            return PartialView(payments);
        }
        public IActionResult PaymentDetailsViewOnly(long Payment_Id)
        {
            Payments payments = _paymentsRepository.GetPaymentDetails(Payment_Id);
            if (payments != null)
            {
                payments.PaymentApplicationLineItemsList = _paymentsRepository.GetPaymentApplicationLineItemsByPayment_Id(Payment_Id);
                payments.WriteoffPaymentApplicationLineItems = _paymentsRepository.GetWriteoffPaymentApplicationLineItemsByPayment_Id(Payment_Id);
            }
            return PartialView(payments);
        }
        public IActionResult PaymentReceipt(long Payment_Id)
        {
            Payments payments = new Payments();
            List<Clients> clients = new();
            payments = _paymentsRepository.GetPaymentDetails(Payment_Id);
            payments.Clients = _paymentsRepository.GetClientByPaymentId(Payment_Id);
            payments.WriteoffPaymentApplicationLineItems = _paymentsRepository.GetWriteoffPaymentApplicationLineItemsByPayment_Id(Payment_Id);


            if (payments != null)
            {
                payments.PaymentApplicationLineItemsList = _paymentsRepository.GetPaymentApplicationLineItemsForReceiptByPayment_Id(Payment_Id);
            }
            return PartialView(payments);
        }

        public ServiceResponse ApplyPaymentWriteoff(SelectedPaymentWriteoffDTO writeoffDetails)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            SqlTransaction PaymentsTransaction = null;
            PaymentsTransaction = _baseRepository.GetNewTransaction();
            Clients clients = _accountRepository.GetClients(userSession.Client_Id);
            if (PaymentsTransaction == null)
            {
                serviceResponse.Status = false;
                serviceResponse.Data = "Unable to initiate a Transaction for the process.";
                return serviceResponse;
            }
            try
            {
                if (writeoffDetails != null)
                {
                    if (writeoffDetails.Payment_Id == 0)
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Message = $"No payments were associated.";
                        return serviceResponse;
                    }
                    else
                    {
                        Payments payments = _paymentsRepository.GetPaymentDetails(writeoffDetails.Payment_Id);
                        double PaymentsRemainingAmount = Math.Round(payments.RemainingAmount, 2);
                        if (PaymentsRemainingAmount >= writeoffDetails.Amount)
                        {
                            PaymentApplications paymentApplications = new PaymentApplications();
                            paymentApplications.Payment_Id = writeoffDetails.Payment_Id;
                            paymentApplications.Invoice_Id = null;
                            paymentApplications.AppliedDate = DateTime.Now;
                            paymentApplications.Notes = writeoffDetails.Note;
                            paymentApplications.TotalAppliedAmount = Convert.ToDouble(writeoffDetails.Amount);
                            paymentApplications = _paymentsRepository.InsertUpdatePaymentApplications(PaymentsTransaction, paymentApplications);
                            PaymentApplicationLineItems paymentApplicationLineItems = new PaymentApplicationLineItems();
                            if (paymentApplications.Payment_Id > 0)
                            {
                                paymentApplicationLineItems.PaymentApplication_Id = paymentApplications.PaymentApplication_Id;
                                paymentApplicationLineItems.InvoiceLineItem_Id = null;
                                paymentApplicationLineItems.AppliedAmount = writeoffDetails.Amount;
                                paymentApplicationLineItems.Is_SplitPayment = false;
                                paymentApplicationLineItems.SubType_Id = MasterLookup._PaymentDirectionsSubTypes.Writeoff;
                                _paymentsRepository.InsertUpdatePaymentApplicationLineItems(PaymentsTransaction, paymentApplicationLineItems);
                            }
                            PaymentsRemainingAmount = Math.Round(PaymentsRemainingAmount - Convert.ToDouble(writeoffDetails.Amount), 2);
                            _paymentsRepository.UpdatePaymentRemainingAmount(PaymentsTransaction, writeoffDetails.Payment_Id, PaymentsRemainingAmount);
                            if (paymentApplicationLineItems != null)
                            {
                                List<JournalEntries> journalEntryList = _journalEntryLogRepository.GetJournalEntryByPayment(MasterLookup._Sources.Payment, payments.Payment_Id);
                                JournalEntries journalEntry = journalEntryList?.OrderByDescending(x => x.JournalIndex).FirstOrDefault();

                                if (clients.AccountingService_Id == MasterLookup._Services.Intuit)
                                {
                                    QBOUtility qBOUtility = new QBOUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                                    //UpdateJournalRes updateJournalRes = qBOUtility.CreatePaymentWriteoffJournalEntry(PaymentsTransaction, payments, paymentApplications, paymentApplicationLineItems, journalEntries);
                                    CreateJournalRes createJournalRes = qBOUtility.CreatePaymentWriteoffJournalEntry(PaymentsTransaction, payments, paymentApplications, paymentApplicationLineItems, journalEntry, false, false);
                                    if (createJournalRes != null && createJournalRes.JournalEntry?.Id != null)
                                    {
                                        serviceResponse.Status = true;
                                        serviceResponse.Message = $"Successfully applied Payment write-off (Payment ID: {payments.Payment_Id}).";
                                        PaymentsTransaction.Commit();
                                        return serviceResponse;
                                    }
                                    else
                                    {
                                        if (PaymentsTransaction.Connection != null)
                                        {
                                            PaymentsTransaction.Rollback();
                                        }
                                        serviceResponse.Status = false;
                                        serviceResponse.Message = "Unable to process request.";
                                        return serviceResponse;
                                    }
                                }
                                else if (clients.AccountingService_Id == MasterLookup._Services.Sage)
                                {
                                    SageUtility sageUtility = new SageUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                                    var createJournalRes = sageUtility.CreatePaymentWriteoffJournalEntry_Intacct(PaymentsTransaction, payments, paymentApplications, paymentApplicationLineItems, journalEntry, false, false);
                                    if (createJournalRes != null && createJournalRes != null)
                                    {
                                        serviceResponse.Status = true;
                                        serviceResponse.Message = $"Successfully applied Payment write-off (Payment ID: {payments.Payment_Id}).";
                                        PaymentsTransaction.Commit();
                                        return serviceResponse;
                                    }
                                    else
                                    {
                                        if (PaymentsTransaction.Connection != null)
                                        {
                                            PaymentsTransaction.Rollback();
                                        }
                                        serviceResponse.Status = false;
                                        serviceResponse.Message = "Unable to process request.";
                                        return serviceResponse;
                                    }
                                }
                                else
                                {
                                    PaymentsTransaction.Commit();
                                }
                            }
                        }
                        else
                        {
                            serviceResponse.Status = false;
                            serviceResponse.Message = $"Write-off amount does not match the payment remaining amount.";
                            return serviceResponse;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (PaymentsTransaction.Connection != null)
                {
                    PaymentsTransaction.Rollback();
                }
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
        public ServiceResponse UnApplyUpdatePaymentWriteoff(SelectedPaymentWriteoffDTO writeoffDetails)
        {
            try
            {
                if (writeoffDetails != null)
                {
                    if (writeoffDetails.Payment_Id == 0)
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Message = $"No payments were associated.";
                        return serviceResponse;
                    }
                    else
                    {
                        PaymentApplicationLineItems paymentApplicationLineItems = _paymentsRepository.GetWriteoffPaymentApplicationLineItemsByPayment_Id(writeoffDetails.Payment_Id);
                        if (paymentApplicationLineItems != null)
                        {
                            ServiceResponse unApplypaymentwriteoffserviceResponse = new ServiceResponse();
                            unApplypaymentwriteoffserviceResponse = UnApplyPaymentWriteoff(paymentApplicationLineItems, true);
                            if (!unApplypaymentwriteoffserviceResponse.Status)
                            {
                                return unApplypaymentwriteoffserviceResponse;
                            }
                            else
                            {
                                ServiceResponse updatepaymentwriteoffserviceResponse = new ServiceResponse();
                                updatepaymentwriteoffserviceResponse = UpdatePaymentWriteoff(writeoffDetails);
                                if (!updatepaymentwriteoffserviceResponse.Status)
                                {
                                    return updatepaymentwriteoffserviceResponse;
                                }
                            }
                            serviceResponse.Status = true;
                            serviceResponse.Message = $"Successfully update Payment write-off (Payment ID: {writeoffDetails.Payment_Id}).";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
        public ServiceResponse UpdatePaymentWriteoff(SelectedPaymentWriteoffDTO writeoffDetails)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            SqlTransaction PaymentsTransaction = null;
            PaymentsTransaction = _baseRepository.GetNewTransaction();
            Clients clients = _accountRepository.GetClients(userSession.Client_Id);
            if (PaymentsTransaction == null)
            {
                serviceResponse.Status = false;
                serviceResponse.Data = "Unable to initiate a Transaction for the process.";
                return serviceResponse;
            }
            try
            {
                if (writeoffDetails != null)
                {
                    if (writeoffDetails.Payment_Id == 0)
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Message = $"No payments were associated.";
                        return serviceResponse;
                    }
                    else
                    {
                        Payments payments = _paymentsRepository.GetPaymentDetails(writeoffDetails.Payment_Id);
                        double PaymentsRemainingAmount = Math.Round(payments.RemainingAmount, 2);
                        if (PaymentsRemainingAmount >= writeoffDetails.Amount)
                        {
                            PaymentApplications paymentApplications = new PaymentApplications();
                            paymentApplications.Payment_Id = writeoffDetails.Payment_Id;
                            paymentApplications.Invoice_Id = null;
                            paymentApplications.AppliedDate = DateTime.Now;
                            paymentApplications.Notes = writeoffDetails.Note;
                            paymentApplications.TotalAppliedAmount = Convert.ToDouble(writeoffDetails.Amount);
                            paymentApplications = _paymentsRepository.InsertUpdatePaymentApplications(PaymentsTransaction, paymentApplications);
                            PaymentApplicationLineItems paymentApplicationLineItems = new PaymentApplicationLineItems();
                            if (paymentApplications.Payment_Id > 0)
                            {
                                paymentApplicationLineItems.PaymentApplication_Id = paymentApplications.PaymentApplication_Id;
                                paymentApplicationLineItems.InvoiceLineItem_Id = null;
                                paymentApplicationLineItems.AppliedAmount = writeoffDetails.Amount;
                                paymentApplicationLineItems.Is_SplitPayment = false;
                                paymentApplicationLineItems.SubType_Id = MasterLookup._PaymentDirectionsSubTypes.Writeoff;
                                _paymentsRepository.InsertUpdatePaymentApplicationLineItems(PaymentsTransaction, paymentApplicationLineItems);
                            }
                            PaymentsRemainingAmount = Math.Round(PaymentsRemainingAmount - Convert.ToDouble(writeoffDetails.Amount), 2);
                            _paymentsRepository.UpdatePaymentRemainingAmount(PaymentsTransaction, writeoffDetails.Payment_Id, PaymentsRemainingAmount);
                            if (paymentApplicationLineItems != null)
                            {
                                List<JournalEntries> journalEntryList = _journalEntryLogRepository.GetJournalEntryByPayment(MasterLookup._Sources.Payment, payments.Payment_Id);
                                JournalEntries journalEntry = journalEntryList?.OrderByDescending(x => x.JournalIndex).FirstOrDefault();
                                if (clients.AccountingService_Id == MasterLookup._Services.Intuit)
                                {
                                    QBOUtility qBOUtility = new QBOUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                                    //UpdateJournalRes updateJournalRes = qBOUtility.UpdatePaymentWriteoffJournalEntry(PaymentsTransaction, payments, paymentApplications, paymentApplicationLineItems, journalEntries);
                                    CreateJournalRes createJournalRes = qBOUtility.CreatePaymentWriteoffJournalEntry(PaymentsTransaction, payments, paymentApplications, paymentApplicationLineItems, journalEntry, true);
                                    if (createJournalRes != null && createJournalRes.JournalEntry?.Id != null)
                                    {
                                        serviceResponse.Status = true;
                                        serviceResponse.Message = $"Successfully update Payment write-off (Payment ID: {payments.Payment_Id}).";
                                        PaymentsTransaction.Commit();
                                        return serviceResponse;
                                    }
                                    else
                                    {
                                        if (PaymentsTransaction.Connection != null)
                                        {
                                            PaymentsTransaction.Rollback();
                                        }
                                        serviceResponse.Status = false;
                                        serviceResponse.Message = "Unable to process request.";
                                        return serviceResponse;
                                    }
                                }
                                else if (clients.AccountingService_Id == MasterLookup._Services.Sage)
                                {
                                    SageUtility sageUtility = new SageUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                                    var createJournalRes = sageUtility.CreatePaymentWriteoffJournalEntry_Intacct(PaymentsTransaction, payments, paymentApplications, paymentApplicationLineItems, journalEntry, true);
                                    if (createJournalRes != null && createJournalRes != null)
                                    {
                                        serviceResponse.Status = true;
                                        serviceResponse.Message = $"Successfully update Payment write-off (Payment ID: {payments.Payment_Id}).";
                                        PaymentsTransaction.Commit();
                                        return serviceResponse;
                                    }
                                    else
                                    {
                                        if (PaymentsTransaction.Connection != null)
                                        {
                                            PaymentsTransaction.Rollback();
                                        }
                                        serviceResponse.Status = false;
                                        serviceResponse.Message = "Unable to process request.";
                                        return serviceResponse;
                                    }
                                }
                                else
                                {
                                    PaymentsTransaction.Commit();
                                }
                            }
                        }
                        else
                        {
                            serviceResponse.Status = false;
                            serviceResponse.Message = $"Write-off amount does not match the payment remaining amount.";
                            return serviceResponse;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (PaymentsTransaction.Connection != null)
                {
                    PaymentsTransaction.Rollback();
                }
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
        public ServiceResponse UnApplyPaymentWriteoff(PaymentApplicationLineItems paymentApplicationLineItems, bool Is_PaymentWriteoffDeleted = false)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            SqlTransaction PaymentsTransaction = null;
            PaymentsTransaction = _baseRepository.GetNewTransaction();
            Clients clients = _accountRepository.GetClients(userSession.Client_Id);
            if (PaymentsTransaction == null)
            {
                serviceResponse.Status = false;
                serviceResponse.Data = "Unable to initiate a Transaction for the process.";
                return serviceResponse;
            }
            try
            {
                double RemainingBalance = 0;
                if (paymentApplicationLineItems != null)
                {
                    PaymentApplications paymentApplications = _paymentsRepository.GetPaymentApplicationsByPaymentApplication_Id(paymentApplicationLineItems.PaymentApplication_Id);
                    if (paymentApplications != null)
                    {
                        Payments payments = _paymentsRepository.GetPaymentDetails(paymentApplications.Payment_Id);
                        if (payments != null)
                        {
                            RemainingBalance = payments.RemainingAmount + paymentApplicationLineItems.AppliedAmount;
                        }
                        _paymentsRepository.UpdatePaymentRemainingAmount(PaymentsTransaction, paymentApplications.Payment_Id, RemainingBalance);
                        _paymentsRepository.InactivePaymentApplicationLineItems(PaymentsTransaction, paymentApplicationLineItems.PaymentApplicationLineItem_Id);

                        List<JournalEntries> journalEntryList = _journalEntryLogRepository.GetJournalEntryByPayment(MasterLookup._Sources.Payment, payments.Payment_Id);
                        JournalEntries journalEntry = journalEntryList?.OrderByDescending(x => x.JournalIndex).FirstOrDefault();

                        if (clients.AccountingService_Id == MasterLookup._Services.Intuit)
                        {
                            QBOUtility qBOUtility = new QBOUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                            CreateJournalRes createJournalRes = qBOUtility.CreatePaymentWriteoffJournalEntry(PaymentsTransaction, payments, paymentApplications, paymentApplicationLineItems, journalEntry, false, Is_PaymentWriteoffDeleted);
                            if (createJournalRes != null && createJournalRes?.JournalEntry?.Id != null)
                            {
                                serviceResponse.Status = true;
                                serviceResponse.Message = "Un Applied payment write off successfully and synced to accounting software successfully.";
                                PaymentsTransaction.Commit();
                            }
                            else
                            {
                                if (PaymentsTransaction.Connection != null)
                                {
                                    PaymentsTransaction.Rollback();
                                }
                                serviceResponse.Status = false;
                                serviceResponse.Message = "Couldn't un apply and sync to accounting software";
                                return serviceResponse;
                            }
                        }
                        else if (clients.AccountingService_Id == MasterLookup._Services.Sage && Is_PaymentWriteoffDeleted)
                        {
                            SageUtility sageUtility = new SageUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                            var createJournalRes = sageUtility.CreatePaymentWriteoffJournalEntry_Intacct(PaymentsTransaction, payments, paymentApplications, paymentApplicationLineItems, journalEntry, false, Is_PaymentWriteoffDeleted);
                            if (createJournalRes != null && createJournalRes != null)
                            {
                                serviceResponse.Status = true;
                                serviceResponse.Message = $"Un Applied payment write off successfully and synced to accounting software successfully.";
                                PaymentsTransaction.Commit();
                                return serviceResponse;
                            }
                            else
                            {
                                if (PaymentsTransaction.Connection != null)
                                {
                                    PaymentsTransaction.Rollback();
                                }
                                serviceResponse.Status = false;
                                serviceResponse.Message = "Couldn't un apply and sync to accounting software";
                                return serviceResponse;
                            }
                        }
                        else
                        {
                            PaymentsTransaction.Commit();
                        }
                    }
                }
                serviceResponse.Status = true;
                serviceResponse.Message = string.IsNullOrEmpty(serviceResponse.Message) ? $"Un Applied payment write off succesfully." : serviceResponse.Message;
            }
            catch (Exception ex)
            {
                if (PaymentsTransaction.Connection != null)
                {
                    PaymentsTransaction.Rollback();
                }
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
        [HttpPost]
        public ServiceResponse DeletePaymentWriteoffDetails(long Payment_Id)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (Payment_Id == 0)
                {
                    serviceResponse.Status = false;
                    return serviceResponse;
                }
                else
                {
                    PaymentApplicationLineItems _paymentapplicationlineitems = _paymentsRepository.GetWriteoffPaymentApplicationLineItemsByPayment_Id(Payment_Id);
                    if (_paymentapplicationlineitems != null)
                    {
                        ServiceResponse response = UnApplyPaymentWriteoff(_paymentapplicationlineitems, true);
                        if (!response.Status)
                        {
                            serviceResponse.Status = true;
                            serviceResponse.Message = $"Failed to delete the payment write-off for (Payment ID: {Payment_Id}).";
                            return response;
                        }
                    }
                    serviceResponse.Status = true;
                    serviceResponse.Message = $"Successfully deleted the payment write-off for (Payment ID: {Payment_Id}).";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        [HttpPost]
        public async Task<ServiceResponse> PaymentDetails(Payments paymentDetails)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            CreateJournalRes createJournalResponse = new CreateJournalRes();
            SageResponse createSageJournalResponse = new SageResponse();
            SqlTransaction PaymentsTransaction = null;
            PaymentsTransaction = _baseRepository.GetNewTransaction();
            if (PaymentsTransaction == null)
            {
                serviceResponse.Status = false;
                serviceResponse.Data = "Unable to initiate a Transaction for the process.";
                return serviceResponse;
            }
            try
            {
                if (paymentDetails == null)
                {
                    serviceResponse.Status = false;
                }
                else
                {
                    if (paymentDetails.Payment_Id == 0)
                    {
                        paymentDetails.RemainingAmount = paymentDetails.Amount;
                    }
                    paymentDetails.SourceType_Id = MasterLookup._Sources.Payment;
                    paymentDetails.JournalIndex = 0;
                    paymentDetails = _paymentsRepository.InsertUpdatePayments(PaymentsTransaction, paymentDetails);
                    if (paymentDetails.Payment_Id > 0)
                    {
                        Clients clients = _accountRepository.GetClients(userSession.Client_Id);
                        if (clients?.AccountingService_Id == MasterLookup._Services.Intuit)
                        {
                            QBOUtility qBOUtility = new QBOUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);

                            if (!paymentDetails.IsEdit)
                            {
                                createJournalResponse = qBOUtility.CreatePaymentJournalEntry(PaymentsTransaction, paymentDetails);
                            }
                            else
                            {
                                List<JournalEntries> journalEntries = _journalEntryLogRepository.GetJournalEntryByPayment(MasterLookup._Sources.Payment, paymentDetails.Payment_Id);
                                JournalEntries journalEntry = journalEntries?.FirstOrDefault(x => x.JournalIndex == 0);

                                if (journalEntry != null)
                                {
                                    createJournalResponse = qBOUtility.UpdatePaymentJournalEntry(PaymentsTransaction, paymentDetails, journalEntry);
                                }
                            }
                            if (string.IsNullOrEmpty(createJournalResponse?.JournalEntry?.Id))
                            {
                                serviceResponse.Status = false;
                                serviceResponse.Message = "Cannot sync payment with Intuit!";
                                return serviceResponse;
                            }
                        }
                        else if (clients?.AccountingService_Id == MasterLookup._Services.Sage)
                        {
                            SageUtility sageUtility = new SageUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                            if (!paymentDetails.IsEdit)
                            {
                                string response = await sageUtility.CreatePaymentJournalEntry_Intacct(PaymentsTransaction, paymentDetails);
                                var serializer = new XmlSerializer(typeof(SageResponse));
                                using (var reader = new StringReader(response))
                                {
                                    createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                                }
                                if (createSageJournalResponse?.Operation?.Result?.Data?.GlBatch?.RecordNo == null)
                                {
                                    if (PaymentsTransaction?.Connection != null)
                                    {
                                        PaymentsTransaction.Rollback();
                                    }
                                    serviceResponse.Status = false;
                                    serviceResponse.Message = "Cannot sync payment with Sage!" + createSageJournalResponse?.Operation?.Result?.Errormessage?.Error?[0]?.Description2;
                                    return serviceResponse;
                                }
                            }
                            else
                            {
                                List<JournalEntries> journalEntries = _journalEntryLogRepository.GetJournalEntryByPayment(MasterLookup._Sources.Payment, paymentDetails.Payment_Id);

                                JournalEntries journalEntry = journalEntries?.FirstOrDefault(x => x.JournalIndex == 0);

                                if (journalEntry != null)
                                {
                                    var response = await sageUtility.UpdatePaymentJournalEntry_Intacct(PaymentsTransaction, paymentDetails, journalEntry);

                                    var parsed = ParseResponse(response);

                                    int? newBatchNo = parsed?.Operation?.Result?.Data?.GlBatch?.BatchNo;

                                    if (newBatchNo != null)
                                    {
                                        journalEntry.ExternalJournalEntryIdentifier = newBatchNo.Value.ToString();
                                        _journalEntryLogRepository.UpdateExternalId(PaymentsTransaction, journalEntry.JournalEntry_Id, newBatchNo.Value);
                                    }
                                }
                                else
                                {
                                    var response = await sageUtility.CreatePaymentJournalEntry_Intacct(PaymentsTransaction, paymentDetails, 0);
                                    var parsed = ParseResponse(response);

                                    int? batchNo = parsed?.Operation?.Result?.Data?.GlBatch?.BatchNo;

                                    if (batchNo != null && journalEntry != null)
                                    {
                                        journalEntry.ExternalJournalEntryIdentifier = batchNo.Value.ToString();
                                        _journalEntryLogRepository.InsertJournalEntries(PaymentsTransaction, journalEntry);
                                    }
                                }
                            }

                        }

                    }
                    PaymentsTransaction.Commit();
                    serviceResponse.Data = paymentDetails.Payment_Id;
                    serviceResponse.Status = true;
                }
            }
            catch (Exception ex)
            {
                if (PaymentsTransaction.Connection != null)
                {
                    PaymentsTransaction.Rollback();
                }
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
        public SageResponse ParseResponse(string xml)
        {
            var doc = XDocument.Parse(xml);

            return new SageResponse
            {
                Operation = new Operation
                {
                    Result = new Result
                    {
                        Status = doc.Descendants("result").FirstOrDefault()?.Element("status")?.Value,
                        Function = doc.Descendants("result").FirstOrDefault()?.Element("function")?.Value,
                        ControlId = doc.Descendants("result").FirstOrDefault()?.Element("controlid")?.Value,
                        Data = new Data
                        {
                            GlBatch = new GlBatch
                            {
                                BatchNo = int.TryParse(doc.Descendants("BATCHNO").FirstOrDefault()?.Value, out int batchNo) ? batchNo : 0,
                                RecordNo = int.TryParse(doc.Descendants("RECORDNO").FirstOrDefault()?.Value, out int recordNo) ? recordNo : 0
                            }
                        }
                    }
                }
            };
        }

        [HttpPost]
        public async Task<ServiceResponse> DeletePaymentDetails(long Payment_Id, bool IsCancel = false)
        {
            ServiceResponse serviceResponse = new ServiceResponse();

            SqlTransaction PaymentsTransaction = null;
            PaymentsTransaction = _baseRepository.GetNewTransaction();
            try
            {
                if (Payment_Id == 0)
                {
                    serviceResponse.Status = false;
                    return serviceResponse;
                }
                else
                {
                    List<PaymentApplicationLineItems> _paymentapplicationlineitems = _paymentsRepository.GetPaymentApplicationLineItemsByPayment_Id(Payment_Id);
                    if (_paymentapplicationlineitems != null && _paymentapplicationlineitems?.Count > 0)
                    {
                        foreach (var item in _paymentapplicationlineitems)
                        {
                            ServiceResponse response = UnApplyPayment(item, null, true);
                            if (!response.Status)
                            {
                                serviceResponse.Status = false;
                                serviceResponse.Message = $"Failed to unapply payment for this payment";
                                return serviceResponse;
                            }
                        }

                    }
                    Clients clients = _accountRepository.GetClients(userSession.Client_Id);
                    if (clients?.AccountingService_Id == MasterLookup._Services.Intuit)
                    {
                        QBOUtility qBOUtility = new QBOUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                        Payments_BillsMapping payments_BillsMapping = _payments_BillsMappingRepository.GetPayments_BillsMapping(Payment_Id);
                        if (payments_BillsMapping != null)
                        {
                            ExternalBillReference externalBillReference = _externalBillReferenceRepository.GetExternalBillReference(userSession.Client_Id, clients.AccountingService_Id, MasterLookup._Sources.Bill, payments_BillsMapping.Bill_Id);
                            if (!string.IsNullOrWhiteSpace(externalBillReference?.ExternalBillIdentifier))
                            {
                                Entities.External.QBO.Bill.ReadByIdRes readByIdRes = qBOUtility.ReadBillById(externalBillReference.ExternalBillIdentifier);
                                if (!string.IsNullOrWhiteSpace(readByIdRes?.Bill?.Id))
                                {
                                    qBOUtility.DeleteBill(readByIdRes);
                                    _externalBillReferenceRepository.InactivePaymentBill(Payment_Id, externalBillReference);
                                }
                            }
                        }
                        else
                        {
                            Payments payments = _paymentsRepository.GetPaymentDetails(Payment_Id);
                            List<JournalEntries> journalEntryList = _journalEntryLogRepository.GetJournalEntryByPayment(MasterLookup._Sources.Payment, payments.Payment_Id);
                            foreach (var item in journalEntryList)
                            {
                                DeleteJournalResponse deleteJournalRes = qBOUtility.DeleteJournalEntry(item);
                            }
                            if (journalEntryList != null)
                            {
                                _journalEntryLogRepository.DeleteJournalEntry(journalEntryList.FirstOrDefault().SourceType_Id, journalEntryList.FirstOrDefault().Source_Id);
                            }
                        }
                    }
                    else if (clients?.AccountingService_Id == MasterLookup._Services.Sage)
                    {
                        SageUtility sageUtility = new SageUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                        Payments_BillsMapping payments_BillsMapping = _payments_BillsMappingRepository.GetPayments_BillsMapping(Payment_Id);
                        if (payments_BillsMapping != null)
                        {
                            ExternalBillReference externalBillReference = _externalBillReferenceRepository.GetExternalBillReference(userSession.Client_Id, clients.AccountingService_Id, MasterLookup._Sources.Bill, payments_BillsMapping.Bill_Id);
                            if (!string.IsNullOrWhiteSpace(externalBillReference?.ExternalBillIdentifier))
                            {
                                SageBillResponse sageBillResponse = await sageUtility.ReadBillById_Intacct(externalBillReference.ExternalBillIdentifier);

                                if (sageBillResponse is not null || !string.IsNullOrWhiteSpace(sageBillResponse.RecordNo))
                                {
                                    await sageUtility.DeleteBill_Intacct(sageBillResponse.RecordNo);
                                    _externalBillReferenceRepository.InactivePaymentBill(Payment_Id, externalBillReference);
                                }

                            }
                        }
                        else
                        {
                            Payments payments = _paymentsRepository.GetPaymentDetails(Payment_Id);
                            if (payments != null)
                            {
                                List<JournalEntries> journalEntryList = _journalEntryLogRepository.GetJournalEntryByPayment(MasterLookup._Sources.Payment, payments.Payment_Id);
                                foreach (var item in journalEntryList)
                                {
                                    item.ReferenceNumber = payments.ReferenceNumber;
                                    var response = await sageUtility.DeleteJournalEntry_Intacct(item);
                                    if (response.Contains("failure"))
                                    {
                                        serviceResponse.Status = false;
                                        serviceResponse.Message = $"Failed to reverse journal entry";
                                        return serviceResponse;
                                    }
                                }

                                if (journalEntryList != null)
                                {
                                    _journalEntryLogRepository.DeleteJournalEntry(journalEntryList.FirstOrDefault().SourceType_Id, journalEntryList.FirstOrDefault().Source_Id);
                                }
                            }
                        }

                    }
                    if (IsCancel)
                    {
                        _paymentsRepository.UpdatePaymentStatus(PaymentsTransaction, Payment_Id, MasterLookup._PaymentStatus.Cancelled, clients.Client_Id);
                        _policyDetailsRepository.UpdateBillProcessQueueStatus(clients.Client_Id, Payment_Id, MasterLookup._BillProcessWorkFlowStatus.Cancelled, userSession.User_Id);
                    }
                    else
                    {
                        _paymentsRepository.DeletePaymentDetails(Payment_Id);
                        _policyDetailsRepository.UpdateBillProcessQueueStatus(clients.Client_Id, Payment_Id, MasterLookup._BillProcessWorkFlowStatus.Deleted, userSession.User_Id);
                    }
                    PaymentsTransaction.Commit();
                    serviceResponse.Status = true;
                }
            }
            catch (Exception ex)
            {
                if (PaymentsTransaction?.Connection != null)
                {
                    PaymentsTransaction.Rollback();
                }
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
        public IActionResult SelectedPaymentsTile(long Payment_Id, byte Direction_Id)
        {
            Payments payments = _paymentsRepository.GetPaymentDetails(Payment_Id);
            ViewBag.Direction_Id = Direction_Id;
            return PartialView(payments);
        }
        public IActionResult PartialPayment(long policyDetail_Id, long Invoice_Id, byte Direction_Id)
        {
            Invoices inv = _invoiceRepository.GetInvoicesByInvoice_Id(Invoice_Id);
            if (inv != null)
            {
                List<InvoiceDetails> List = _invoiceRepository.GetInvoiceDetailsByDirection_Id(Invoice_Id, Direction_Id);
                PolicyDetails policyDetails = _policyDetailsRepository.GetPolicyDetails(policyDetail_Id);
                AgentDetails agentDetails = _policyDetailsRepository.GetAgentDetails(policyDetails?.AgentDetail_Id ?? 0);
                InsuredDetails insuredDetails = _policyDetailsRepository.GetInsuredDetails(policyDetails?.InsuredDetail_Id ?? 0);
                inv.InvoicesDetailsList = List;
                inv.AgencyName = agentDetails.Name;
                inv.InsuredsName = insuredDetails?.Name;
                inv.PolicyNumber = policyDetails?.PolicyNumber;
                inv.Amount = List.Where(c => c.Invoice_Id == inv.Id)?.Sum(x => x.Amount) ?? 0.0;
                inv.RemainingBalance = List.Where(c => c.Invoice_Id == inv.Id)?.Sum(x => x.RemainingBalance) ?? 0.0;
                ViewBag.Direction_Id = Direction_Id;
            }
            else
            {
                return BadRequest("Invalid Request.");
            }
            return PartialView(inv);
        }
        //public ServiceResponse ApplyPayments(List<SelectedInvoiceDetailDTO> checkedInvoiceDetails, long Payment_Id, byte Direction_Id, bool IsOverride = false)
        //{
        //    ServiceResponse serviceResponse = new ServiceResponse();
        //    SqlTransaction PaymentsTransaction = null;
        //    double RemainingBalance = 0;
        //    bool Paid = false;
        //    DateTime? PaidOn = null;
        //    Clients clients = _accountRepository.GetClients(userSession.Client_Id);
        //    List<PaymentApplicationLineItems> PaymentApplicationLineItemsList = new List<PaymentApplicationLineItems>();
        //    PaymentsTransaction = _baseRepository.GetNewTransaction();
        //    if (PaymentsTransaction == null)
        //    {
        //        serviceResponse.Status = false;
        //        serviceResponse.Data = "Unable to initiate a Transaction for the process.";
        //        return serviceResponse;
        //    }
        //    try
        //    {
        //        if (Payment_Id == 0)
        //        {
        //            serviceResponse.Status = false;
        //            serviceResponse.Message = $"No payments were associated.";
        //            return serviceResponse;
        //        }
        //        else
        //        {
        //            if (checkedInvoiceDetails?.Count() == 0)
        //            {
        //                serviceResponse.Status = false;
        //                serviceResponse.Data = "Please select at least one full or partial invoice to apply payment";
        //                return serviceResponse;
        //            }
        //            else
        //            {
        //                Payments payments = _paymentsRepository.GetPaymentDetails(Payment_Id);
        //                double PaymentsRemainingAmount = payments.RemainingAmount;
        //                bool hasPositive = checkedInvoiceDetails.Any(x => x.Amount > 0);
        //                bool hasNegative = checkedInvoiceDetails.Any(x => x.Amount < 0);
        //                bool allPositive = hasPositive && !hasNegative;
        //                bool allNegative = hasNegative && !hasPositive;
        //                bool hasMixed = hasPositive && hasNegative;

        //                bool isAR = Direction_Id == MasterLookup._Directions.AR;
        //                bool isAP = Direction_Id == MasterLookup._Directions.AP;
        //                bool isIncoming = payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming;
        //                bool isOutgoing = payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing;
        //                if (
        //                    (allPositive && isAR && isOutgoing) ||  // AR: All positive amounts cannot be paid out with Outgoing Payment Direction
        //                    (allNegative && isAR && isIncoming) ||  // AR: All negative amounts cannot be received with Incoming Payment Direction
        //                                                            //(hasMixed && isAR && isOutgoing) || (REMOVED THIS CONDITION)    // AR: Mixed (positive and negative) amounts cannot be paid out with Outgoing Payment Direction

        //                    (allPositive && isAP && isIncoming) ||   // AP: All positive amounts cannot be received with Incoming Payment Direction
        //                    (allNegative && isAP && isOutgoing)    // AP: All negative amounts cannot be paid out with Outgoing Payment Direction
        //                                                           //(hasMixed && isAP && isIncoming)   (REMOVED THIS CONDITION)      // AP: Mixed positive and negative amounts cannot be received with Incoming Payment Direction
        //                    )
        //                {
        //                    serviceResponse.Status = false;
        //                    if (isAR)
        //                    {
        //                        if (isOutgoing)
        //                            serviceResponse.Message = "In AR, outgoing payments cannot have positive or mixed amounts.";
        //                        else
        //                            serviceResponse.Message = "In AR, incoming payments cannot have negative amounts.";
        //                    }
        //                    else if (isAP)
        //                    {
        //                        if (isIncoming)
        //                            serviceResponse.Message = "In AP, incoming payments cannot have positive or mixed amounts.";
        //                        else
        //                            serviceResponse.Message = "In AP, outgoing payments cannot have negative amounts.";
        //                    }
        //                    return serviceResponse;
        //                }
        //                ////////////////////////Removed this which was previosuly added for validaing Positive Nets now we are allowing Negative net amounts also


        //                //////if ((payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && checkedInvoiceDetails.Any(x => x.Amount > 0) && Direction_Id == MasterLookup._Directions.AP) || (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && checkedInvoiceDetails.Any(x => x.Amount > 0) && Direction_Id == MasterLookup._Directions.AR))
        //                //////{
        //                //////    serviceResponse.Status = false;
        //                //////    if (Direction_Id == MasterLookup._Directions.AR)
        //                //////    {
        //                //////        serviceResponse.Message = "Outgoing payments cannot have positive amounts selected.";
        //                //////    }
        //                //////    else
        //                //////    {
        //                //////        serviceResponse.Message = "Incoming payments cannot have positive amounts selected.";
        //                //////    }
        //                //////    return serviceResponse;
        //                //////}



        //                // 1. Determine the effective sign based on business rules
        //                // In AP Incoming or AR Outgoing, the amount is treated as negative in the UI
        //                bool isNegativeContext = (isIncoming && isAP) || (isOutgoing && isAR);
        //                decimal displaySign = isNegativeContext ? -1 : 1;

        //                // 2. Calculate what the user "sees" as the payment balance
        //                decimal effectivePaymentBalance = (decimal)payments.RemainingAmount * displaySign;

        //                // 3. Calculate what the user "sees" as the total they are trying to apply
        //                decimal effectiveAppliedTotal = checkedInvoiceDetails.Sum(x => (decimal)x.Amount);

        //                // 4. Validation logic
        //                // If positive context: Payment must be >= Applied (e.g., $100 >= $80)
        //                // If negative context: Payment must be >= Applied (e.g., -$80 >= -$100)
        //                // In negative numbers, -30,975.34 is GREATER than -30,975.35
        //                if (effectivePaymentBalance < effectiveAppliedTotal)
        //                {
        //                    serviceResponse.Status = false;
        //                    serviceResponse.Message = "The amount selected exceeds the payment amount.";
        //                    return serviceResponse;
        //                }
        //                var partialpaymentInvoiceList = checkedInvoiceDetails?.Where(c => c.IsPartial == true).GroupBy(c => c.Invoice_Id).ToList();
        //                var fullpaymentInvoiceList = checkedInvoiceDetails?.Where(c => c.IsPartial == false).GroupBy(c => c.Invoice_Id).ToList();
        //                if (partialpaymentInvoiceList?.Count() > 0)
        //                {
        //                    byte _AccountingType_Id = 0;
        //                    foreach (var item in partialpaymentInvoiceList)
        //                    {
        //                        long invoiceId = item.Key;
        //                        Invoices invoices = _invoiceRepository.GetInvoicesByInvoice_Id(invoiceId);
        //                        PolicyDetails policyDetails = _policyDetailsRepository.GetPolicyDetails(invoices.PolicyDetails_Id);
        //                        byte BillingType_Id = LookupUtility.GetAllBillingTypes().FirstOrDefault(x => x.Code == policyDetails.BillType)?.BillingType_Id ?? 0;
        //                        LU_Clients_AccountingTypes_BillingTypes _AccountingTypes_BillingTypes = _lookupRepository.GetClients_AccountingTypes_BillingTypes(clients.Client_Id, BillingType_Id);
        //                        if (_AccountingTypes_BillingTypes?.AccountingType_Id > 0)
        //                        {
        //                            _AccountingType_Id = _AccountingTypes_BillingTypes.AccountingType_Id;
        //                        }
        //                        else
        //                        {
        //                            _AccountingType_Id = clients.AccountingType_Id;
        //                        }
        //                        var invoiceLineItems = checkedInvoiceDetails?.Where(x => x.Invoice_Id == invoiceId)?.ToList();
        //                        decimal RemaingBalanceFromUI = invoiceLineItems?.Sum(x => Convert.ToDecimal(x.RemainingBalance)) ?? 0;
        //                        decimal TotalAmountChecked = 0;
        //                        decimal ActualAmount = 0;
        //                        if ((payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && invoiceLineItems.All(x => x.Amount < 0) && Direction_Id == MasterLookup._Directions.AP) || (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && invoiceLineItems.All(x => x.Amount < 0) && Direction_Id == MasterLookup._Directions.AR))
        //                        {
        //                            TotalAmountChecked = (decimal)invoiceLineItems.Sum(x => Convert.ToDecimal(Math.Abs(x.Amount)));
        //                        }
        //                        else
        //                        {
        //                            TotalAmountChecked = (decimal)invoiceLineItems.Sum(x => Convert.ToDecimal(x.Amount));
        //                        }
        //                        ActualAmount = (decimal)invoiceLineItems.Sum(x => Convert.ToDecimal(x.Amount));
        //                        if (Math.Sign(ActualAmount) != Math.Sign(RemaingBalanceFromUI))
        //                        {
        //                            serviceResponse.Status = false;
        //                            serviceResponse.Message = "Payable amount must have the same sign as the remaining balance.";
        //                            return serviceResponse;
        //                        }
        //                        PaymentApplications paymentApplications = new PaymentApplications();
        //                        paymentApplications.Payment_Id = Payment_Id;
        //                        paymentApplications.Invoice_Id = invoiceId;
        //                        paymentApplications.AppliedDate = DateTime.Now;
        //                        paymentApplications.TotalAppliedAmount = Convert.ToDouble(ActualAmount);
        //                        if (Direction_Id == MasterLookup._Directions.AR)
        //                        {
        //                            RemainingBalance = Math.Round(invoices.RemainingBalance - Convert.ToDouble(Math.Sign(invoices.RemainingBalance) * Math.Abs(TotalAmountChecked)), 2);
        //                            Paid = RemainingBalance == 0;
        //                            _invoiceRepository.UpdateBalanceInInvoices(PaymentsTransaction, invoiceId, RemainingBalance, Paid);
        //                        }
        //                        paymentApplications = _paymentsRepository.InsertUpdatePaymentApplications(PaymentsTransaction, paymentApplications);
        //                        if (paymentApplications.Payment_Id > 0)
        //                        {
        //                            double AllocatedRemainingAmount = (double)ActualAmount;
        //                            var SingleInvoiceLine = invoiceLineItems.FirstOrDefault();
        //                            var invoiceDetailsList = _invoiceRepository.GetInvoiceDetailsByDirection_Id(invoiceId, Direction_Id);
        //                            if (SingleInvoiceLine != null)
        //                            {
        //                                if (SingleInvoiceLine.InvoiceDetail_Ids?.Count() > 0)
        //                                {
        //                                    invoiceDetailsList = invoiceDetailsList.Where(x => SingleInvoiceLine.InvoiceDetail_Ids.Contains(x.Id) && x.RemainingBalance != 0).ToList();
        //                                }
        //                                else
        //                                {
        //                                    invoiceDetailsList = invoiceDetailsList.Where(x => x.RemainingBalance != 0).ToList();
        //                                }
        //                            }
        //                            var priorityBuckets = new List<List<int>>
        //                            {
        //                                new List<int> { MasterLookup._LineItemTypes.Tax },                                    // Tier 1
        //                                new List<int> { MasterLookup._LineItemTypes.Fee, MasterLookup._LineItemTypes.CarrierFee }, // Tier 2
        //                                new List<int> { MasterLookup._LineItemTypes.Commission, MasterLookup._LineItemTypes.Premium } // Tier 3
        //                            };

        //                            foreach (var bucket in priorityBuckets)
        //                            {
        //                                // Filter items in the current tier. We allow items with 0 balance 
        //                                // in the last bucket to catch any remaining 'excess' over-application.
        //                                bool isLastBucket = bucket.Contains(MasterLookup._LineItemTypes.Premium);
        //                                var currentTierItems = invoiceDetailsList
        //                                    .Where(x => bucket.Contains(x.LineItemType_Id) && (isLastBucket || x.RemainingBalance != 0))
        //                                    .ToList();

        //                                if (currentTierItems.Count == 0) continue;

        //                                var allocationMap = new Dictionary<long, double>();
        //                                double totalTierBalanceMagnitude = Math.Abs(currentTierItems.Sum(x => x.RemainingBalance));

        //                                // Determine how much of the current 'AllocatedRemainingAmount' to use for this tier.
        //                                // The last bucket takes everything left to ensure total application matches payment.
        //                                double amountForThisTier = isLastBucket ? AllocatedRemainingAmount : Math.Min(AllocatedRemainingAmount, totalTierBalanceMagnitude);

        //                                if (amountForThisTier == 0 && AllocatedRemainingAmount == 0) continue;

        //                                // --- DISTRIBUTION LOGIC ---
        //                                if (amountForThisTier > totalTierBalanceMagnitude && totalTierBalanceMagnitude > 0)
        //                                {
        //                                    // CASE A: OVER-APPLICATION (Excess exists in this tier)
        //                                    // 1. Pay off current balances to zero
        //                                    foreach (var currentItem in currentTierItems)
        //                                    {
        //                                        allocationMap[currentItem.Id] = Math.Abs(currentItem.RemainingBalance);
        //                                    }

        //                                    // 2. Distribute excess evenly across all items in the tier
        //                                    double excess = Math.Round(amountForThisTier - totalTierBalanceMagnitude, 2);
        //                                    double evenShare = Math.Floor((excess / currentTierItems.Count) * 100) / 100;
        //                                    double dust = Math.Round(excess - (evenShare * currentTierItems.Count), 2);

        //                                    foreach (var cItem in currentTierItems)
        //                                    {
        //                                        allocationMap[cItem.Id] = Math.Round(allocationMap[cItem.Id] + evenShare, 2);
        //                                    }
        //                                    allocationMap[currentTierItems.First().Id] = Math.Round(allocationMap[currentTierItems.First().Id] + dust, 2);
        //                                }
        //                                else if (bucket.Contains(MasterLookup._LineItemTypes.Commission) && amountForThisTier < totalTierBalanceMagnitude)
        //                                {
        //                                    // CASE B: PROPORTIONAL (Standard logic for Commission/Premium)
        //                                    double distributedInTier = 0;
        //                                    for (int i = 0; i < currentTierItems.Count; i++)
        //                                    {
        //                                        var ctItem = currentTierItems[i];
        //                                        if (i == currentTierItems.Count - 1)
        //                                        {
        //                                            allocationMap[ctItem.Id] = Math.Round(amountForThisTier - distributedInTier, 2);
        //                                        }
        //                                        else
        //                                        {
        //                                            double share = Math.Round((Math.Abs(ctItem.RemainingBalance) / totalTierBalanceMagnitude) * amountForThisTier, 2);
        //                                            allocationMap[ctItem.Id] = share;
        //                                            distributedInTier += share;
        //                                        }
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    // CASE C: WATERFILL (Standard logic for Tax/Fees)
        //                                    double pool = amountForThisTier;
        //                                    var itemsToPay = currentTierItems.ToList();
        //                                    while (pool > 0 && itemsToPay.Count > 0)
        //                                    {
        //                                        double share = Math.Floor((pool / itemsToPay.Count) * 100) / 100;
        //                                        if (share <= 0) // Handle tiny remainders
        //                                        {
        //                                            var topItem = itemsToPay.OrderByDescending(x => Math.Abs(x.RemainingBalance)).First();
        //                                            allocationMap[topItem.Id] = Math.Round((allocationMap.ContainsKey(topItem.Id) ? allocationMap[topItem.Id] : 0) + pool, 2);
        //                                            break;
        //                                        }

        //                                        var completedThisPass = new List<InvoiceDetails>();
        //                                        foreach (var itemToPay in itemsToPay)
        //                                        {
        //                                            double currentApp = allocationMap.ContainsKey(itemToPay.Id) ? allocationMap[itemToPay.Id] : 0;
        //                                            double needed = Math.Abs(itemToPay.RemainingBalance) - currentApp;
        //                                            double toApply = Math.Min(share, needed);

        //                                            allocationMap[itemToPay.Id] = Math.Round(currentApp + toApply, 2);
        //                                            pool = Math.Round(pool - toApply, 2);
        //                                            if (Math.Round(needed - toApply, 2) <= 0) completedThisPass.Add(itemToPay);
        //                                        }
        //                                        itemsToPay.RemoveAll(x => completedThisPass.Contains(x));
        //                                        if (completedThisPass.Count == 0) break;
        //                                    }
        //                                }

        //                                // --- EXECUTION: Apply to DB ---
        //                                foreach (var lineitem in currentTierItems)
        //                                {
        //                                    double appliedMagnitude = allocationMap.ContainsKey(lineitem.Id) ? allocationMap[lineitem.Id] : 0;
        //                                    if (appliedMagnitude == 0 && !isLastBucket) continue;

        //                                    // Use the sign of the ActualAmount (from the UI/Payment context) 
        //                                    // to move the balance correctly (positive vs negative application)
        //                                    double signedApply = appliedMagnitude;

        //                                    // Calculate new balance: Current Balance - Applied Amount
        //                                    // Example: $500 - $700 = -$200 (Over-app) | -$200 - (-$200) = 0 (Netting)
        //                                    RemainingBalance = Math.Round(lineitem.RemainingBalance - signedApply, 2);



        //                                    PaymentApplicationLineItems paymentApplicationLineItems = new PaymentApplicationLineItems();
        //                                    paymentApplicationLineItems.PaymentApplication_Id = paymentApplications.PaymentApplication_Id;
        //                                    paymentApplicationLineItems.InvoiceLineItem_Id = lineitem.Id;
        //                                    paymentApplicationLineItems.AppliedAmount = signedApply;
        //                                    paymentApplicationLineItems.Is_SplitPayment = false;
        //                                    paymentApplicationLineItems.SubType_Id = null;
        //                                    paymentApplicationLineItems.AccountingType_Id = _AccountingType_Id;
        //                                    _paymentsRepository.InsertUpdatePaymentApplicationLineItems(PaymentsTransaction, paymentApplicationLineItems);
        //                                    RemainingBalance = Math.Round(lineitem.RemainingBalance - Convert.ToDouble(Math.Sign(lineitem.RemainingBalance) * Math.Abs(signedApply)), 2);
        //                                    lineitem.RemainingBalance = RemainingBalance;
        //                                    Paid = RemainingBalance == 0;
        //                                    PaidOn = Paid == true ? DateTime.Now : null;
        //                                    _invoiceRepository.UpdateBalanceInInvoiceDetails(PaymentsTransaction, lineitem.Id, RemainingBalance, Paid);
        //                                    PaymentApplicationLineItemsList.Add(paymentApplicationLineItems);

        //                                    lineitem.RemainingBalance = RemainingBalance;
        //                                    AllocatedRemainingAmount = Math.Round(AllocatedRemainingAmount - appliedMagnitude, 2);


        //                                }
        //                            }
        //                        }
        //                        //////////PaymentsRemainingAmount = Math.Round(PaymentsRemainingAmount - Convert.ToDouble(TotalAmountChecked), 2);
        //                        //////////_paymentsRepository.UpdatePaymentRemainingAmount(PaymentsTransaction, Payment_Id, PaymentsRemainingAmount);
        //                        ///

        //                        // Get the signed total for this specific invoice
        //                        decimal signedInvoiceTotal = ActualAmount;

        //                        // Apply the deduction to the signed payment balance
        //                        double currentSignedPayment = PaymentsRemainingAmount * (double)displaySign;
        //                        if (Math.Sign(currentSignedPayment) != Math.Sign(signedInvoiceTotal) && isNegativeContext)
        //                        {
        //                            if (isAP)
        //                            {
        //                                if (currentSignedPayment < 0)
        //                                {
        //                                    currentSignedPayment = Math.Sign(signedInvoiceTotal) * currentSignedPayment;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                currentSignedPayment = Math.Sign(signedInvoiceTotal) * currentSignedPayment;
        //                            }
        //                        }
        //                        double updatedSignedPayment = currentSignedPayment - (double)signedInvoiceTotal;

        //                        // Update DB with the magnitude
        //                        PaymentsRemainingAmount = Math.Abs(Math.Round(updatedSignedPayment, 2));
        //                        if (isAR)
        //                        {
        //                            if (currentSignedPayment < 0)
        //                            {
        //                                PaymentsRemainingAmount = -PaymentsRemainingAmount;
        //                            }
        //                        }
        //                        _paymentsRepository.UpdatePaymentRemainingAmount(PaymentsTransaction, Payment_Id, PaymentsRemainingAmount);
        //                    }
        //                }

        //                if (fullpaymentInvoiceList?.Count() > 0)
        //                {
        //                    byte _AccountingType_Id = 0;
        //                    foreach (var item in fullpaymentInvoiceList)
        //                    {
        //                        long invoiceId = item.Key;
        //                        Invoices invoices = _invoiceRepository.GetInvoicesByInvoice_Id(invoiceId);
        //                        PolicyDetails policyDetails = _policyDetailsRepository.GetPolicyDetails(invoices.PolicyDetails_Id);
        //                        byte BillingType_Id = LookupUtility.GetAllBillingTypes().FirstOrDefault(x => x.Code == policyDetails.BillType)?.BillingType_Id ?? 0;
        //                        LU_Clients_AccountingTypes_BillingTypes _AccountingTypes_BillingTypes = _lookupRepository.GetClients_AccountingTypes_BillingTypes(clients.Client_Id, BillingType_Id);
        //                        if (_AccountingTypes_BillingTypes?.AccountingType_Id > 0)
        //                        {
        //                            _AccountingType_Id = _AccountingTypes_BillingTypes.AccountingType_Id;
        //                        }
        //                        else
        //                        {
        //                            _AccountingType_Id = clients.AccountingType_Id;
        //                        }
        //                        var invoiceLineItems = checkedInvoiceDetails?.Where(x => x.Invoice_Id == invoiceId)?.ToList();
        //                        decimal RemaingBalanceFromUI = invoiceLineItems?.Sum(x => Convert.ToDecimal(x.RemainingBalance)) ?? 0;
        //                        decimal TotalAmountChecked = 0;
        //                        decimal ActualAmount = 0;
        //                        //if ((payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming && invoiceLineItems.All(x => x.Amount < 0) && Direction_Id == MasterLookup._Directions.AP) || (payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing && invoiceLineItems.All(x => x.Amount < 0) && Direction_Id == MasterLookup._Directions.AR))
        //                        //{
        //                        //    TotalAmountChecked = (decimal)invoiceLineItems.Sum(x => Convert.ToDecimal(Math.Abs(x.Amount)));
        //                        //}
        //                        //else
        //                        //{
        //                        //    TotalAmountChecked = (decimal)invoiceLineItems.Sum(x => Convert.ToDecimal(x.Amount));
        //                        //}
        //                        TotalAmountChecked = (decimal)Math.Abs(invoiceLineItems?.Sum(x => Convert.ToDecimal(x.Amount)) ?? 0);
        //                        ActualAmount = (decimal)invoiceLineItems.Sum(x => Convert.ToDecimal(x.Amount));
        //                        if (Math.Sign(ActualAmount) != Math.Sign(RemaingBalanceFromUI))
        //                        {
        //                            serviceResponse.Status = false;
        //                            serviceResponse.Message = "Payable amount must have the same sign as the remaining balance.";
        //                            return serviceResponse;
        //                        }
        //                        PaymentApplications paymentApplications = new PaymentApplications();
        //                        paymentApplications.Payment_Id = Payment_Id;
        //                        paymentApplications.Invoice_Id = invoiceId;
        //                        paymentApplications.AppliedDate = DateTime.Now;
        //                        paymentApplications.TotalAppliedAmount = Convert.ToDouble(ActualAmount);

        //                        if (Direction_Id == MasterLookup._Directions.AR)
        //                        {
        //                            RemainingBalance = Math.Round(invoices.RemainingBalance - Convert.ToDouble(Math.Sign(invoices.RemainingBalance) * Math.Abs(TotalAmountChecked)), 2);
        //                            Paid = RemainingBalance == 0;
        //                            _invoiceRepository.UpdateBalanceInInvoices(PaymentsTransaction, invoiceId, RemainingBalance, Paid);
        //                        }
        //                        paymentApplications = _paymentsRepository.InsertUpdatePaymentApplications(PaymentsTransaction, paymentApplications);
        //                        if (paymentApplications.Payment_Id > 0)
        //                        {
        //                            var invoiceDetailsList = checkedInvoiceDetails?.Where(x => x.Invoice_Id == invoiceId);
        //                            if (invoiceDetailsList?.Count() > 0)
        //                            {
        //                                foreach (var invoice in invoiceDetailsList)
        //                                {
        //                                    if (invoice.RemainingBalance != 0)
        //                                    {
        //                                        InvoiceDetails invoiceDetails = _invoiceRepository.GetInvoiceDetailsByInvoiceDetail_Id(invoice.InvoiceDetail_Id);
        //                                        if ((invoiceDetails.EntityType_Id != payments.EntityType_Id || invoiceDetails.Entity_Id != payments.Entity_Id) && !IsOverride)
        //                                        {
        //                                            if (PaymentsTransaction.Connection != null)
        //                                            {
        //                                                PaymentsTransaction.Rollback();
        //                                            }
        //                                            serviceResponse.Status = false;
        //                                            serviceResponse.Message = "Payment entity type must match the invoice entity type.";
        //                                            serviceResponse.Data = new { EntityMismatch = true };
        //                                            return serviceResponse;
        //                                        }
        //                                        PaymentApplicationLineItems paymentApplicationLineItems = new PaymentApplicationLineItems();
        //                                        paymentApplicationLineItems.PaymentApplication_Id = paymentApplications.PaymentApplication_Id;
        //                                        paymentApplicationLineItems.InvoiceLineItem_Id = invoice.InvoiceDetail_Id;
        //                                        paymentApplicationLineItems.AppliedAmount = invoice.Amount;
        //                                        paymentApplicationLineItems.Is_SplitPayment = false;
        //                                        paymentApplicationLineItems.AccountingType_Id = _AccountingType_Id;
        //                                        _paymentsRepository.InsertUpdatePaymentApplicationLineItems(PaymentsTransaction, paymentApplicationLineItems);
        //                                        RemainingBalance = Math.Round(invoice.RemainingBalance - Convert.ToDouble(Math.Sign(invoice.RemainingBalance) * Math.Abs(invoice.Amount)), 2);
        //                                        Paid = RemainingBalance == 0;
        //                                        PaidOn = Paid == true ? DateTime.Now : null;
        //                                        _invoiceRepository.UpdateBalanceInInvoiceDetails(PaymentsTransaction, invoice.InvoiceDetail_Id, RemainingBalance, Paid);
        //                                        PaymentApplicationLineItemsList.Add(paymentApplicationLineItems);
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        ////////PaymentsRemainingAmount = Math.Round(PaymentsRemainingAmount - Convert.ToDouble(TotalAmountChecked), 2);
        //                        ////////_paymentsRepository.UpdatePaymentRemainingAmount(PaymentsTransaction, Payment_Id, PaymentsRemainingAmount);
        //                        ///

        //                        // Get the signed total for this specific invoice
        //                        decimal signedInvoiceTotal = ActualAmount;

        //                        // Apply the deduction to the signed payment balance
        //                        double currentSignedPayment = PaymentsRemainingAmount * (double)displaySign;
        //                        if (Math.Sign(currentSignedPayment) != Math.Sign(signedInvoiceTotal) && isNegativeContext)
        //                        {
        //                            if (isAP)
        //                            {
        //                                if (currentSignedPayment < 0)
        //                                {
        //                                    currentSignedPayment = Math.Sign(signedInvoiceTotal) * currentSignedPayment;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                currentSignedPayment = Math.Sign(signedInvoiceTotal) * currentSignedPayment;
        //                            }
        //                        }
        //                        double updatedSignedPayment = currentSignedPayment - (double)signedInvoiceTotal;

        //                        // Update DB with the magnitude
        //                        PaymentsRemainingAmount = (double)Math.Abs(Math.Round(updatedSignedPayment, 2));
        //                        if (isAR)
        //                        {
        //                            if (currentSignedPayment < 0)
        //                            {
        //                                PaymentsRemainingAmount = -PaymentsRemainingAmount;
        //                            }
        //                        }
        //                        _paymentsRepository.UpdatePaymentRemainingAmount(PaymentsTransaction, Payment_Id, PaymentsRemainingAmount);
        //                    }
        //                }
        //                if (PaymentApplicationLineItemsList.Count() > 0)
        //                {
        //                    List<JournalEntries> journalEntryList = _journalEntryLogRepository.GetJournalEntryByPayment(MasterLookup._Sources.Payment, payments.Payment_Id);
        //                    JournalEntries journalEntry = journalEntryList?.OrderByDescending(x => x.JournalIndex)?.FirstOrDefault();

        //                    if (clients.AccountingService_Id == MasterLookup._Services.Intuit)
        //                    {
        //                        QBOUtility qBOUtility = new QBOUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);

        //                        //UpdateJournalRes updateJournalRes = qBOUtility.UpdateJournalEntry(PaymentsTransaction, payments, PaymentApplicationLineItemsList, journalEntries, true);
        //                        CreateJournalRes createJournalRes = qBOUtility.CreateJournalEntry(PaymentsTransaction, payments, PaymentApplicationLineItemsList, journalEntry, true);
        //                        if (createJournalRes != null && createJournalRes.JournalEntry?.Id != null)
        //                        {
        //                            serviceResponse.Status = true;
        //                            serviceResponse.Message = "Applied payment successfully.";
        //                            PaymentsTransaction.Commit();
        //                            return serviceResponse;
        //                        }
        //                        else
        //                        {
        //                            if (PaymentsTransaction.Connection != null)
        //                            {
        //                                PaymentsTransaction.Rollback();
        //                            }
        //                            serviceResponse.Status = false;
        //                            serviceResponse.Message = createJournalRes?.Fault?.Error?[0]?.Detail != null ? $"[QBO Error]: {createJournalRes.Fault.Error[0].Detail}" : "Unable to process request";
        //                            return serviceResponse;
        //                        }
        //                    }
        //                    else if (clients.AccountingService_Id == MasterLookup._Services.Sage)
        //                    {
        //                        SageUtility sageUtility = new SageUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
        //                        var createJournalRes = Task.Run(() =>
        //                        sageUtility.CreateJournalEntry_Intacct(
        //                            PaymentsTransaction,
        //                            payments,
        //                            PaymentApplicationLineItemsList,
        //                            journalEntry,
        //                            true
        //                        )
        //                    ).Result;
        //                        var serializer = new XmlSerializer(typeof(SageResponse));
        //                        SageResponse createSageJournalResponse = new SageResponse();
        //                        using (var reader = new StringReader(createJournalRes))
        //                        {
        //                            createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
        //                        }
        //                        if (createSageJournalResponse != null && createSageJournalResponse?.Operation.Result?.Data?.GlBatch?.RecordNo != null)
        //                        {
        //                            serviceResponse.Status = true;
        //                            serviceResponse.Message = $"Successfully applied Payment (Payment ID: {payments.Payment_Id}).";
        //                            PaymentsTransaction.Commit();
        //                            return serviceResponse;
        //                        }
        //                        else
        //                        {
        //                            if (PaymentsTransaction.Connection != null)
        //                            {
        //                                PaymentsTransaction.Rollback();
        //                            }
        //                            serviceResponse.Status = false;
        //                            serviceResponse.Message = "Unable to process request.";
        //                            return serviceResponse;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        PaymentsTransaction.Commit();
        //                    }
        //                }
        //            }
        //            serviceResponse.Status = true;
        //            serviceResponse.Message = "Applied payment successfully.";
        //            return serviceResponse;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        if (PaymentsTransaction.Connection != null)
        //        {
        //            PaymentsTransaction.Rollback();
        //        }
        //        serviceResponse.Status = false;
        //        serviceResponse.Message = ex.Message;
        //    }
        //    return serviceResponse;

        //}

        public ServiceResponse ApplyPayments(List<SelectedInvoiceDetailDTO> checkedInvoiceDetails, long Payment_Id, byte Direction_Id, bool IsOverride = false, bool AllowCredit = false, bool AllowInvoiceCredit = false)
        {
            ServiceResponse serviceResponse = new ServiceResponse();

            double RemainingBalance = 0;
            bool Paid = false;
            DateTime? PaidOn = null;
            Clients clients = _accountRepository.GetClients(userSession.Client_Id);
            List<PaymentApplicationLineItems> PaymentApplicationLineItemsList = new List<PaymentApplicationLineItems>();

            try
            {
                if (Payment_Id == 0)
                {
                    serviceResponse.Status = false;
                    serviceResponse.Message = $"No payments were associated.";
                    return serviceResponse;
                }
                else
                {
                    if (checkedInvoiceDetails?.Count() == 0)
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Data = "Please select at least one full or partial invoice to apply payment";
                        return serviceResponse;
                    }
                    else
                    {
                        Payments payments = _paymentsRepository.GetPaymentDetails(Payment_Id);
                        //checkedInvoiceDetails = PrepareInvoicePayload(checkedInvoiceDetails);
                        double PaymentsRemainingAmount = payments.RemainingAmount;
                        bool hasPositive = checkedInvoiceDetails.Any(x => x.Amount > 0);
                        bool hasNegative = checkedInvoiceDetails.Any(x => x.Amount < 0);
                        bool allPositive = hasPositive && !hasNegative;
                        bool allNegative = hasNegative && !hasPositive;
                        bool hasMixed = hasPositive && hasNegative;

                        bool isAR = Direction_Id == MasterLookup._Directions.AR;
                        bool isAP = Direction_Id == MasterLookup._Directions.AP;
                        bool isIncoming = payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Incoming;
                        bool isOutgoing = payments.PaymentDirection_Id == MasterLookup._PaymentDirections.Outgoing;
                        double netAmount = (double)checkedInvoiceDetails.Sum(x => Convert.ToDecimal(x.Amount));
                        bool isNetPositive = netAmount > 0;
                        bool isNetNegative = netAmount < 0;
                        if (((isAR && isNetNegative && isIncoming) || (isAR && isNetNegative && isIncoming) || (isAP && isNetNegative && isOutgoing) || (isAP && isNetPositive && isIncoming)) && !AllowCredit)
                        {
                            serviceResponse.Status = false;
                            serviceResponse.Message = "Credit will make remaining amount exceed the original payment amount. Do you want to continue?";
                            serviceResponse.Data = new { AllowCredit = true };
                            return serviceResponse;
                        }
                        bool isNegativeContext = (isIncoming && isAP) || (isOutgoing && isAR);
                        decimal displaySign = isNegativeContext ? -1 : 1;
                        decimal effectivePaymentBalance = (decimal)payments.RemainingAmount;
                        decimal effectiveAppliedTotal = checkedInvoiceDetails.Sum(x => (decimal)x.Amount) * displaySign;
                        if (effectivePaymentBalance < effectiveAppliedTotal)
                        {
                            serviceResponse.Status = false;
                            serviceResponse.Message = "The amount selected exceeds the payment amount.";
                            return serviceResponse;
                        }

                        var allInvoiceDetailIds = checkedInvoiceDetails?.Where(x => x.InvoiceDetail_Ids != null && x.InvoiceDetail_Ids?.Any() == true)?
     .SelectMany(x => x.InvoiceDetail_Ids)?
     .Distinct()?
     .ToList();
                        string invoiceDetailIds =
    string.Join(",", allInvoiceDetailIds);
                        bool IsEntityMismatch = _invoiceRepository.ValidateInvoiceEntityMismatch(invoiceDetailIds, payments.Entity_Id, (payments.EntityType_Id ?? 0));

                        if (IsEntityMismatch && !IsOverride)
                        {
                            serviceResponse.Status = false;
                            serviceResponse.Message = "Payment entity type must match the invoice entity type.";
                            serviceResponse.Data = new
                            {
                                EntityMismatch = true
                            };

                            return serviceResponse;
                        }
                        bool invoiceOverApplied = checkedInvoiceDetails?.Any(x =>
    Math.Abs(x.Amount) > Math.Abs(x.RemainingBalance)) == true;

                        if (invoiceOverApplied && !AllowInvoiceCredit)
                        {
                            serviceResponse.Status = false;

                            serviceResponse.Message =
                                "You are about to apply more than the original amount on this invoice. Only do so if you intend to have a credit on this invoice.";

                            serviceResponse.Data = new
                            {
                                AllowInvoiceCredit = true
                            };

                            return serviceResponse;
                        }
                        if (clients.AccountingService_Id > 0)
                        {
                            List<Accounting.Entities.External.QBO.Journal.Error> errors = _paymentsRepository.ValidateInvoiceCOAConfiguration(clients.Client_Id, invoiceDetailIds, clients.AccountingService_Id, Payment_Id);
                            if (errors?.Count > 0)
                            {
                                var groupedErrors = errors
        .GroupBy(x => x.Message)
        .Select(g =>
            $"{g.Key}:{Environment.NewLine}" +
            string.Join(
                Environment.NewLine,
                g.Select(e => $"• {e.Detail}")
            )
        );

                                serviceResponse.Status = false;

                                serviceResponse.Message =
                                    "One or more invoice line items are not properly configured in the Chart of Accounts."
                                    + Environment.NewLine + Environment.NewLine
                                    + string.Join(Environment.NewLine + Environment.NewLine, groupedErrors);

                                return serviceResponse;
                            }
                        }
                        try
                        {
                            string PostJson = JsonConvert.SerializeObject(checkedInvoiceDetails);
                            bool IsNegative = (checkedInvoiceDetails.Sum(x => Convert.ToDecimal(x.Amount)) < 0 && ((isAR && isOutgoing) || (isAP && isIncoming)));
                            PaymentApplicationLineItemsList = _paymentsRepository.ApplyPayments(clients.Client_Id, PostJson, Payment_Id, Direction_Id, IsNegative);
                        }
                        catch (Exception ex)
                        {
                            serviceResponse.Status = false;
                            serviceResponse.Message = ex.Message;
                            return serviceResponse;
                        }
                        SqlTransaction PaymentsTransaction = _baseRepository.GetNewTransaction();
                        //    if (PaymentsTransaction == null)
                        //    {
                        //        serviceResponse.Status = false;
                        //        serviceResponse.Data = "Unable to initiate a Transaction for the process.";
                        //        return serviceResponse;
                        //    }
                        try
                        {
                            if (PaymentApplicationLineItemsList.Count() > 0)
                            {
                                string paymentApplicationLineitemJson = JsonConvert.SerializeObject(PaymentApplicationLineItemsList.Select(x => new { PaymentApplicationLineItem_Id = x.PaymentApplicationLineItem_Id }).ToList());
                                List<JournalEntries> journalEntryList = _journalEntryLogRepository.GetJournalEntryByPayment(MasterLookup._Sources.Payment, payments.Payment_Id);
                                JournalEntries journalEntry = journalEntryList?.OrderByDescending(x => x.JournalIndex)?.FirstOrDefault();

                                if (clients.AccountingService_Id == MasterLookup._Services.Intuit)
                                {
                                    QBOUtility qBOUtility = new QBOUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);

                                    //UpdateJournalRes updateJournalRes = qBOUtility.UpdateJournalEntry(PaymentsTransaction, payments, PaymentApplicationLineItemsList, journalEntries, true);
                                    CreateJournalRes createJournalRes = qBOUtility.CreateJournalEntry(PaymentsTransaction, payments, PaymentApplicationLineItemsList, journalEntry, true);
                                    if (createJournalRes != null && createJournalRes.JournalEntry?.Id != null)
                                    {
                                        serviceResponse.Status = true;
                                        serviceResponse.Message = "Applied payment successfully.";
                                        PaymentsTransaction.Commit();
                                        return serviceResponse;
                                    }
                                    else
                                    {
                                        if (PaymentsTransaction.Connection != null)
                                        {
                                            PaymentsTransaction.Rollback();
                                        }

                                        string Result = _paymentsRepository.UnApplyPayments(clients.Client_Id, Direction_Id, paymentApplicationLineitemJson);
                                        serviceResponse.Status = false;
                                        serviceResponse.Message = createJournalRes?.Fault?.Error?[0]?.Detail != null ? $"[QBO Error]: {createJournalRes.Fault.Error[0].Detail}" : $"Unable to sync to Intuit! {Result}";
                                        return serviceResponse;
                                    }
                                }
                                else if (clients.AccountingService_Id == MasterLookup._Services.Sage)
                                {
                                    SageUtility sageUtility = new SageUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                                    var createJournalRes = Task.Run(() =>
                                    sageUtility.CreateJournalEntry_Intacct(
                                        PaymentsTransaction,
                                        payments,
                                        PaymentApplicationLineItemsList,
                                        journalEntry,
                                        true
                                    )
                                ).Result;
                                    var serializer = new XmlSerializer(typeof(SageResponse));
                                    SageResponse createSageJournalResponse = new SageResponse();
                                    using (var reader = new StringReader(createJournalRes))
                                    {
                                        createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                                    }
                                    if (createSageJournalResponse != null && createSageJournalResponse?.Operation.Result?.Data?.GlBatch?.RecordNo != null)
                                    {
                                        serviceResponse.Status = true;
                                        serviceResponse.Message = $"Successfully applied Payment (Payment ID: {payments.Payment_Id}).";
                                        PaymentsTransaction.Commit();
                                        return serviceResponse;
                                    }
                                    else
                                    {
                                        if (PaymentsTransaction.Connection != null)
                                        {
                                            PaymentsTransaction.Rollback();
                                        }
                                        string Result = _paymentsRepository.UnApplyPayments(clients.Client_Id, Direction_Id, paymentApplicationLineitemJson);
                                        serviceResponse.Status = false;
                                        serviceResponse.Message = $"Unable to sync to Sage. {Result}";
                                        return serviceResponse;
                                    }
                                }
                                else
                                {
                                    PaymentsTransaction.Commit();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (PaymentsTransaction.Connection != null)
                            {
                                PaymentsTransaction.Rollback();
                            }
                        }

                    }
                    serviceResponse.Status = true;
                    serviceResponse.Message = "Applied payment successfully.";
                    return serviceResponse;
                }

            }
            catch (Exception ex)
            {

                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;

        }
        [HttpPost]
        public ServiceResponse UnApplyPayments(List<SelectedInvoiceDetailDTO> checkedInvoiceDetails)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                if (checkedInvoiceDetails?.Count() > 0)
                {
                    var allInvoiceList = checkedInvoiceDetails?.GroupBy(c => c.Invoice_Id)?.ToList();
                    if (allInvoiceList.Count() > 0)
                    {
                        var unappliedAmountByPayment = new Dictionary<long, double>();
                        foreach (var invoicedetails in checkedInvoiceDetails)
                        {
                            var paymentAppLineItem = _paymentsRepository.GetPaymentApplicationLineItemsByInvoiceDetail_Id(invoicedetails.InvoiceDetail_Id);
                            if (paymentAppLineItem?.Count() > 0)
                            {
                                foreach (var lineitem in paymentAppLineItem)
                                {
                                    var paymentApp = _paymentsRepository.GetPaymentApplicationsByPaymentApplication_Id(lineitem.PaymentApplication_Id);
                                    long paymentId = paymentApp.Payment_Id;
                                    if (!unappliedAmountByPayment.ContainsKey(paymentId))
                                        unappliedAmountByPayment[paymentId] = 0;
                                    unappliedAmountByPayment[paymentId] += lineitem.AppliedAmount;
                                }
                            }
                        }
                        //foreach (var unapplypayment in unappliedAmountByPayment)
                        //{
                        //    long paymentId = unapplypayment.Key;
                        //    double totalUnappliedAmount = unapplypayment.Value;
                        //    var payment = _paymentsRepository.GetPaymentDetails(paymentId);
                        //    if (payment == null)
                        //    {
                        //        serviceResponse.Status = false;
                        //        serviceResponse.Message = $"Payment ID {paymentId} is does not exist.";
                        //        return serviceResponse;
                        //    }
                        //    double totalPaymentAfterUnapply = Math.Abs(Math.Round((payment.RemainingAmount + totalUnappliedAmount), 2));
                        //    if (totalPaymentAfterUnapply > payment.Amount || totalPaymentAfterUnapply < 0)
                        //    {
                        //        serviceResponse.Status = false;
                        //        serviceResponse.Message = "Unapplying payment will cause payment to be overapplied due to netted amount";
                        //        return serviceResponse;
                        //    }
                        //}

                        foreach (var item in checkedInvoiceDetails)
                        {
                            try
                            {
                                List<PaymentApplicationLineItems> paymentApplicationLineItems = _paymentsRepository.GetPaymentApplicationLineItemsByInvoiceDetail_Id(item.InvoiceDetail_Id);
                                if (paymentApplicationLineItems != null && paymentApplicationLineItems?.Count > 0)
                                {
                                    foreach (var unapplyitem in paymentApplicationLineItems)
                                    {
                                        ServiceResponse unApplyserviceResponse = new ServiceResponse();
                                        unApplyserviceResponse = UnApplyPayment(unapplyitem);
                                        if (!unApplyserviceResponse.Status)
                                        {
                                            return unApplyserviceResponse;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                serviceResponse.Status = false;
                                serviceResponse.Message = ex.Message;
                            }
                        }
                        Clients clients = _accountRepository.GetClients(userSession.Client_Id);
                        serviceResponse.Status = true;
                        serviceResponse.Message = "Un Applied payment successfully.";
                        return serviceResponse;
                    }
                    else
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Data = "Please select at least one line item.";
                        return serviceResponse;
                    }
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
        public ServiceResponse UnApplyPayment(PaymentApplicationLineItems paymentApplicationLineItems, List<PaymentApplicationLineItems> PaymentApplicationLineItems_List = null, bool Is_PaymentDeleted = false)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            SqlTransaction PaymentsTransaction = null;
            PaymentsTransaction = _baseRepository.GetNewTransaction();
            if (PaymentsTransaction == null)
            {
                serviceResponse.Status = false;
                serviceResponse.Data = "Unable to initiate a Transaction for the process.";
                return serviceResponse;
            }
            try
            {
                double RemainingBalance = 0;
                bool Paid = false;
                DateTime? PaidOn = null;
                byte _AccountingType_Id = 0;
                if (paymentApplicationLineItems != null)
                {
                    InvoiceDetails invoiceDetails = _invoiceRepository.GetInvoiceDetailsByInvoiceDetail_Id(paymentApplicationLineItems.InvoiceLineItem_Id);
                    if (invoiceDetails != null)
                    {
                        Invoices invoices = _invoiceRepository.GetInvoicesByInvoice_Id(invoiceDetails.Invoice_Id);
                        PolicyDetails policyDetails = _policyDetailsRepository.GetPolicyDetails(invoices.PolicyDetails_Id);
                        byte BillingType_Id = LookupUtility.GetAllBillingTypes().FirstOrDefault(x => x.Code == policyDetails.BillType)?.BillingType_Id ?? 0;
                        LU_Clients_AccountingTypes_BillingTypes _AccountingTypes_BillingTypes = _lookupRepository.GetClients_AccountingTypes_BillingTypes(clients.Client_Id, BillingType_Id);
                        if (_AccountingTypes_BillingTypes?.AccountingType_Id > 0)
                        {
                            _AccountingType_Id = _AccountingTypes_BillingTypes.AccountingType_Id;
                        }
                        else
                        {
                            _AccountingType_Id = clients.AccountingType_Id;
                        }
                        paymentApplicationLineItems.AccountingType_Id = _AccountingType_Id;
                        LU_BillingType_LineItems_Directions BillingLineItemsDirections = LookupUtility.GetAllLU_BillingType_LineItems_Directions().FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id);
                        LU_LineItems_Directions LineItemsDirections = LookupUtility.GetAllLineItems_Directions().FirstOrDefault(x => x.LineItems_Directions_Id == BillingLineItemsDirections.LineItems_Directions_Id);
                        RemainingBalance = invoiceDetails.RemainingBalance + paymentApplicationLineItems.AppliedAmount;
                        Paid = RemainingBalance == 0;
                        _invoiceRepository.UpdateBalanceInInvoiceDetails(PaymentsTransaction, invoiceDetails.Id, RemainingBalance, Paid);
                        if (LineItemsDirections?.Direction_Id == MasterLookup._Directions.AR)
                        {
                            if (invoices != null)
                            {
                                var invoiceDetailsList = _invoiceRepository.GetInvoiceDetailsByDirection_Id(invoices.Id, LineItemsDirections.Direction_Id);

                                //RemainingBalance = invoices.RemainingBalance + paymentApplicationLineItems.AppliedAmount;
                                //Paid = RemainingBalance == 0;
                                Paid = invoiceDetailsList.All(x => x != null && x.RemainingBalance == 0);
                                PaidOn = Paid == true ? DateTime.Now : null;
                                double invoiceRemainingBalance = invoices.RemainingBalance + paymentApplicationLineItems.AppliedAmount;
                                _invoiceRepository.UpdateBalanceInInvoices(PaymentsTransaction, invoices.Id, invoiceRemainingBalance, Paid);
                            }
                        }
                        PaymentApplications paymentApplications = _paymentsRepository.GetPaymentApplicationsByPaymentApplication_Id(paymentApplicationLineItems.PaymentApplication_Id);
                        if (paymentApplications != null)
                        {
                            Payments payments = _paymentsRepository.GetPaymentDetails(paymentApplications.Payment_Id);
                            if (payments != null)
                            {
                                if (paymentApplicationLineItems.isNegativeContext == true)
                                {
                                    RemainingBalance = payments.RemainingAmount + Math.Abs(paymentApplicationLineItems.AppliedAmount);
                                }
                                else
                                {
                                    RemainingBalance = payments.RemainingAmount + paymentApplicationLineItems.AppliedAmount;
                                }

                                _paymentsRepository.UpdatePaymentRemainingAmount(PaymentsTransaction, paymentApplications.Payment_Id, RemainingBalance);
                                if (PaymentApplicationLineItems_List != null)
                                {
                                    foreach (var item in PaymentApplicationLineItems_List)
                                    {
                                        _paymentsRepository.InactivePaymentApplicationLineItems(PaymentsTransaction, item.PaymentApplicationLineItem_Id);
                                    }
                                }
                                else
                                {
                                    _paymentsRepository.InactivePaymentApplicationLineItems(PaymentsTransaction, paymentApplicationLineItems.PaymentApplicationLineItem_Id);
                                }
                                Clients clients = _accountRepository.GetClients(userSession.Client_Id);
                                if (!Is_PaymentDeleted && PaymentApplicationLineItems_List == null)
                                {
                                    List<JournalEntries> journalEntryList = _journalEntryLogRepository.GetJournalEntryByPayment(MasterLookup._Sources.Payment, payments.Payment_Id);
                                    JournalEntries journalEntry = journalEntryList?.OrderByDescending(x => x.JournalIndex)?.FirstOrDefault();

                                    if (journalEntry == null && payments?.PaymentType_Id == MasterLookup._PaymentTypes.Bill)
                                    {
                                        if (PaymentsTransaction.Connection != null)
                                        {
                                            PaymentsTransaction.Rollback();
                                        }
                                        serviceResponse.Status = false;
                                        serviceResponse.Message = "Unapply is not supported for payments created through Bill Creation.";
                                        return serviceResponse;
                                    }

                                    if (clients.AccountingService_Id == MasterLookup._Services.Intuit)
                                    {
                                        QBOUtility qBOUtility = new QBOUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, _AccountingType_Id);
                                        //UpdateJournalRes updateJournalRes = qBOUtility.UpdateJournalEntry(PaymentsTransaction, new Payments(), new List<PaymentApplicationLineItems>(), journalEntries, false, paymentApplicationLineItems);
                                        CreateJournalRes createJournalRes = qBOUtility.CreateJournalEntry(PaymentsTransaction, payments, new List<PaymentApplicationLineItems>(), journalEntry, false, paymentApplicationLineItems);
                                        if (createJournalRes != null && createJournalRes.JournalEntry?.Id != null)
                                        {
                                            serviceResponse.Status = true;
                                            serviceResponse.Message = "Un Applied payment successfully and synced to accounting software successfully.";
                                            PaymentsTransaction.Commit();
                                        }
                                        else
                                        {
                                            if (PaymentsTransaction.Connection != null)
                                            {
                                                PaymentsTransaction.Rollback();
                                            }
                                            serviceResponse.Status = false;
                                            serviceResponse.Message = "Couldn't un apply and sync to accounting software";
                                            return serviceResponse;
                                        }
                                    }
                                    else if (clients.AccountingService_Id == MasterLookup._Services.Sage)
                                    {
                                        SageUtility sageUtility = new SageUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                                        var createJournalRes = Task.Run(() =>
                                        sageUtility.CreateJournalEntry_Intacct(
                                            PaymentsTransaction,
                                            payments,
                                             new List<PaymentApplicationLineItems>(),
                                            journalEntry,
                                            false, paymentApplicationLineItems
                                        )).Result;

                                        var serializer = new XmlSerializer(typeof(SageResponse));
                                        SageResponse createSageJournalResponse = new SageResponse();
                                        using (var reader = new StringReader(createJournalRes))
                                        {
                                            createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                                        }
                                        if (createSageJournalResponse != null && createSageJournalResponse?.Operation.Result?.Data?.GlBatch?.RecordNo != null)
                                        {
                                            serviceResponse.Status = true;
                                            serviceResponse.Message = $"Un Applied payment successfully and synced to accounting software successfully.";
                                            PaymentsTransaction.Commit();
                                            return serviceResponse;
                                        }
                                        else
                                        {
                                            if (PaymentsTransaction.Connection != null)
                                            {
                                                PaymentsTransaction.Rollback();
                                            }
                                            serviceResponse.Status = false;
                                            serviceResponse.Message = "Couldn't un apply and sync to accounting software." /*+ createSageJournalResponse.Operation?.Result?.*/;
                                            return serviceResponse;
                                        }
                                    }
                                    else
                                    {
                                        PaymentsTransaction.Commit();
                                    }

                                }
                                else
                                {
                                    PaymentsTransaction.Commit();
                                }
                            }
                        }
                    }
                }
                serviceResponse.Status = true;
                serviceResponse.Message = string.IsNullOrEmpty(serviceResponse.Message) ? $"Un Applied payments succesfully." : serviceResponse.Message;
            }
            catch (Exception ex)
            {
                if (PaymentsTransaction.Connection != null)
                {
                    PaymentsTransaction.Rollback();
                }
                serviceResponse.Status = false;
                serviceResponse.Message = "Something went wrong. Please contact administrator.";
            }
            return serviceResponse;

        }

        public IActionResult InvoicePaymentDetailsInfo(long Invoice_Id, long PolicyDetails_Id, byte Direction_Id)
        {
            InvoicePaymentDetails _invoicepaymentdetails = new InvoicePaymentDetails();
            _invoicepaymentdetails.PolicyDetails = _policyDetailsRepository.GetPolicyDetails(PolicyDetails_Id);
            _invoicepaymentdetails.PolicyDetails.InvoicesDetailsList = _invoiceRepository.GetInvoiceDetails(Invoice_Id);
            if (_invoicepaymentdetails.PolicyDetails.InvoicesDetailsList?.Count() > 0)
            {
                _invoicepaymentdetails.PolicyDetails.InvoicesDetailsList = CommonUtility.GetInvoiceDetailsListWithDescription(_invoicepaymentdetails.PolicyDetails.InvoicesDetailsList, _invoicepaymentdetails.PolicyDetails);
            }
            _invoicepaymentdetails.InvoicePaymentDetailsList = _paymentsRepository.GetPaymentDetailsByInvoice_Id(Invoice_Id);
            return PartialView(_invoicepaymentdetails);

            //List<InvoicePaymentDetails> _invoicepaymentdetails = _paymentsRepository.GetPaymentDetailsByInvoice_Id(Invoice_Id);
            //return PartialView(_invoicepaymentdetails);
        }
        public IActionResult JournalEntryDetails(long Payment_Id)
        {
            List<JournalEntriesLog> journalEntryLogsList = new();
            List<JournalEntries> journalentries = _journalEntryLogRepository.GetJournalEntryByPayment(MasterLookup._Sources.Payment, Payment_Id);

            foreach (JournalEntries journalEntry in journalentries)
            {
                var journalEntryLogs =
                    _journalEntryLogRepository.GetJournalEntriesLogByJournalEntry_Id(journalEntry.JournalEntry_Id);

                if (journalEntryLogs != null && journalEntryLogs.Any())
                {
                    journalEntryLogsList.AddRange(journalEntryLogs);
                }
            }

            return PartialView(journalEntryLogsList);
        }

        [HttpGet]
        public IActionResult DownloadSampleFileTemplate()
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", "BulkPaymentsTemplate.xlsx");
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Template file not found. Please contact the administrator or try again later.");
                }

                using (var workbook = new XLWorkbook(filePath))
                {
                    var ws = workbook.Worksheet(1);
                    var hiddenSheet = workbook.Worksheets.FirstOrDefault(s => s.Name == "DropdownData");
                    if (hiddenSheet == null)
                    {
                        hiddenSheet = workbook.Worksheets.Add("DropdownData");
                    }
                    else
                    {
                        hiddenSheet.Clear();
                    }
                    CommonUtility.AdddropdownEntityTypeNames(workbook, hiddenSheet, ws, userSession.Client_Id);
                    CommonUtility.Adddropdown(hiddenSheet, ws, LookupUtility.GetAllPaymentDirections().Select(x => x.Name).ToList(), "PaymentDirectionList", MasterLookup._PaymentsUploadColumnPropertyMappings.PaymentDirection);
                    CommonUtility.Adddropdown(hiddenSheet, ws, LookupUtility.GetAllPaymentTypes().Select(x => x.Name).ToList(), "PaymentTypeList", MasterLookup._PaymentsUploadColumnPropertyMappings.PaymentType);
                    CommonUtility.Adddropdown(hiddenSheet, ws, LookupUtility.GetAllPaymentStatus().Select(x => x.Name).ToList(), "PaymentStatusList", MasterLookup._PaymentsUploadColumnPropertyMappings.PaymentStatus);
                    var operatingBanks = LookupUtility.GetAllOperatingBanks().Where(x => x.Client_Id == userSession.Client_Id).Select(x => x.Name).ToList();
                    if (!operatingBanks.Any())
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Message = "No operating banks are configured. Please configure an operating bank before downloading the payment template.";
                        return Json(serviceResponse);
                    }
                    CommonUtility.Adddropdown(hiddenSheet, ws, operatingBanks, "BankAccountList", MasterLookup._PaymentsUploadColumnPropertyMappings.BankAccount);
                    hiddenSheet.Hide();
                    workbook.Save();
                }

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/xlsx'", "Bulk Payments Template.xlsx");
            }
            catch (InvalidOperationException)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "We were unable to generate the payment template at this time. Please verify the required configuration and try again. If the issue persists, contact your administrator.";
                return Json(serviceResponse);
            }
            catch (Exception)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "An unexpected issue occurred while preparing the payment template. Please try again later or contact your administrator if the problem continues.";
                return Json(serviceResponse);
            }
        }
        public IActionResult UploadPayments()
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<ServiceResponse> UploadBulkPayments(IFormFile file)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            CreateJournalRes createJournalResponse = new CreateJournalRes();
            SqlTransaction PaymentsTransaction = null;
            PaymentsTransaction = _baseRepository.GetNewTransaction();
            if (PaymentsTransaction == null)
            {
                serviceResponse.Status = false;
                serviceResponse.Data = "Unable to initiate a Transaction for the process.";
                return serviceResponse;
            }

            if (file == null || file.Length == 0)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = $"No file uploaded.";
                return serviceResponse;
            }

            try
            {
                List<string> columnHeaders = new List<string>();
                var rowData = new List<Dictionary<string, string>>();
                List<Payments> _bulkpaymentsupload = new List<Payments>();
                List<UploadErrorMessage> errorMessages = new List<UploadErrorMessage>();
                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.FirstOrDefault();
                        var rowCount = worksheet?.LastRowUsed()?.RowNumber();
                        var colCount = worksheet?.LastColumnUsed()?.ColumnNumber();
                        if (worksheet == null)
                        {
                            serviceResponse.Status = false;
                            serviceResponse.Message = $"No worksheet found.";
                            return serviceResponse;
                        }

                        var firstRow = worksheet.FirstRowUsed();
                        if (firstRow == null)
                        {
                            serviceResponse.Status = false;
                            serviceResponse.Message = $"The worksheet is empty.";
                            return serviceResponse;
                        }

                        foreach (var cell in firstRow.CellsUsed())
                        {
                            columnHeaders.Add(cell.GetString().Trim());
                        }

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var data = new Dictionary<string, string>();
                            for (int col = 1; col <= colCount; col++)
                            {
                                string header = columnHeaders[col - 1];
                                string value = worksheet.Cell(row, col).GetString().Trim();
                                data[header] = value;
                            }
                            rowData.Add(data);
                        }
                    }
                }

                StringBuilder _sb = new StringBuilder();
                List<string> _Excelcolumns = CommonUtility.GetAllPaymentUploadColumns();
                List<string> _IncorrectColumns = _Excelcolumns.Except(columnHeaders).ToList();
                if (_IncorrectColumns.Count() > 0 || rowData.Count < 1)
                {
                    if (_IncorrectColumns.Count() > 0)
                    {
                        foreach (var item in _IncorrectColumns)
                        {
                            _sb.Append(item + ",");
                        }
                        serviceResponse.Status = false;
                        serviceResponse.Message = $"{_sb.Remove(_sb.Length - 1, 1).ToString()} columns do not belong to table.";
                        return serviceResponse;
                    }
                    if (rowData.Count < 1)
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Message = $"Please upload the file with valid data";
                        return serviceResponse;
                    }
                }
                else
                {
                    foreach (var row in rowData)
                    {
                        Payments payments = new Payments();
                        payments = CommonUtility.MapPaymentsUploadColumnToPayments(row, out errorMessages, userSession.Client_Id);
                        _bulkpaymentsupload.Add(payments);
                    }

                    if (_bulkpaymentsupload.Count > 0)
                    {
                        if (errorMessages.Count > 0)
                        {
                            serviceResponse.Status = false;
                            serviceResponse.Message = $"{string.Join("<br>", errorMessages.DistinctBy(e => $"{e.PropertyName}:{e.Message}").Select(e => $"<b> {e.PropertyName}</b>: {e.Message}"))}";
                            return serviceResponse;
                        }
                        else
                        {
                            foreach (var paymentUpload in _bulkpaymentsupload)
                            {
                                Payments paymentUploadedDetails = _paymentsRepository.InsertUpdatePayments(PaymentsTransaction, paymentUpload);
                                if (paymentUploadedDetails.Payment_Id > 0)
                                {
                                    Clients clients = _accountRepository.GetClients(userSession.Client_Id);
                                    if (clients?.AccountingService_Id == MasterLookup._Services.Intuit)
                                    {
                                        QBOUtility qBOUtility = new QBOUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                                        createJournalResponse = qBOUtility.CreatePaymentJournalEntry(PaymentsTransaction, paymentUploadedDetails);
                                        if (string.IsNullOrEmpty(createJournalResponse?.JournalEntry?.Id))
                                        {
                                            serviceResponse.Status = false;
                                            serviceResponse.Message = "Cannot sync payment with Intuit!";
                                            return serviceResponse;
                                        }
                                    }
                                    else if (clients?.AccountingService_Id == MasterLookup._Services.Sage)
                                    {
                                        SageUtility sageUtility = new SageUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                                        var response = await sageUtility.CreatePaymentJournalEntry_Intacct(PaymentsTransaction, paymentUploadedDetails);
                                        var serializer = new XmlSerializer(typeof(SageResponse));
                                        SageResponse createSageJournalResponse = new SageResponse();
                                        using (var reader = new StringReader(response))
                                        {
                                            createSageJournalResponse = (SageResponse)serializer.Deserialize(reader);
                                        }
                                        if (createSageJournalResponse?.Operation.Result?.Data?.GlBatch?.RecordNo == null)
                                        {
                                            serviceResponse.Status = false;
                                            serviceResponse.Message = "Cannot sync payment with Sage!";
                                            return serviceResponse;
                                        }
                                    }

                                }
                            }
                            PaymentsTransaction.Commit();
                            serviceResponse.Status = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (PaymentsTransaction.Connection != null)
                {
                    PaymentsTransaction.Rollback();
                }
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public IActionResult ParentEntityName()
        {
            return PartialView();
        }
        [HttpPost]
        public ServiceResponse ParentEntityName(LU_EntityTypes entityType)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            SqlTransaction EntityTransaction = null;
            EntityTransaction = _baseRepository.GetNewTransaction();
            if (EntityTransaction == null)
            {
                serviceResponse.Status = false;
                serviceResponse.Data = "Unable to initiate a Transaction for the process.";
                return serviceResponse;
            }
            try
            {
                if (entityType == null)
                {
                    serviceResponse.Status = false;
                }
                else
                {
                    if (entityType.EntityType_Id == MasterLookup._EntityTypes.Agency)
                    {
                        AgentDetails agentDetails = new AgentDetails();
                        agentDetails.Name = entityType.Name;
                        agentDetails.Client_Id = userSession.Client_Id;
                        agentDetails = _policyDetailsRepository.InsertUpdateAgentDetails(EntityTransaction, agentDetails);
                    }
                    else if (entityType.EntityType_Id == MasterLookup._EntityTypes.Insured)
                    {
                        InsuredDetails insuredDetails = new InsuredDetails();
                        insuredDetails.Name = entityType.Name;
                        insuredDetails.Client_Id = userSession.Client_Id;
                        insuredDetails = _policyDetailsRepository.InsertUpdateInsuredDetails(EntityTransaction, insuredDetails);
                    }
                    else if (entityType.EntityType_Id == MasterLookup._EntityTypes.Carrier)
                    {
                        CarrierDetails carrierDetails = new CarrierDetails();
                        carrierDetails.Name = entityType.Name;
                        carrierDetails.Client_Id = userSession.Client_Id;
                        carrierDetails = _policyDetailsRepository.InsertUpdateCarrierDetails(EntityTransaction, carrierDetails);
                    }
                    else if (entityType.EntityType_Id == MasterLookup._EntityTypes.FinanceCompany)
                    {
                        FinanceDetails financeDetails = new FinanceDetails();
                        financeDetails.Name = entityType.Name;
                        financeDetails.Client_Id = userSession.Client_Id;
                        financeDetails = _policyDetailsRepository.InsertUpdateFinanceDetails(EntityTransaction, financeDetails);
                    }
                    else if (entityType.EntityType_Id == MasterLookup._EntityTypes.WholesalerMGA)
                    {
                        WholesalerDetails wholesalerDetails = new WholesalerDetails();
                        wholesalerDetails.Name = entityType.Name;
                        wholesalerDetails.Client_Id = userSession.Client_Id;
                        wholesalerDetails = _policyDetailsRepository.InsertUpdateWholesalerDetails(EntityTransaction, wholesalerDetails);
                    }
                    else if (entityType.EntityType_Id == MasterLookup._EntityTypes.State)
                    {
                        StateDetails stateDetails = new StateDetails();
                        stateDetails.Name = entityType.Name;
                        stateDetails.Client_Id = userSession.Client_Id;
                        stateDetails = _policyDetailsRepository.InsertUpdateStateDetails(EntityTransaction, stateDetails);
                    }
                    else if (entityType.EntityType_Id == MasterLookup._EntityTypes.Client)
                    {
                        serviceResponse.Status = false;
                        serviceResponse.Message = $"cannot add Client entity type";
                        return serviceResponse;
                    }
                    else if (entityType.EntityType_Id == MasterLookup._EntityTypes.Others)
                    {
                        var invoiceDetails = new InvoiceDetails
                        {
                            EntityType_Id = entityType.EntityType_Id,
                            EntityName = entityType.Name,
                        };
                        invoiceDetails = _policyDetailsRepository.InsertUpdateOtherEntityDetails(EntityTransaction, invoiceDetails, userSession.Client_Id);
                    }
                    EntityTransaction.Commit();
                    serviceResponse.Status = true;
                    serviceResponse.Message = $"{LookupUtility.GetAllEntityTypes().FirstOrDefault(c => c.EntityType_Id == entityType.EntityType_Id)?.Name} saved successfully.";
                }
            }
            catch (Exception ex)
            {
                if (EntityTransaction.Connection != null)
                {
                    EntityTransaction.Rollback();
                }
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
        public IActionResult DeletedPayments()
        {
            List<Payments> payments = _paymentsRepository.GetDeletedPayments(userSession.Client_Id);
            return PartialView(payments);
        }

        public IActionResult DownloadPaymentReceipt(long Payment_Id, int FileType_Id)
        {
            DocumentFileInfo documentFileInfo = DownloadReceipt(Payment_Id, FileType_Id);
            if (!string.IsNullOrEmpty(documentFileInfo?.Base64String))
            {
                serviceResponse.Status = true;
                serviceResponse.Data = documentFileInfo;
            }
            else
            {
                serviceResponse.Status = false;
                serviceResponse.Message = "There was an error while processing your request.";
            }

            return Ok(serviceResponse);
        }

        [NonAction]
        public DocumentFileInfo DownloadReceipt(long Payment_Id, int FileType_Id)
        {
            DataSet ds = new DataSet();
            DataSet dsAgency = new DataSet();
            byte[]? content = null;
            Payments payments = new Payments();
            Clients clients = new();
            payments = _paymentsRepository.GetPaymentDetails(Payment_Id);
            clients = _paymentsRepository.GetClientByPaymentId(Payment_Id);
            payments.Clients = clients;
            //payments = _paymentsRepository.GetPaymentDetails(Payment_Id);
            DocumentFileInfo documentFileInfo = new DocumentFileInfo();
            if (Payment_Id != null)
            {
                documentFileInfo.Name = $"Report-{DateTime.Now.Ticks}.pdf";
                documentFileInfo.FileType_Id = (byte)FileType_Id;

                if (FileType_Id == MasterLookup._LUFileTypes.PDF)
                {
                    payments.PaymentApplicationLineItemsList = _paymentsRepository.GetPaymentApplicationLineItemsForReceiptByPayment_Id(Payment_Id);
                    string FinalHTMLAlongWithTemplate = "";
                    string DynamicReportHTMLContent = "";
                    string ApplyFonts = "<link rel=\"preconnect\" href=\"https://fonts.googleapis.com\" />\r\n<link rel=\"preconnect\" href=\"https://fonts.gstatic.com\" crossorigin=\"anonymous\" />\r\n<link href=\"https://fonts.googleapis.com/css2?family=Poppins:ital,wght@0,100;0,200;0,300;0,400;0,500;0,600;0,700;0,800;0,900;1,100;1,200;1,300;1,400;1,500;1,600;1,700;1,800;1,900&display=swap\" rel=\"stylesheet\" />";

                    var viewDataDictionary = new ViewDataDictionary
                                               (
                                            new EmptyModelMetadataProvider(),
                                            new ModelStateDictionary()
                                               )
                    {
                        Model = payments,
                    };
                    viewDataDictionary.Add("FontSize", 6);
                    viewDataDictionary.Add("IsPDF", true);
                    string htmlContent = this.view.Render("Payments/PaymentReceipt", viewDataDictionary);
                    //htmlContent= htmlContent.Replace("{{fonts}}", ApplyFonts);
                    List<string> CSSReferenceList = new List<string> { "css/Reports/ReportPDF.css" };
                    var HtmlContent = htmlContent;
                    string HeaderHTMLString = @"<style>body {margin: 0px;padding: 0px;}</style>";
                    string FooterHTMLString = @"<div style=text-align: right; padding: 10pt 20pt; font-size: 16pt; font-family: 'Poppins', sans-serif;>""{CrtPage}</div>";
                    bool IsCoverPageExcluded = false;
                    PDFRequest request = new PDFRequest()
                    {
                        BodyHTMLString = HtmlContent,
                        HeaderHTMLString = HeaderHTMLString,
                        FooterHTMLString = FooterHTMLString,
                        IsCoverPageExcluded = IsCoverPageExcluded
                    };
                    string Service_Url = MasterLookup._PDFCoverterAPIURL;
                    ServiceLogInfo serviceLogInfo = new ServiceLogInfo();
                    ServiceResponse response = (ServiceResponse)HttpServiceUtility.ExecuteJsonPostRequest(serviceLogInfo, Service_Url, request, typeof(ServiceResponse), true);
                    if (response.Status)
                    {
                        documentFileInfo.Name = $"PaymentReceipt-{Payment_Id} -{DateTime.Now.Ticks}.pdf";
                        documentFileInfo.Base64String = response.Data.ToString();
                        documentFileInfo.ContentType = "application/pdf";
                    }
                }
            }

            return documentFileInfo;
        }
        [HttpPost]
        public IActionResult UploadInvoicePaymentFile(IFormFile file, byte Direction_Id)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file");

            InvoiceUpload model = new InvoiceUpload();

            try
            {
                string tempFileId = Guid.NewGuid().ToString();

                byte[] fileBytes;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    fileBytes = ms.ToArray();
                }

                var uploads = new InvoiceFilterFileUploads
                {
                    Document = fileBytes,
                    Filename = file.FileName,
                    CreatedBy = userSession.User_Id
                };

                var loggedExcel = _IExcelLogRepository.InsertInvoiceFilterFileUploads(uploads);

                if (loggedExcel?.Document == null)
                    throw new Exception("Failed to save Excel file.");

                model.Excel_Id = loggedExcel.InvoiceFilterFileUploads_Id;

                using var stream = new MemoryStream(loggedExcel.Document);
                model.Columns = ExcelHelper.GetColumns(stream);

                model.TempFileId = tempFileId;
                model.Direction_Id = Direction_Id;
                model.Status = true;

                return PartialView("ColumnMapping", model);
            }
            catch (Exception ex)
            {
                model.Status = false;
                model.Message = ex.Message;
                return PartialView("ColumnMapping", model);
            }
        }

        [HttpPost]
        public IActionResult SearchInvoicesPaymentDetailsFromExcel(InvoiceExcelMappingRequest model)
        {
            //string filePath = Directory.GetFiles(Path.Combine(_webHostEnvironment.WebRootPath, "TempUploads"), model.TempFileId + ".*").FirstOrDefault();
            var ExcelFile = _IExcelLogRepository.GetExcelFileById(model.Excel_Id);

            if (ExcelFile.Document == null)
                return BadRequest("File not found");

            using var stream = new MemoryStream(ExcelFile.Document);

            var excelRows = ExcelHelper.ReadAsDictionaryList(stream);

            var uploadedInvoicesPayment = new List<UploadedInvoicesPaymentDTO>();

            // Normalize mapping keys ONCE
            string invoiceKey = model.ColumnMapping.ContainsKey("InvoiceNumber") ? model.ColumnMapping["InvoiceNumber"]?.Trim().ToLowerInvariant() : null;

            string policyKey = model.ColumnMapping.ContainsKey("PolicyNumber") ? model.ColumnMapping["PolicyNumber"]?.Trim().ToLowerInvariant() : null;

            string amountKey = model.ColumnMapping["PaymentAmount"]?.Trim().ToLowerInvariant();

            foreach (var row in excelRows)
            {
                // Normalize Excel row keys
                var normalizedRow = row.ToDictionary(
                    k => k.Key.Trim().ToLowerInvariant(),
                    v => v.Value
                );

                uploadedInvoicesPayment.Add(new UploadedInvoicesPaymentDTO
                {
                    InvoiceNumber = invoiceKey != null && normalizedRow.ContainsKey(invoiceKey) ? normalizedRow[invoiceKey] : null,

                    PolicyNumber = policyKey != null && normalizedRow.ContainsKey(policyKey) ? normalizedRow[policyKey] : null,

                    PaymentAmount = amountKey != null && normalizedRow.ContainsKey(amountKey) ? normalizedRow[amountKey] : null,

                    TempFileId = model.TempFileId,
                    Direction_Id = model.Direction_Id
                });
            }

            return Json(new
            {
                uploadedInvoicesPayment,
                model.Direction_Id
            });
        }
        [HttpPost]
        public ServiceResponse UnApplyPaymentsByPaymentId(long paymentId)
        {
            return ProcessUnapplyPayment(paymentId, false);
        }
        public ServiceResponse MaskAsBounced(long paymentId)
        {
            return ProcessUnapplyPayment(paymentId, true);
        }
        private ServiceResponse ProcessUnapplyPayment(long paymentId, bool isBounce = false)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            CreateJournalRes createJournalResponse = new CreateJournalRes();
            SqlTransaction transaction = _baseRepository.GetNewTransaction();

            try
            {
                var paymentLineItems = _paymentsRepository.GetPaymentApplicationLineItemsByPayment_Id(paymentId);
                if (paymentLineItems == null || paymentLineItems.Count == 0)
                {
                    return new ServiceResponse
                    {
                        Status = false,
                        Message = $"No transactions found with Payment ID {paymentId}"
                    };
                }
                var groupedLineItems = paymentLineItems.GroupBy(x => x.InvoiceLineItem_Id).ToList();
                foreach (var group in groupedLineItems)
                {
                    var combinedItem = new PaymentApplicationLineItems
                    {
                        InvoiceLineItem_Id = group.Key,
                        AppliedAmount = group.Sum(x => x.AppliedAmount),
                        UnAppliedAmount = group.Sum(x => x.AppliedAmount),
                        PaymentApplication_Id = group.First().PaymentApplication_Id,
                        AccountingType_Id = group.First().AccountingType_Id
                    };
                    var response = UnApplyPayment(combinedItem, group.ToList(), true);
                    if (!response.Status)
                    {
                        transaction.Rollback();
                        return response;
                    }
                }
                var clients = _accountRepository.GetClients(userSession.Client_Id);
                if (clients?.AccountingService_Id == MasterLookup._Services.Intuit)
                {
                    var qboUtility = new QBOUtility(userSession.Client_Id, userSession.User_Id, clients.AccountingService_Id, clients.AccountingType_Id);
                    var journalEntries = _journalEntryLogRepository.GetJournalEntryByPayment(MasterLookup._Sources.Payment, paymentId);
                    var journalEntry = journalEntries?.OrderByDescending(x => x.JournalIndex)?.FirstOrDefault();
                    if (journalEntry == null)
                    {
                        transaction.Rollback();
                        return new ServiceResponse
                        {
                            Status = false,
                            Message = "No previous journal entry found."
                        };
                    }
                    var payments = _paymentsRepository.GetPaymentDetails(paymentId);
                    var groupedSummary = groupedLineItems
                        .Select(g => new PaymentApplicationLineItems
                        {
                            InvoiceLineItem_Id = g.Key,
                            AppliedAmount = g.Sum(x => x.AppliedAmount),
                            PaymentApplication_Id = g.First().PaymentApplication_Id,
                            AccountingType_Id = g.First().AccountingType_Id
                        }).ToList();
                    createJournalResponse = qboUtility.CreateUnapplyPaymentJournalEntry(transaction, payments, groupedSummary, journalEntry, isBounce);
                    if (string.IsNullOrEmpty(createJournalResponse?.JournalEntry?.Id))
                    {
                        transaction.Rollback();
                        return new ServiceResponse
                        {
                            Status = false,
                            Message = "Cannot sync payment with Intuit!"
                        };
                    }
                }
                if (isBounce)
                {
                    _paymentsRepository.UpdatePaymentRemainingAmount(transaction, paymentId, 0);
                    if (_paymentsRepository.UpdatePaymentStatus(transaction, paymentId, MasterLookup._PaymentStatus.BouncedReversed, userSession.Client_Id) <= 0)
                    {
                        transaction.Rollback();
                        return new ServiceResponse
                        {
                            Status = false,
                            Message = "Failed to update payment status."
                        };
                    }
                }
                transaction.Commit();
                return new ServiceResponse
                {
                    Status = true,
                    Message = isBounce ? "Payment Bounced/Reversed successfully" : "Payments unapplied successfully."
                };
            }
            catch (Exception ex)
            {
                transaction?.Rollback();

                return new ServiceResponse
                {
                    Status = false,
                    Message = ex.Message
                };
            }
        }
        //   private List<SelectedInvoiceDetailDTO> PrepareInvoicePayload(
        //List<SelectedInvoiceDetailDTO> selectedInvoiceDetails)
        //   {
        //       List<SelectedInvoiceDetailDTO> groupedList =
        //           new List<SelectedInvoiceDetailDTO>();

        //       // Already partial records
        //       groupedList.AddRange(
        //           selectedInvoiceDetails.Where(x => x.IsPartial));

        //       // Group non-partial records by Invoice_Id
        //       var groupedInvoices = selectedInvoiceDetails
        //           .Where(x => !x.IsPartial)
        //           .GroupBy(x => x.Invoice_Id);

        //       foreach (var group in groupedInvoices)
        //       {
        //           double ApplyAmount = (double)group.Sum(x => Convert.ToDecimal(x.Amount));
        //           double RemainingBalance = (double)group.Sum(x => Convert.ToDecimal(x.RemainingBalance));
        //           bool isPartial = false;
        //           if (ApplyAmount == RemainingBalance)
        //           {
        //               isPartial = false;
        //           }
        //           else if ((ApplyAmount > RemainingBalance) && (ApplyAmount > 0 && RemainingBalance > 0))
        //           {
        //               isPartial = false;
        //           }
        //           else
        //           {
        //               isPartial = true;
        //           }
        //           SelectedInvoiceDetailDTO dto =
        //               new SelectedInvoiceDetailDTO
        //               {
        //                   Invoice_Id = group.Key,
        //                   InvoiceDetail_Id = 0,
        //                   Amount = ApplyAmount,
        //                   RemainingBalance = RemainingBalance,
        //                   IsPartial = isPartial,
        //                   InvoiceDetail_Ids = group?
        //                       .Where(x => x.InvoiceDetail_Id > 0)?
        //                       .Select(x => x.InvoiceDetail_Id)?
        //                       .ToList()
        //               };

        //           groupedList.Add(dto);
        //       }

        //       return groupedList;
        //   }
        [HttpPost]
        public IActionResult PrintCheck(long Payment_Id, string AccountNumber, string RoutingNumber, string SignerValue, string SignerType)
        {
            ViewBag.PaymentId = Payment_Id;
            ViewBag.AccountNumber = AccountNumber;
            ViewBag.RoutingNumber = RoutingNumber;
            ViewBag.SignerValue = SignerValue;
            ViewBag.SignerType = SignerType;
            return PartialView();
        }
        [HttpPost]
        public ServiceResponse DownloadCheck(long Payment_Id, string AccountNumber, string RoutingNumber, string SignerValue, int CheckNumber, string SignerType)
        {
            ChecKeeperUtility checKeeperUtility = new ChecKeeperUtility(userSession.Client_Id);
            ServiceResponse serviceResponse = new ServiceResponse();
            serviceResponse = checKeeperUtility.CreateCheck(Payment_Id, AccountNumber, RoutingNumber, SignerValue, CheckNumber, SignerType);
            if (serviceResponse.Data != null)
            {
                int UpdatedStatus = _paymentsRepository.UpdateCheckPaymentPrintStatus(Payment_Id);
            }
            return serviceResponse;
        }
        [HttpPost]
        public ServiceResponse BillBatchRetry(long payment_Id, DateTime date)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            try
            {
                var result = _paymentsRepository.RetryBillBatch(userSession.Client_Id, userSession.User_Id, payment_Id, date);
                serviceResponse.Status = true;
                serviceResponse.Message = result;
            }
            catch (Exception ex)
            {
                serviceResponse.Status = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
    }
}
