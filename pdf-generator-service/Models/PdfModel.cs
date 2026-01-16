using System.ComponentModel.DataAnnotations;

namespace pdf_generator_service.Models
{
    public class PdfModel
    {
        /// <summary>
        /// Pdf Title
        /// </summary>
        public string Title { get; set; } = "Confidential Document";
        /// <summary>
        /// Pdf Content
        /// </summary>
        [Required]
        public string Content { get; set; } = string.Empty;
        /// <summary>
        /// encryption password
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}
