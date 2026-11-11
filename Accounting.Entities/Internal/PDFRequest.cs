namespace Accounting.Entities.Internal
{
    public class PDFRequest
    {
        public string BodyHTMLString { get; set; }
        public string HeaderHTMLString { get; set; }
        public string FooterHTMLString { get; set; }
        public bool IsCoverPageExcluded { get; set; }
    }
}
