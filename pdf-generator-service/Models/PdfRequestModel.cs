using System.ComponentModel.DataAnnotations;

namespace pdf_generator_service.Models
{
    public class PdfRequestModel
    {

        /// <summary>
        /// User Content
        /// </summary>
        [Required]
        public string Content { get; set; } = string.Empty;
        /// <summary>
        /// encryption password
        /// </summary>
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
