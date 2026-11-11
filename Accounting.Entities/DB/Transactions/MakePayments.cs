using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class MakePayments : BaseEntity
    {
        public long AgentDetail_Id { get; set; }
        public string InvoiceNo {  get; set; }
        public string BillType {  get; set; }
        public long? LineItem_Id {  get; set; }
        public byte? EntityType_Id { get; set; }
        public long? Entity_Id { get; set; }
        public byte? AmountDueBasedOnType_Id { get; set; }
        public string AgencyName { get; set; }
        public decimal APAmount {  get; set; }
        public double AmountCollected {  get; set; }
        public double AmountPaid {  get; set; }
        public double RemainingBalance {  get; set; }
        public double NetDue {  get; set; }
        public string PolicyNumber { get; set; }
        public string InsuredName { get; set; }
        public string LineTypeName { get; set; }
        public bool OutstandingBalances { get; set; }
        public string EntityName { get; set; }
        public long? Payee_Id { get; set; }
        public byte? PayeeType_Id { get; set; }
        public long Invoice_Id { get; set; }
        public long LineItemId { get; set; }
        public string LineItemList { get; set; }
        public double Amount {  get; set; }
        public decimal AgentCommissionByPremiumAmount {  get; set; }
        public decimal AgentCommissionByTotalCommission {  get; set; }
        public long InvoiceLineItem_Id { get; set; }
        public string? TransactionType { get; set; }
        public DateTime EffectiveDate { get; set; }
        public byte Direction_Id { get; set; }
        public decimal NetOutgoingPaymentDue { get; set; }
        public decimal ARAmount { get; set; }
        public DateTime TransactionEffectiveDate { get; set; }
        public string SortColumn { get; set; }
        public string SortOrder { get; set; }
        public string InvoiceDate { get; set; }
        public string InvoiceCreatedFromDate { get; set; }
        public string InvoiceCreatedToDate { get; set; }
        public string InvoiceEffectiveFromDate { get; set; }
        public string InvoiceEffectiveToDate { get; set; }
        public string PaymentBookedDate { get; set; }
    }
}
