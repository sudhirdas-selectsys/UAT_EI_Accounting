using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Entities.External.ChecKeeper
{
    public class CreateCheckReq
    {
        public Delivery delivery { get; set; }

        public List<CheckModel> checks { get; set; }
    }

    public class Delivery
    {
        public string method { get; set; }
    }

    public class CheckModel
    {
        public Bank bank { get; set; }

        public Payer payer { get; set; }

        public Payee payee { get; set; }

        public Signer signer { get; set; }

        public int amount { get; set; }

        public int number { get; set; }

        public string date { get; set; }

        public string memo { get; set; }

        public FromAddress from_address { get; set; }

        public ToAddress to_address { get; set; }

        public InvoiceTable invoice_table { get; set; }

        public string template { get; set; }
        public bool ExcludeTemplate { get; set; }

        public bool ShouldSerializetemplate()
        {
            return !ExcludeTemplate;
        }
    }

    public class Bank
    {
        public string routing { get; set; }

        public string account { get; set; }
    }

    public class Payer
    {
        public string line1 { get; set; }
        public string line2 { get; set; }
    }

    public class Payee
    {
        public string line1 { get; set; }
    }

    public class Signer
    {
        public string type { get; set; }

        public string value { get; set; }
    }

    public class FromAddress
    {
        public string name { get; set; }

        public string line1 { get; set; }

        public string line2 { get; set; }
        public string line3 { get; set; }   
        public string city { get; set; }

        public string state { get; set; }

        public string zip { get; set; }

        public string country { get; set; }
    }

    public class ToAddress
    {
        public string name { get; set; }

        public string company { get; set; }

        public string line1 { get; set; }

        public string line2 { get; set; }

        public string city { get; set; }

        public string state { get; set; }

        public string zip { get; set; }

        public string country { get; set; }
    }

    public class InvoiceTable
    {
        public List<Heading> headings { get; set; }

        public List<List<string>> rows { get; set; }
    }

    public class Heading
    {
        public string label { get; set; }

        public int size { get; set; }
    }
}
