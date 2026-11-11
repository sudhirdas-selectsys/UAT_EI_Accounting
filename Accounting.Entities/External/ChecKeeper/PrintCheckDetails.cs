using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.ChecKeeper
{
    public class PrintCheckDetails
    {
        public long? Payment_Id { get; set; }

        public DateTime? PaymentDate { get; set; }

        public double Amount { get; set; }

        public string EntityName { get; set; }

        public string EntityAddress { get; set; }

        public string EntityCity { get; set; }

        public string EntityState { get; set; }

        public string EntityZipCode { get; set; }

        public string AccountNumber { get; set; }

        public string RoutingNumber { get; set; }

        public string FromCompanyName { get; set; }

        public string FromAddress { get; set; }

        public string FromCity { get; set; }

        public string FromState { get; set; }

        public string FromZipCode { get; set; }

        public string InvoiceNo { get; set; }

        public string PolicyNumber { get; set; }

        public string AgencyName { get; set; }

        public string InsuredsName { get; set; }

        public string LineItemName { get; set; }

        public double AppliedAmount { get; set; }

        public string? PayeeNameOnCheck { get; set; }
        public string? CompanyNameOnCheck { get; set; }
    }
}
