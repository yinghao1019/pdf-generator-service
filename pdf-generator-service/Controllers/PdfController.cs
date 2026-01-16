using Microsoft.AspNetCore.Mvc;
using pdf_generator_service.Models;
using pdf_generator_service.Services.Interface;

namespace pdf_generator_service
{
    [ApiController]
    [Route("api/pdf")]
    public class PdfController : ControllerBase
    {
        private readonly IPdfService _pdfService;
        private readonly ILogger<PdfController> _logger;

        public PdfController(IPdfService pdfService, ILogger<PdfController> logger)
        {
            _pdfService = pdfService;
            _logger = logger;
        }

        /// <summary>
        /// Generate encrypted PDF and download
        /// </summary>
        /// <param name="request">Contains user content and pdf encryption password</param>
        /// <returns>Encrypted PDF document</returns>
        [HttpPost("generate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GenerateEncryptedPdf([FromBody] PdfRequestModel request)
        {
            var pdfBytes = _pdfService.GenerateEncryptedPdf(
                request.Content,
                request.Password
            );

            var fileName = $"encrypted_{DateTime.Now:yyyyMMddHHmmss}.pdf";

            _logger.LogInformation("PDF generated successfully, file name: {FileName}, size: {Size} bytes",
                fileName, pdfBytes.Length);
            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}
