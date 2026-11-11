using Accounting.DBUtilities.Implementations.Lookups;
using Accounting.DBUtilities.Implementations.Transactions;
using Accounting.DBUtilities.Interfaces.Lookups;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Entities.DB.Lookups;
using Accounting.Entities.DB.Transactions;
using Accounting.Entities.External.QBO.Bill;
using Accounting.Entities.External.QBO.Journal;
using Accounting.Utilities.Internal;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.Data.SqlClient;
using System.Text;

namespace Accounting.Utilities.External.QBO
{

    public class BillUtility
    {
        IPolicyDetailsRepository _PolicyDetailsRepository = new PolicyDetailsRepository();
        IClientsRepository _ClientsRepository = new ClientsRepository();
        ILookupRepository _LookupRepository = new LookupRepository();
        ILOBCarrierGroupingRepository _LobCarrierGroupingRepository = new LOBCarrierGroupingRepository();
        public byte _Service_Id { get; set; }
        public long _Client_Id { get; set; }
        public BillUtility(byte Sevice_Id, long Client_Id)
        {
            _Service_Id = Sevice_Id;
            _Client_Id = Client_Id;
        }

        public CreateBillReq PrepareCreateBillReqObject(Payments payments, string ExternalVendorIdentifier, byte Direction_Id)
        {
            CreateBillReq createBillReq = new CreateBillReq();
            var ChartOfAccountsLineItemsList = _LookupRepository.GetAllLineItems_ChartOfAccounts(_Service_Id, _Client_Id);
            byte _AccountingType_Id = 0;
            var PolicyDetailsList = payments.PolicyDetailsList;
            if (PolicyDetailsList?.Count > 0)
            {
                createBillReq.VendorRef = new VendorRef()
                {
                    value = ExternalVendorIdentifier,
                };
                List<Line> lines = new List<Line>();
                byte _ChartOfAccountsClassificationType_Id = 0;
                foreach (var policy in PolicyDetailsList)
                {
                    _AccountingType_Id = policy.AccountingType_Id;
                    //if (_AccountingType_Id == MasterLookup._AccountingTypes.Cash)
                    //{
                    //    _ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability;
                    //}
                    //else
                    //{
                    //    _ChartOfAccountsClassificationType_Id = MasterLookup._ChartOfAccountsClassificationTypes.AccountsReceivableAccountsPayable;
                    //}
                    string DebitCOAValue = null;
                    string CreditCOAValue = null;
                    string DebitCOAName = null;
                    string CreditCOAName = null;
                    string DebitDescription = null;
                    string CreditDescription = null;
                    int? DebitCOA = null;
                    int? CreditCOA = null;
                    InvoiceDetails invoiceDetails = policy.Invoices.InvoiceDetails;
                    Invoices invoices = policy.Invoices;
                    PolicyDetails policyDetails = policy;

                    if (invoiceDetails != null)
                    {
                        Line line = new Line();
                        LU_BillingType_LineItems_Directions BillingLineItemsDirections = LookupUtility.GetAllLU_BillingType_LineItems_Directions().FirstOrDefault(x => x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id);
                        LU_LineItems_Directions LineItemsDirections = LookupUtility.GetAllLineItems_Directions().FirstOrDefault(x => x.LineItems_Directions_Id == BillingLineItemsDirections.LineItems_Directions_Id);
                        LU_LineItems_Clients lineItems_Clients = LookupUtility.GetAllLineItems_Clients().FirstOrDefault(x => x.LineItems_Clients_Id == LineItemsDirections.LineItems_Clients_Id);
                        LU_LineItems LineItems = LookupUtility.GetAllLineItems().FirstOrDefault(c => c.LineItem_Id == lineItems_Clients?.LineItem_Id);
                        LU_LineItemTypes LineType = LookupUtility.GetAllLineItemTypes().FirstOrDefault(c => c.LineItemType_Id == LineItems.LineItemType_Id);
                        LU_LOBs LOB = LookupUtility.GetAllLOBs().FirstOrDefault(x => x.LOB_Id == invoiceDetails.LOB_Id);
                        if (_AccountingType_Id == MasterLookup._AccountingTypes.Cash)
                        {
                            _ChartOfAccountsClassificationType_Id =
                                MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability;
                        }
                        else if (_AccountingType_Id == MasterLookup._AccountingTypes.Accrual)
                        {
                            if (lineItems_Clients?.Is_Passthrough == true && (LineItemsDirections?.Direction_Id == MasterLookup._Directions.AP))
                            {
                                _ChartOfAccountsClassificationType_Id =
                                    MasterLookup._ChartOfAccountsClassificationTypes.IncomeExpenseLiability;
                            }
                            else
                            {
                                _ChartOfAccountsClassificationType_Id =
                                    MasterLookup._ChartOfAccountsClassificationTypes.AccountsReceivableAccountsPayable;
                            }
                        }
                        LOBCarrierGrouping lobcarriergroupingDetails = _LobCarrierGroupingRepository.GetLOBCarrierGroupingDetails(LOB.LOB_Id, policyDetails.CarrierDetail_Id, MasterLookup._Services.Intuit, policyDetails.Client_Id);
                        var ChartOfAccountsLineItems = ChartOfAccountsLineItemsList.FirstOrDefault(x => x.Client_Id == _Client_Id && x.BillingType_LineItems_Directions_Id == invoiceDetails.BillingType_LineItems_Directions_Id && x.LOB_Carrier_Grouping_Id == lobcarriergroupingDetails.LOB_Carrier_Grouping_Id && x.ChartOfAccountsClassificationType_Id == _ChartOfAccountsClassificationType_Id);
                        var ChartOfAccounts = LookupUtility.GetAllChartofAccounts().FirstOrDefault(x => x.ChartOfAccounts_Id == ChartOfAccountsLineItems?.ChartOfAccount_Id);
                        PaymentApplicationLineItems paymentApplicationLineItems = invoiceDetails.PaymentApplicationLineItems;
                        if(Direction_Id == MasterLookup._Directions.AR && paymentApplicationLineItems.AppliedAmount < 0)
                        {
                            line.Amount = Math.Abs((decimal)paymentApplicationLineItems.AppliedAmount);
                        }
                        else
                        {
                            line.Amount = (decimal)paymentApplicationLineItems.AppliedAmount;
                        }
                        line.Description = $"{invoices.InvoiceNo} - {LineItems.Name}, Amount Due for Policy Number: {policyDetails.PolicyNumber}";
                        line.DetailType = "AccountBasedExpenseLineDetail";
                        line.AccountBasedExpenseLineDetail = new AccountBasedExpenseLineDetail()
                        {
                            AccountRef = new Entities.External.QBO.Bill.AccountRef()
                            {
                                value = ChartOfAccounts.ReferenceID,
                            }
                        };
                        lines.Add(line);
                    }

                }
                var notes = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(payments.MemoDescription))
                {
                    notes.AppendLine("---Memo---");
                    notes.AppendLine(payments.MemoDescription);
                }

                if (payments.EntityType_Id == MasterLookup._EntityTypes.Agency &&
                    PolicyDetailsList != null &&
                    PolicyDetailsList.Any(policy =>
                        !string.IsNullOrWhiteSpace(policy.BankName) ||
                        !string.IsNullOrWhiteSpace(policy.AccountTypeName) ||
                        !string.IsNullOrWhiteSpace(policy.RoutingNumber) ||
                        !string.IsNullOrWhiteSpace(policy.AccountNumber)))
                {
                    // Two empty lines before ACH section
                    notes.AppendLine();
                    notes.AppendLine();

                    notes.AppendLine("---ACH---");
                    var agentPolicyDetails = PolicyDetailsList?.FirstOrDefault();
                    //foreach (var policy in PolicyDetailsList)
                    if (agentPolicyDetails != null)
                    {
                        //if (policy == null)
                        //    continue;

                        notes.AppendLine($"Bank Name: {agentPolicyDetails?.BankName ?? string.Empty}");
                        notes.AppendLine();

                        notes.AppendLine($"Account Type: {agentPolicyDetails?.AccountTypeName ?? string.Empty}");
                        notes.AppendLine();

                        notes.AppendLine($"Routing Number: {agentPolicyDetails?.RoutingNumber ?? string.Empty}");
                        notes.AppendLine();

                        notes.AppendLine($"Account Number: {agentPolicyDetails?.AccountNumber ?? string.Empty}");
                        notes.AppendLine();
                    }
                }

                createBillReq.PrivateNote = notes.ToString().TrimEnd();
                createBillReq.TxnDate = payments.TxnDate.ToString("yyyy-MM-dd");
                DateTime PaymentDate = Convert.ToDateTime(payments.PaymentDate);
                createBillReq.DueDate = PaymentDate.ToString("yyyy-MM-dd");
                createBillReq.DocNumber = $"EI-PAY-{payments.Payment_Id}";
                createBillReq.Line = lines;
            }
            return createBillReq;
        }
    }
}
