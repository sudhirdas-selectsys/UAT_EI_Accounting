using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.CoverPay
{
    public class CoverPayInvoiceDetails
    {
        public List<InvoiceSchedule> InvoiceSchedules { get; set; } = new();
        public List<InvoiceAmount> InvoiceAmounts { get; set; } = new();
        public List<InvoiceFeeTax> InvoiceFeeTaxes { get; set; } = new();
        public InvoiceSummary Summary { get; set; } = new();
    }

    public class InvoiceSchedule
    {
        public int Invoice_Id { get; set; }
        public int InstallmentNo { get; set; }
        public string Type { get; set; }
        public DateTime DueDate { get; set; }
        public string ChargeTiming { get; set; }
        public int? Installments { get; set; }
    }

    public class InvoiceAmount
    {
        public int Invoice_Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public double TotalAmount { get; set; }
    }

    public class InvoiceFeeTax
    {
        public int Invoice_Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public double Amount { get; set; }
    }

    public class InvoiceSummary
    {
        public double TotalAmount { get; set; }
        public string InvoiceNo { get; set; }
        public string BillingType { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AddressType { get; set; }
        public string AddressLine1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }
}
