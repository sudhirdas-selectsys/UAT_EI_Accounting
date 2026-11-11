using System.Net.Http;

namespace Accounting.Utilities.Internal.Interfaces
{
    public interface IPDFOptions
    {
        byte[] ExportToPdf(string htmlContent, byte[] logoBytes, byte[] carrierLogo = null, byte[] coverPayLogo = null, string coverPayUrl = null);
        byte[] DownloadPDF(string HTMLContent, string Rootpath, List<string> CSSReferencePathList);
    }
}
