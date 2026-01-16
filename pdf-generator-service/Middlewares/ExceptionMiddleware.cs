
using System.Net;
using pdf_generator_service.Enums;
using pdf_generator_service.Exceptions;
using pdf_generator_service.Models;

namespace pdf_generator_service.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}, TraceId: {TraceId}", exception.Message, context.TraceIdentifier);
            await HandleApiExceptionAsync(context, exception);
        }

        private async Task HandleApiExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, errorCode, message) = GetErrorDetails(exception);

            var response = new MessageResult
            {
                Success = false,
                ErrorCode = errorCode,
                Message = message,
                TraceId = context.TraceIdentifier
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            await context.Response.WriteAsJsonAsync(response);
        }

        private static (HttpStatusCode statusCode, ErrorCodeEnum errorCode, string message) GetErrorDetails(Exception exception)
        {
            return exception switch
            {
                BadRequestException ex => (HttpStatusCode.BadRequest, ErrorCodeEnum.InvalidInput, ex.Message),
                ArgumentException ex => (HttpStatusCode.BadRequest, ErrorCodeEnum.Conflict, ex.Message),
                InvalidOperationException ex => (HttpStatusCode.BadRequest, ErrorCodeEnum.InternalError, ex.Message),
                NotFoundException ex => (HttpStatusCode.NotFound, ErrorCodeEnum.NotFound, ex.Message),
                ServerErrorException ex => (HttpStatusCode.InternalServerError, ErrorCodeEnum.InternalError, ex.Message),
                _ => (HttpStatusCode.InternalServerError, ErrorCodeEnum.Unknown, "An unexpected error occurred")
            };
        }
    }
}