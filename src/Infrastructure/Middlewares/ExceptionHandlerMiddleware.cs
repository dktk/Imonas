using System.Net;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;

using SG.Common;

namespace Infrastructure.Middlewares;

internal class ExceptionHandlerMiddleware : IMiddleware
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(
        ICurrentUserService currentUserService,
        ILogger<ExceptionHandlerMiddleware> logger
       )
    {
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {

            var userId = _currentUserService.UserId;
            if (!string.IsNullOrEmpty(userId)) LogContext.PushProperty("UserId", userId);
            string errorId = Guid.NewGuid().ToString();
            LogContext.PushProperty("ErrorId", errorId);
            LogContext.PushProperty("StackTrace", exception.StackTrace);
            var responseModel = Result<bool>.CreateFailure(new string[] { exception.Message });
            var response = context.Response;
            response.ContentType = "application/json";
            if (exception.InnerException != null)
            {
                while (exception.InnerException != null)
                {
                    exception = exception.InnerException;
                }
            }

            switch (exception)
            {
                case ValidationException e:
                    response.StatusCode =  (int)HttpStatusCode.BadRequest;
                    responseModel = Result<bool>.CreateFailure(e.Errors.Select(x=>$"{x.Key}:{string.Join(',',x.Value)}"));
                    break;
                case NotFoundException e:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    responseModel = Result<bool>.CreateFailure(new string[] {e.Message });
                    break;
                case KeyNotFoundException:
                    response.StatusCode =  (int)HttpStatusCode.NotFound;
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    responseModel = Result<bool>.CreateFailure(new string[] { exception.Message });
                    break;
            }
            //_logger.LogError(exception,$"Request failed with Status Code {response.StatusCode} and Error Id {errorId}.");
            await response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(responseModel));
        }
    }
}
