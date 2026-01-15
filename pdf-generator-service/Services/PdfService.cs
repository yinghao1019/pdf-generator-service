using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using pdf_generator_service.Services.Interface;

namespace pdf_generator_service.Services
{
    public class PdfService : IPdfService
    {
        public byte[] GenerateEncryptedPdf(string content, string password)
        {
            using var memoryStream = new MemoryStream();

            // 設定 PDF 寫入器與加密參數
            var writerProperties = new WriterProperties();

            // 使用相同密碼作為 userPassword 和 ownerPassword
            writerProperties.SetStandardEncryption(
                System.Text.Encoding.UTF8.GetBytes(password),      // 使用者密碼
                System.Text.Encoding.UTF8.GetBytes(password),      // 擁有者密碼（相同）
                EncryptionConstants.ALLOW_PRINTING,                 // 允許列印
                EncryptionConstants.ENCRYPTION_AES_256              // 使用 AES-256 加密
            );

            // 創建 PDF 文件
            using (var pdfWriter = new PdfWriter(memoryStream, writerProperties))
            using (var pdfDocument = new PdfDocument(pdfWriter))
            using (var document = new Document(pdfDocument))
            {
                // 添加使用者內容
                document.Add(new Paragraph(content));
                document.Add(new Paragraph("")); // 空行
                document.Add(new Paragraph($"文件生成時間: {DateTime.Now:yyyy-MM-dd HH:mm:ss}"));
            }

            return memoryStream.ToArray();
        }
    }
}
