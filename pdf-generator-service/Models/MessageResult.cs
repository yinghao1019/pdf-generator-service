
using System.Text.Json.Serialization;
using pdf_generator_service.Enums;

namespace pdf_generator_service.Models
{
    public class MessageResult
    {
        public bool Success { get; set; }
        /// <summary>
        /// Error Message (for client)
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; set; }

        /// <summary>
        /// Error Code (for frontend to identify error type)
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ErrorCodeEnum? ErrorCode { get; set; }

        /// <summary>
        /// Request Trace Id (for backend to check logs)
        /// </summary>
        public string? TraceId { get; set; }

        public MessageResult()
        {
        }

        public MessageResult(bool success, string? traceId)
        {
            this.Success = success;
            this.TraceId = traceId;
        }

        public MessageResult(bool success, ErrorCodeEnum errorCode, string? message, string? traceId)
        {
            this.Success = success;
            this.ErrorCode = errorCode;
            this.Message = message;
            this.TraceId = traceId;
        }
    }
}