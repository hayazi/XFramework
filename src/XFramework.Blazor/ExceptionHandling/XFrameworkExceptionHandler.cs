using Microsoft.Extensions.Logging;
using XFramework.Application.Contracts.Errors;
using XFramework.Application.Exceptions;

namespace XFramework.Blazor.ExceptionHandling;

public sealed class XFrameworkExceptionHandler(
    ILogger<XFrameworkExceptionHandler> logger)
{
    public ErrorInfo Handle(
        Exception exception,
        string? traceId = null)
    {
        switch (exception)
        {
            case BusinessException business:
                return new ErrorInfo
                {
                    Code = business.Code,
                    Message = business.Message,
                    TraceId = traceId
                };

            case ValidationException validation:
                return new ErrorInfo
                {
                    Code = "Validation.Error",
                    Message = validation.Message,
                    ValidationErrors = validation.Errors,
                    TraceId = traceId
                };

            case EntityNotFoundException notFound:
                return new ErrorInfo
                {
                    Code = "Entity.NotFound",
                    Message = notFound.Message,
                    TraceId = traceId
                };

            case UnauthorizedException:
                return new ErrorInfo
                {
                    Code = "Authorization.Error",
                    Message = "You are not authorized to perform this operation.",
                    TraceId = traceId
                };

            default:
                logger.LogError(
                    exception,
                    "Unhandled XFramework exception. TraceId: {TraceId}",
                    traceId);

                return new ErrorInfo
                {
                    Code = "Internal.Error",
                    Message = "An unexpected error occurred.",
                    TraceId = traceId
                };
        }
    }
}