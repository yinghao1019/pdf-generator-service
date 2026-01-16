
using pdf_generator_service.Models;
using pdf_generator_service.Services.Interface;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;

namespace pdf_generator_service.Services
{
    public class PdfService : IPdfService
    {
        private readonly ILogger<PdfService> _logger;
        public PdfService(ILogger<PdfService> logger)
        {
            _logger = logger;
        }

        public byte[] GeneratePdf(PdfModel pdfModel)
        {
            _logger.LogInformation("Starting to generate encrypted PDF");
            // Create PDF document
            using var document = new PdfDocument();
            // Set document information
            document.Info.Title = "Encrypted PDF";
            document.Info.Author = "Fii Common System Generated";
            document.Info.CreationDate = DateTime.Now;
            // Add first page
            var page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            // Draw content
            DrawContent(page, pdfModel, document);
            if (!string.IsNullOrWhiteSpace(pdfModel.Password))
            {
                SetEncryption(document, pdfModel.Password);
            }
            // Save to memory
            return SaveToMemory(document);
        }

        private void DrawContent(PdfPage page, PdfModel pdfModel, PdfDocument document)
        {
            XGraphics? gfx = null;
            try
            {
                gfx = XGraphics.FromPdfPage(page);

                // Define fonts
                var titleFont = new XFont("Noto Sans TC", 20, XFontStyleEx.Bold);
                var normalFont = new XFont("Noto Sans TC", 12, XFontStyleEx.Regular);
                var smallFont = new XFont("Noto Sans TC", 10, XFontStyleEx.Regular);

                // Define colors
                var darkBlueBrush = new XSolidBrush(XColor.FromArgb(0, 0, 139));
                var blackBrush = XBrushes.Black;
                var grayBrush = XBrushes.Gray;
                var redBrush = XBrushes.Red;
                var darkBluePen = new XPen(XColor.FromArgb(0, 0, 139), 1);
                var lightGrayPen = new XPen(XColor.FromArgb(211, 211, 211), 1);
                var redPen = new XPen(XColors.Red, 1);

                double yPosition = 50;
                double margin = 50;
                double pageWidth = page.Width.Point;
                double pageHeight = page.Height.Point;
                double contentWidth = pageWidth - 2 * margin;
                double lineHeight = 25;
                int currentPageNumber = 1;

                // Draw title
                gfx.DrawString(pdfModel.Title, titleFont, darkBlueBrush,
                    new XRect(margin, yPosition, contentWidth, 30),
                    XStringFormats.TopCenter);
                yPosition += 50;

                gfx.DrawLine(darkBluePen, margin, yPosition, pageWidth - margin, yPosition);
                yPosition += 30;

                // Process content, using \n as line breaks
                var lines = pdfModel.Content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    if (yPosition > pageHeight - 120)
                    {
                        DrawFooter(gfx, page, currentPageNumber, smallFont, grayBrush);
                        // create new page if not enough space
                        gfx.Dispose();
                        page = document.AddPage();
                        page.Size = PdfSharp.PageSize.A4;
                        gfx = XGraphics.FromPdfPage(page);

                        yPosition = 50;
                        pageWidth = page.Width.Point;
                        pageHeight = page.Height.Point;
                        currentPageNumber++;
                    }

                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var textFormatter = new XTextFormatter(gfx);
                        var rect = new XRect(margin, yPosition, contentWidth, lineHeight * 1.5);
                        textFormatter.DrawString(line, normalFont, blackBrush, rect, XStringFormats.TopLeft);
                    }

                    yPosition += lineHeight;
                }

                yPosition += 20;

                // Draw final elements
                if (yPosition > pageHeight - 120)
                {
                    DrawFooter(gfx, page, currentPageNumber, smallFont, grayBrush);
                    // create new page if not enough space
                    gfx.Dispose();
                    page = document.AddPage();
                    page.Size = PdfSharp.PageSize.A4;
                    gfx = XGraphics.FromPdfPage(page);
                    yPosition = 50;
                    currentPageNumber++;
                }

                gfx.DrawLine(lightGrayPen, margin, yPosition, page.Width.Point - margin, yPosition);
                yPosition += 20;

                gfx.DrawString($"PDF Document Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                    smallFont, grayBrush,
                    new XRect(margin, yPosition, contentWidth, 20),
                    XStringFormats.TopLeft);
                yPosition += 30;

                // Warning box
                var warningRect = new XRect(margin, yPosition, contentWidth, 45);
                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(255, 255, 224)), warningRect);
                gfx.DrawRectangle(redPen, warningRect);

                var warningText = "This document is encrypted and protected. Please keep your password secure.";
                var textFormatter2 = new XTextFormatter(gfx);
                textFormatter2.Alignment = XParagraphAlignment.Center;
                textFormatter2.DrawString(warningText, smallFont, redBrush,
                    new XRect(margin + 10, yPosition + 10, contentWidth - 20, 35),
                    XStringFormats.TopLeft);

                DrawFooter(gfx, page, currentPageNumber, smallFont, grayBrush);
            }
            finally
            {
                gfx?.Dispose();
            }
        }
        private void DrawFooter(XGraphics gfx, PdfPage page, int pageNumber, XFont font, XBrush brush)
        {
            var footerText = $"Page {pageNumber}";
            gfx.DrawString(footerText, font, brush,
                new XRect(0, page.Height.Point - 40, page.Width.Point, 20),
                XStringFormats.TopCenter);
        }

        private void SetEncryption(PdfDocument document, string password)
        {
            var securitySettings = document.SecuritySettings;
            // default using aes 128
            // Set passwords (user password and owner password are the same)
            securitySettings.UserPassword = password;
            securitySettings.OwnerPassword = password;

            // Set permissions
            // Allow printing
            securitySettings.PermitPrint = true;
            // Disallow document modification
            securitySettings.PermitModifyDocument = false;
            // Disallow content extraction
            securitySettings.PermitExtractContent = false;
            // Disallow annotations
            securitySettings.PermitAnnotations = false;
            // Disallow form filling
            securitySettings.PermitFormsFill = false;
            // Disallow document assembly
            securitySettings.PermitAssembleDocument = false;
            // Allow high quality printing
            securitySettings.PermitFullQualityPrint = true;

            _logger.LogInformation("PDF encryption settings completed - AES-128 encryption");
        }

        private byte[] SaveToMemory(PdfDocument document)
        {
            using var memoryStream = new MemoryStream();
            document.Save(memoryStream, false);

            var size = memoryStream.Length;
            return memoryStream.ToArray();
        }
    }
}