using System;
using System.IO;
using System.Text.RegularExpressions;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml.pipeline.html;

namespace Accounting.SchedulerJobs.Utilities
{
    public static class HtmlPdfConverter
    {
        // Void elements that must be self-closed for XMLWorker's XHTML parser
        private static readonly string[] VoidElements =
        {
            "area", "base", "br", "col", "embed", "hr", "img",
            "input", "link", "meta", "param", "source", "track", "wbr"
        };

        /// <summary>
        /// Converts an HTML string to a PDF byte array.
        /// Logos and images must be embedded in the HTML as base64 data URIs.
        /// </summary>
        /// <param name="htmlContent">Well-formed or loose HTML to render as PDF.</param>
        /// <returns>PDF file contents as a byte array.</returns>
        /// <exception cref="ArgumentException">Thrown when htmlContent is null or whitespace.</exception>
        public static byte[] ConvertToPdf(string htmlContent, byte[] clientLogo = null)
        {
            string tempLogoPath = null;
            try
            {
                FileLogger.LogInformation("HtmlPdfConverter: ConvertToPdf Started");

                if (string.IsNullOrWhiteSpace(htmlContent))
                {
                    FileLogger.LogInformation("HtmlPdfConverter: HTML content is null or empty.");
                    return null;
                }

                string sanitized = SanitizeToXhtml(htmlContent);
                //                sanitized = @"<html>
                //<head>
                //    <title>Courtesy Payment Reminder</title>
                //</head>
                //<body style=""font-family: Arial, sans-serif; font-size: 14px; color: #000; background: #fff; margin: 0; padding: 0;"">

                //    <div style=""width: 90%; margin: 40px auto; padding: 40px 50px; border: 1px solid #ddd; box-sizing: border-box;"">
                //        <table style=""width:100%; border-bottom: 3px solid #000; 
                //              margin-bottom: 30px; padding-bottom: 15px;"">
                //            <tr>
                //           <td style=""vertical-align: top;"">
                //                <img src=""data:image/jpeg;base64,[ClientLogo]"" alt=""ClientLogo"" style=""width: 160px;"" />
                //            </td>

                //             <td style=""text-align: right; font-size: 12px; 
                //                   line-height: 1.6; color: #333; vertical-align: top;"">
                //                [CompanyAddress]
                //<br/>
                //                <a href=""https://[CompanyWebSite]"" target=""_blank"" style=""color: #1a0dab; text-decoration: underline;"">
                //                    [CompanyWebSite]
                //                </a>
                //             </td>
                //            </tr>
                //       </table>
                //        <div style=""margin-bottom: 30px; font-size: 14px; font-weight: bold; line-height: 1.8;"">
                //            [CurrentDate]

                //            [AgencyAddress]
                //        </div>

                //        <div style=""text-align: center; font-size: 18px; font-weight: bold; text-decoration: underline; margin-bottom: 35px; letter-spacing: 0.5px;"">
                //            COURTESY PAYMENT REMINDER
                //        </div>

                //        <div>
                //            <p style=""font-weight: bold; margin-bottom: 20px; line-height: 1.6;"">
                //                Please be advised your payment for the referenced policy is due tomorrow.
                //            </p>

                //            <p style=""font-weight: bold; margin-bottom: 20px; line-height: 1.6;"">
                //                Payment must be made by the due date to avoid any additional fees or possible cancellations.
                //            </p>

                //            <p style=""font-weight: bold; margin-bottom: 20px; line-height: 1.6;"">
                //                If you have any questions, please contact our office.
                //            </p>
                //        </div>

                //        <div style=""margin-top: 30px;"">
                //            <p style=""font-weight: bold; margin-bottom: 6px;"">
                //                Payment Link:
                //            </p>

                //            <a href=""[PaymentLink]"" target=""_blank""
                //               style=""color: #1a0dab; text-decoration: underline; font-size: 14px;"">
                //                [PaymentLink]
                //            </a>
                //        </div>
                //        <div style=""margin-top: 50px;"">
                //            <p style=""font-weight: bold; margin-bottom: 15px;"">
                //                Sincerely,
                //            </p>

                //            <img src=""data:image/jpeg;base64,[ClientLogo]"" alt=""ClientLogo"" style=""width: 160px;"" />
                //        </div>
                //    </div>

                //</body>
                //</html>";

                if (clientLogo != null)
                {
                    tempLogoPath = Path.Combine(Path.GetTempPath(), $"logo_{Guid.NewGuid()}.png");
                    File.WriteAllBytes(tempLogoPath, clientLogo);
                    string imageUrl = new Uri(tempLogoPath).AbsoluteUri;
                    sanitized = sanitized.Replace("[ClientLogo]", imageUrl);
                }
                
                using (var outputStream = new MemoryStream())
                {
                    using (var document = new Document(PageSize.A4, 36, 36, 36, 36))
                    {
                        PdfWriter writer = PdfWriter.GetInstance(document, outputStream);
                        document.Open();
                        using (var reader = new StringReader(sanitized))
                        {
                            try
                            {
                                XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, reader);
                            }
                            catch (Exception ex)
                            {
                                FileLogger.LogError($"HtmlPdfConverter: XMLWorker render failed: {ex.Message}");
                                string plainText = Regex.Replace(htmlContent, "<.*?>", string.Empty);
                                document.Add(new Paragraph($"[Render error: {ex.Message}]\n\n{plainText}"));
                            }
                        }
                        if (writer.PageNumber == 0)
                            document.NewPage();

                        document.Close();
                    }
                    string temp = Convert.ToBase64String(outputStream.ToArray());
                    return outputStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"HtmlPdfConverter: ConvertToPdf Exception: {ex.Message}");
                return null;
            }
            finally
            {
                try
                {
                    if (!string.IsNullOrEmpty(tempLogoPath) && File.Exists(tempLogoPath))
                    {
                        File.Delete(tempLogoPath);
                    }
                     
                }
                catch(Exception ex)
                {
                    FileLogger.LogError($"HtmlPdfConverter: ConvertToPdf TempLogoPath: {tempLogoPath} Exception: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Normalises arbitrary HTML into the strict XHTML that iTextSharp XMLWorker requires
        /// without any third-party dependency — uses only regex and string operations.
        /// Steps: ensure root element → strip DOCTYPE → self-close void elements → remove XML declaration.
        /// </summary>
        private static string SanitizeToXhtml(string html)
        {
            try
            {
                string s = html.Trim();

                // 1. Ensure a single <html> root element
                //if (!s.StartsWith("<html", StringComparison.OrdinalIgnoreCase))
                //    s = $"<html><body>{s}</body></html>";

                // 2. Remove DOCTYPE — XMLWorker's XHTML parser rejects it
                s = Regex.Replace(s, @"<!DOCTYPE[^>]*>", string.Empty,
                        RegexOptions.IgnoreCase | RegexOptions.Singleline);

                // 3. Self-close void elements that XMLWorker requires to be XHTML-style
                //    Matches e.g. <br>, <br >, <br class="x"> but NOT <br/> or <br />
                foreach (string tag in VoidElements)
                {
                    // Match opening tags NOT already self-closed (no trailing / before >)
                    s = Regex.Replace(
                        s,
                        $@"<({tag})(\s[^>]*)?(?<!/)>",   // fixed: lookbehind on char before >
                        m =>
                        {
                            string inner = m.Value.TrimEnd('>').TrimEnd('/').TrimEnd();
                            return inner + " />";
                        },
                        RegexOptions.IgnoreCase);
                }

                // 4. Remove any leading XML declaration (<?xml ... ?>)
                s = Regex.Replace(s, @"<\?xml[^?]*\?>", string.Empty,
                        RegexOptions.IgnoreCase).TrimStart();

                return s;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}