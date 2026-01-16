using pdf_generator_service.Models;

namespace pdf_generator_service.Services.Interface
{
    public interface IPdfService
    {
        public byte[] GeneratePdf(PdfModel pdfModel);
    }
}
