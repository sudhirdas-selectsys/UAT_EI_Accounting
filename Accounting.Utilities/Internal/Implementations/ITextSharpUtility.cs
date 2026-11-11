using Accounting.Utilities.Internal.Interfaces;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml.pipeline.html;
using System.Text;

namespace Accounting.Utilities.Internal.Implementations
{
    public class ITextSharpUtility : IPDFOptions
    {
        static ITextSharpUtility()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        public byte[] ExportToPdf(string htmlContent, byte[] logoBytes = null, byte[] carrierLogo = null, byte[] coverPayLogo = null, string coverPayUrl = null)
        {
            using (MemoryStream outputStream = new MemoryStream())
            {
                float logoReservedHeight = 10f; // must be >= tallest possible logo + padding
                using (Document document = new Document(PageSize.A4, 0, 0, logoReservedHeight, 0))
                {
                    PdfWriter writer = PdfWriter.GetInstance(document, outputStream);
                    document.Open();

                    float appendedX = 0f;
                    float appendedY = 0f;

                    // ---- Original Logo (UNCHANGED) ----
                    //if (logoBytes != null && logoBytes.Length > 0)
                    //{
                    //    var img = iTextSharp.text.Image.GetInstance(logoBytes);
                    //    img.ScaleToFit(100f, 100f);

                    //    float x = 15f;
                    //    float y = document.PageSize.Height - img.ScaledHeight - 15f;

                    //    img.SetAbsolutePosition(x, y);
                    //    writer.DirectContent.AddImage(img);

                    //    // Capture next position ONLY for carrier logo
                    //    appendedX = x + img.ScaledWidth + 10f; // 10px gap
                    //    appendedY = y;
                    //}

                    //// ---- Carrier Logo (ONLY APPENDED if exists) ----
                    //if (carrierLogo != null && carrierLogo.Length > 0 && appendedX > 0)
                    //{
                    //    var carrierImg = iTextSharp.text.Image.GetInstance(carrierLogo);
                    //    carrierImg.ScaleToFit(100f, 100f);
                    //    carrierImg.SetAbsolutePosition(appendedX, appendedY);
                    //    writer.DirectContent.AddImage(carrierImg);
                    //}

                    // ---- Determine which logo to draw ----
                    byte[] logoToDraw = null;

                    if (carrierLogo != null && carrierLogo.Length > 0)
                    {
                        logoToDraw = carrierLogo;
                    }
                    else
                    {
                        if (logoBytes?.Length > 0)
                        {
                            logoToDraw = logoBytes;
                        }
                    }

                    if (logoToDraw != null)
                    {
                        var img = iTextSharp.text.Image.GetInstance(logoToDraw);

                        // ---- Target display size ----
                        float targetHeight = 80f;   // increase if you want bigger (eg: 90f)
                        float maxWidth = 180f;      // prevents very wide logos

                        // Scale based on height first
                        float heightScale = targetHeight / img.Height;
                        float scaledWidth = img.Width * heightScale;

                        // If width exceeds max, scale based on width instead
                        if (scaledWidth > maxWidth)
                        {
                            float widthScale = maxWidth / img.Width;
                            img.ScalePercent(widthScale * 100);
                        }
                        else
                        {
                            img.ScalePercent(heightScale * 100);
                        }

                        // ---- Position ----
                        float x = 15f;
                        float y = document.PageSize.Height - img.ScaledHeight - 10f;

                        img.SetAbsolutePosition(x, y);
                        writer.DirectContent.AddImage(img);
                    }

                    // CoverPay Logo
                    if (coverPayLogo != null && coverPayLogo.Length > 0 && !string.IsNullOrEmpty(coverPayUrl))
                    {
                        var coverPayImg = iTextSharp.text.Image.GetInstance(coverPayLogo);

                        float targetHeight = 35f;
                        float maxWidth = 150f;

                        float heightScale = targetHeight / coverPayImg.Height;
                        float scaledWidth = coverPayImg.Width * heightScale;

                        if (scaledWidth > maxWidth)
                        {
                            float widthScale = maxWidth / coverPayImg.Width;
                            coverPayImg.ScalePercent(widthScale * 100);
                        }
                        else
                        {
                            coverPayImg.ScalePercent(heightScale * 100);
                        }

                        // Position
                        float x = (document.PageSize.Width - coverPayImg.ScaledWidth) / 2;
                        float y = 150f;

                        coverPayImg.SetAbsolutePosition(x, y);

                        // Add image
                        writer.DirectContent.AddImage(coverPayImg);

                        // Add hyperlink over image
                        PdfAnnotation link = PdfAnnotation.CreateLink(
                            writer,
                            new Rectangle(
                                x,
                                y,
                                x + coverPayImg.ScaledWidth,
                                y + coverPayImg.ScaledHeight
                            ),
                            PdfAnnotation.HIGHLIGHT_INVERT,
                            new PdfAction(coverPayUrl)
                        );

                        writer.AddAnnotation(link);
                    }

                    // ---- HTML Content ----
                    using (TextReader reader = new StringReader(htmlContent))
                    {
                        XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, reader);
                    }

                    document.Close();
                }

                return outputStream.ToArray();
            }
        }


        public byte[] DownloadPDF(string HTMLContent, string Rootpath, List<string> CSSReferencePathList)
        {
            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                Document pdfDoc = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();
                using (var strReader = new StringReader(HTMLContent))
                {
                    //Set factories
                    HtmlPipelineContext htmlContext = new HtmlPipelineContext(null);
                    htmlContext.SetTagFactory(Tags.GetHtmlTagProcessorFactory());
                    if (CSSReferencePathList != null && CSSReferencePathList.Count > 0)
                    {
                        ICSSResolver cssResolver = XMLWorkerHelper.GetInstance().GetDefaultCssResolver(false);
                        foreach (var cssRefPath in CSSReferencePathList)
                        {
                            cssResolver.AddCssFile(Path.Combine(Rootpath, cssRefPath), true);
                        }
                        IPipeline pipeline = new CssResolverPipeline(cssResolver, new HtmlPipeline(htmlContext, new PdfWriterPipeline(pdfDoc, writer)));
                        var worker = new XMLWorker(pipeline, true);
                        var xmlParse = new XMLParser(true, worker);
                        xmlParse.Parse(strReader);
                        xmlParse.Flush();
                    }
                }
                pdfDoc.Close();
                return stream.ToArray();
            }
        }
    }
}
