using Microsoft.AspNetCore.Mvc;
using pdf_generator_service.Models;
using pdf_generator_service.Services.Interface;

namespace pdf_generator_service
{
    [ApiController]
    [Route("api/[controller]")]
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
        /// 產生加密的 PDF 並下載
        /// </summary>
        /// <param name="request">包含使用者內文和加密密碼</param>
        /// <returns>加密的 PDF 文件</returns>
        [HttpPost("generate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GenerateEncryptedPdf([FromBody] PdfRequestModel request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("開始產生加密 PDF，內容長度: {ContentLength}", request.Content.Length);

                var pdfBytes = _pdfService.GenerateEncryptedPdf(
                    request.Content,
                    request.Password
                );

                var fileName = $"encrypted_document_{DateTime.Now:yyyyMMddHHmmss}.pdf";

                _logger.LogInformation("PDF 產生成功，檔案名稱: {FileName}, 大小: {Size} bytes",
                    fileName, pdfBytes.Length);
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "產生加密 PDF 時發生錯誤");
                return StatusCode(500, new
                {
                    message = "產生 PDF 時發生錯誤",
                    error = ex.Message
                });
            }
        }
    }
}
