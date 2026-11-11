using Microsoft.Extensions.Logging.Abstractions;
using System.Xml.Serialization;

namespace Accounting.Entities.External.Sage.Journal
{

    [XmlRoot("response")]
    public class SageResponse
    {
        [XmlElement("control")]
        public Control Control { get; set; }

        [XmlElement("operation")]
        public Operation Operation { get; set; }
    }

    public class Control
    {
        [XmlElement("status")]
        public string Status { get; set; }

        [XmlElement("senderid")]
        public string SenderId { get; set; }

        [XmlElement("controlid")]
        public string ControlId { get; set; }

        [XmlElement("uniqueid")]
        public string UniqueId { get; set; }

        [XmlElement("dtdversion")]
        public string DtdVersion { get; set; }
    }

    public class Operation
    {
        [XmlElement("authentication")]
        public Authentication Authentication { get; set; }

        [XmlElement("result")]
        public Result Result { get; set; }
    }

    public class Authentication
    {
        [XmlElement("status")]
        public string Status { get; set; }

        [XmlElement("userid")]
        public string UserId { get; set; }

        [XmlElement("companyid")]
        public string CompanyId { get; set; }

        [XmlElement("locationid")]
        public string LocationId { get; set; }

        [XmlElement("sessiontimestamp")]
        public DateTime SessionTimestamp { get; set; }

        [XmlElement("sessiontimeout")]
        public DateTime SessionTimeout { get; set; }
    }

    public class Result
    {
        [XmlElement("status")]
        public string Status { get; set; }

        [XmlElement("function")]
        public string Function { get; set; }

        [XmlElement("controlid")]
        public string ControlId { get; set; }

        [XmlElement("data")]
        public Data Data { get; set; }
        [XmlElement("errormessage")]
        public Errormessage Errormessage { get; set; }
    }

    public class Data
    {
        [XmlAttribute("listtype")]
        public string ListType { get; set; }

        [XmlAttribute("count")]
        public int Count { get; set; }

        [XmlElement("glbatch")]
        public GlBatch GlBatch { get; set; }

        [XmlElement("GLBATCH")]
        public GlBatch GlBatchRead { get; set; }
        [XmlElement("vendor")]
        public Vendor Vendor { get; set; }

        [XmlElement("VENDOR")]
        public Vendor VendorRead { get; set; }

        [XmlElement("bill")]
        public Bill Bill { get; set; }

        [XmlElement("apbill")]
        public APBill APBill { get; set; }
    }
    public class APBill
    {
        [XmlElement("RECORDNO")]
        public string RecordNo { get; set; }
    }
    public class Bill
    {
        [XmlElement("RECORDNO")]
        public string RecordNo { get; set; }

        [XmlElement("BILLNO")]
        public string BillNo { get; set; }

        [XmlElement("VENDORID")]
        public string VendorId { get; set; }

        [XmlElement("STATE")]
        public string State { get; set; }
    }
    public class Vendor
    {
        [XmlElement("RECORDNO")]
        public string RecordNo { get; set; }

        [XmlElement("VENDORID")]
        public string VendorId { get; set; }

        [XmlElement("NAME")]
        public string Name { get; set; }

        [XmlElement("STATUS")]
        public string Status { get; set; }

        [XmlElement("VENDTYPE")]
        public string VendorType { get; set; }
    }
    public class GlBatch
    {
        [XmlElement("RECORDNO")]
        public int RecordNo { get; set; }
        [XmlElement("BATCHNO")]
        public int BatchNo { get; set; }

        [XmlArray("ENTRIES")]
        [XmlArrayItem("glentry")]
        public List<GLENTRY> Entries { get; set; }
    }
    public class GLENTRY
    {
        [XmlElement("ACCOUNTNO")]
        public string AccountNo { get; set; }

        [XmlElement("TR_TYPE")]
        public int TrType { get; set; }

        [XmlElement("AMOUNT")]
        public decimal Amount { get; set; }

        [XmlElement("DESCRIPTION")]
        public string Description { get; set; }
    }

    public class Errormessage
    {

        [XmlElement("error")]
        public List<Error> Error { get; set; }
    }
    public class Error
    {

        [XmlElement("errorno")]
        public string Errorno { get; set; }

        [XmlElement("description")]
        public object Description { get; set; }

        [XmlElement("description2")]
        public string Description2 { get; set; }

        [XmlElement("correction")]
        public string Correction { get; set; }
    }
}
