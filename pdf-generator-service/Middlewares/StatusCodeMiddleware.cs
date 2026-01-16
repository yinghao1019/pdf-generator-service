

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using pdf_generator_service.Exceptions;
using pdf_generator_service.Models;

namespace pdf_generator_service.Middlewares
{
    public class StatusCodeMiddleware
    {
        private readonly RequestDelegate _next;

        public StatusCodeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {

            await _next(context);

            if (context.Response.StatusCode < 400)
            {
                return;
            }

            switch (context.Response.StatusCode)
            {
                case 400:
                case 404:
                case 500:
                    return;
                case 401:
                    await new JsonResult(new MessageResult
                    {
                        Success = false,
                        ErrorCode = Enums.ErrorCodeEnum.UnAuthentication,
                        Message = $"Unauthorized: {context.Request.Path}",
                        TraceId = Activity.Current?.Id ?? context.TraceIdentifier
                    })
                    {
                        StatusCode = StatusCodes.Status401Unauthorized
                    }.ExecuteResultAsync(new ActionContext { HttpContext = context });
                    return;
                case 403:
                    await new JsonResult(new MessageResult
                    {
                        Success = false,
                        ErrorCode = Enums.ErrorCodeEnum.UnAuthorization,
                        Message = $"Forbidden: {context.Request.Path}",
                        TraceId = Activity.Current?.Id ?? context.TraceIdentifier
                    })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    }.ExecuteResultAsync(new ActionContext { HttpContext = context });
                    return;
                case 405:
                    throw new BadRequestException("No Route");
                case 415:
                    throw new BadRequestException("Accept application/json");
                default:
                    throw new ServerErrorException(ReasonPhrases.GetReasonPhrase(context.Response.StatusCode));
            }
        }
    }
}