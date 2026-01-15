using System.ComponentModel.DataAnnotations;

namespace pdf_generator_service.Models
{
    public class PdfRequestModel
    {

        /// <summary>
        /// 使用者內文
        /// </summary>
        [Required(ErrorMessage = "使用者內文不能為空")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 加密密碼
        /// </summary>
        [Required(ErrorMessage = "加密密碼不能為空")]
        [MinLength(6, ErrorMessage = "密碼至少需要 6 個字元")]
        public string Password { get; set; } = string.Empty;
    }
}
