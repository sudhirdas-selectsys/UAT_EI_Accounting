using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.CoverPay
{
    public class CoverPayOutstandingInvoiceDetails
    {
        public List<CoverPayOutstandingInvoiceDetailsData> InvoiceDetails { get; set; } = new();
        public List<CoverPayOutstandingInvoicePremiumDetails> PremiumDetails { get; set; } = new();
        public List<CoverPayOutstandingInvoiceTaxFeeDetails> TaxFeeDetails { get; set; } = new();
    }

    public class CoverPayOutstandingInvoiceDetailsData
    {
        public long PolicyDetails_Id { get; set; }
        public long Invoice_Id { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public double Amount { get; set; }
        public double PaidAmount { get; set; }
        public double OutstandingAmount { get; set; }
        public int InstallmentNumber { get; set; }
        public string Type { get; set; }
    }

    public class CoverPayOutstandingInvoicePremiumDetails
    {
        public long PolicyDetails_Id { get; set; }
        public long Invoice_Id { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public double OutstandingAmount { get; set; }
    }

    public class CoverPayOutstandingInvoiceTaxFeeDetails
    {
        public long PolicyDetails_Id { get; set; }
        public long Invoice_Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public double OutstandingAmount { get; set; }
    }
}
