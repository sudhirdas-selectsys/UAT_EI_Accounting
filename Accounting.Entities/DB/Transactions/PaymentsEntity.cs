using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.DB.Transactions
{
    public class PaymentsEntity : BaseEntity
    {
        public long AgentDetail_Id { get; set; }
        public string AgentReference_Id { get; set; }
        public string Email { get; set; }
        public string AgencyName { get; set; }
        public byte? EntityType_Id { get; set; }
        public long? Entity_Id { get; set; }
        public string PolicyNumber { get; set; }
        public string InsuredName { get; set; }
        public string InvoiceNo { get; set; }
        public string BillType { get; set; }
        public long? LineItem_Id { get; set; }
        public decimal? NetOutgoingPaymentDue { get; set; }
        public decimal? NetDue { get; set; }
        public byte? AmountDueBasedOnType_Id { get; set; }
        public bool OutstandingBalances { get; set; }
        public string EntityName { get; set; }
        public string EntityTypeName { get; set; }
        public long InvoiceId { get; set; }
        public long LineItemId { get; set; }
        public string LineItemList { get; set; }
        public byte Direction_Id { get; set; }
        public string InvoiceDate { get; set; }
        public string InvoiceCreatedFromDate { get; set; }
        public string InvoiceCreatedToDate { get; set; }
        public string InvoiceEffectiveFromDate { get; set; }
        public string InvoiceEffectiveToDate { get; set; }
        public string PaymentBookedDate { get; set; }
        public bool HasBankInfo { get; set; }
    }
}
