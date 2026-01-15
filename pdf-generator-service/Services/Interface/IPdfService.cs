namespace pdf_generator_service.Services.Interface
{
    public interface IPdfService
    {
        public byte[] GenerateEncryptedPdf(string content, string password);
    }
}
