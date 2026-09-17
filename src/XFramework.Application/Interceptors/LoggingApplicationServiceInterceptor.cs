using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace XFramework.Application.Interceptors;

public sealed class LoggingApplicationServiceInterceptor(
    ILogger<LoggingApplicationServiceInterceptor> logger)
    : IApplicationServiceInterceptor
{
    public async Task<object?> InterceptAsync(
        ApplicationServiceInvocationContext context,
        Func<Task<object?>> next,
        CancellationToken cancellationToken = default)
    {
        var started = Stopwatch.GetTimestamp();

        logger.LogDebug(
            "Application service started: {Service}.{Method}",
            context.ServiceName,
            context.MethodName);

        try
        {
            var result = await next().ConfigureAwait(false);

            logger.LogDebug(
                "Application service completed: {Service}.{Method} in {ElapsedMs} ms",
                context.ServiceName,
                context.MethodName,
                Stopwatch.GetElapsedTime(started).TotalMilliseconds);

            return result;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Application service failed: {Service}.{Method} after {ElapsedMs} ms",
                context.ServiceName,
                context.MethodName,
                Stopwatch.GetElapsedTime(started).TotalMilliseconds);

            throw;
        }
    }
}
