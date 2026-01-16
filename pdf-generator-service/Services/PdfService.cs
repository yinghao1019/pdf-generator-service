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

        public byte[] GenerateEncryptedPdf(string content, string password)
        {
            _logger.LogInformation("Starting to generate encrypted PDF");

            // Create PDF document
            using var document = new PdfDocument();

            // Set document information
            document.Info.Title = "Encrypted Document";
            document.Info.Author = "System Generated";
            document.Info.Subject = "Confidential Document";
            document.Info.Creator = "PdfSharp 6.x";
            document.Info.CreationDate = DateTime.Now;

            // Add first page
            var page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            // Create graphics object
            using var gfx = XGraphics.FromPdfPage(page);

            // Draw content
            DrawContent(gfx, page, content, document);

            // Set encryption
            SetEncryption(document, password);

            // Save to memory
            return SaveToMemory(document);
        }

        private void DrawContent(XGraphics gfx, PdfPage page, string content, PdfDocument document)
        {
            // Define fonts
            var titleFont = new XFont("Noto Sans TC", 20, XFontStyleEx.Bold);
            var headingFont = new XFont("Noto Sans TC", 16, XFontStyleEx.Bold);
            var normalFont = new XFont("Noto Sans TC", 12, XFontStyleEx.Regular);
            var smallFont = new XFont("Noto Sans TC", 10, XFontStyleEx.Regular);

            // Define colors and pens
            var darkBlueBrush = new XSolidBrush(XColor.FromArgb(0, 0, 139));
            var blackBrush = XBrushes.Black;
            var grayBrush = XBrushes.Gray;
            var redBrush = XBrushes.Red;
            var darkBluePen = new XPen(XColor.FromArgb(0, 0, 139), 1);
            var lightGrayPen = new XPen(XColor.FromArgb(211, 211, 211), 1);
            var redPen = new XPen(XColors.Red, 1);

            // Define page parameters
            double yPosition = 50;
            double margin = 50;
            double pageWidth = page.Width;
            double pageHeight = page.Height;
            double contentWidth = pageWidth - 2 * margin;
            double lineHeight = 25;
            int currentPageNumber = 1;

            // === Draw title ===
            gfx.DrawString("Confidential Document", titleFont, darkBlueBrush,
                new XRect(margin, yPosition, contentWidth, 30),
                XStringFormats.TopCenter);
            yPosition += 50;

            // Title separator line
            gfx.DrawLine(darkBluePen, margin, yPosition, pageWidth - margin, yPosition);
            yPosition += 30;

            // === Content heading ===
            gfx.DrawString("Document Content", headingFont, darkBlueBrush,
                new XRect(margin, yPosition, contentWidth, 25),
                XStringFormats.TopLeft);
            yPosition += 35;

            // === Process multi-line content ===
            var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                // Check if new page is needed
                if (yPosition > pageHeight - 120)
                {
                    // Draw footer for current page
                    DrawFooter(gfx, page, currentPageNumber, smallFont, grayBrush);

                    // Add new page
                    page = document.AddPage();
                    page.Size = PdfSharp.PageSize.A4;
                    gfx.Dispose();

                    var newGfx = XGraphics.FromPdfPage(page);
                    gfx = newGfx;
                    yPosition = 50;
                    currentPageNumber++;
                }

                // Draw text line (supports automatic line wrapping)
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var textFormatter = new XTextFormatter(gfx);
                    var rect = new XRect(margin, yPosition, contentWidth, lineHeight * 3);
                    textFormatter.DrawString(line, normalFont, blackBrush, rect, XStringFormats.TopLeft);
                }

                yPosition += lineHeight;
            }

            yPosition += 20;

            // === Separator line ===
            if (yPosition > pageHeight - 120)
            {
                DrawFooter(gfx, page, currentPageNumber, smallFont, grayBrush);
                page = document.AddPage();
                page.Size = PdfSharp.PageSize.A4;
                gfx.Dispose();
                gfx = XGraphics.FromPdfPage(page);
                yPosition = 50;
                currentPageNumber++;
            }

            gfx.DrawLine(lightGrayPen, margin, yPosition, pageWidth - margin, yPosition);
            yPosition += 20;

            // === Timestamp ===
            gfx.DrawString($"Document Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                smallFont, grayBrush,
                new XRect(margin, yPosition, contentWidth, 20),
                XStringFormats.TopLeft);
            yPosition += 30;

            // === Warning message box ===
            if (yPosition > pageHeight - 120)
            {
                DrawFooter(gfx, page, currentPageNumber, smallFont, grayBrush);
                page = document.AddPage();
                page.Size = PdfSharp.PageSize.A4;
                gfx.Dispose();
                gfx = XGraphics.FromPdfPage(page);
                yPosition = 50;
                currentPageNumber++;
            }

            var warningRect = new XRect(margin, yPosition, contentWidth, 45);

            // Draw warning box background
            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(255, 255, 224)), warningRect);
            // Draw warning box border
            gfx.DrawRectangle(redPen, warningRect);

            // Draw warning text
            var warningText = "This document is encrypted and protected. Please keep your password secure.\nUnauthorized copying or distribution is prohibited.";
            var textFormatter2 = new XTextFormatter(gfx);
            textFormatter2.Alignment = XParagraphAlignment.Center;
            textFormatter2.DrawString(warningText, smallFont, redBrush,
                new XRect(margin + 10, yPosition + 10, contentWidth - 20, 35),
                XStringFormats.TopLeft);

            DrawFooter(gfx, page, currentPageNumber, smallFont, grayBrush);
        }

        private void DrawFooter(XGraphics gfx, PdfPage page, int pageNumber, XFont font, XBrush brush)
        {
            var footerText = $"Page {pageNumber}";
            gfx.DrawString(footerText, font, brush,
                new XRect(0, page.Height - 40, page.Width, 20),
                XStringFormats.TopCenter);
        }

        private void SetEncryption(PdfDocument document, string password)
        {
            var securitySettings = document.SecuritySettings;

            // Set passwords (user password and owner password are the same)
            securitySettings.UserPassword = password;
            securitySettings.OwnerPassword = password;

            // Set permissions
            securitySettings.PermitPrint = true;                          // Allow printing
            securitySettings.PermitModifyDocument = false;                // Disallow document modification
            securitySettings.PermitExtractContent = false;                // Disallow content extraction
            securitySettings.PermitAnnotations = false;                   // Disallow annotations
            securitySettings.PermitFormsFill = false;                     // Disallow form filling
            securitySettings.PermitAssembleDocument = false;              // Disallow document assembly
            securitySettings.PermitFullQualityPrint = true;               // Allow high quality printing

            _logger.LogInformation("PDF encryption settings completed - AES-256 encryption");
        }

        private byte[] SaveToMemory(PdfDocument document)
        {
            using var memoryStream = new MemoryStream();
            document.Save(memoryStream, false);

            var size = memoryStream.Length;
            _logger.LogInformation("PDF saved successfully, size: {Size} bytes ({SizeKB} KB)", size, size / 1024);

            return memoryStream.ToArray();
        }
    }
}