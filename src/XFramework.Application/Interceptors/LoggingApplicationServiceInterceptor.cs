using System.Diagnostics;
using Microsoft.Extensions.Logging;
using XFramework.Application.Contracts.Authorization;

namespace XFramework.Application.Interceptors;

public sealed class LoggingApplicationServiceInterceptor(
    ILogger<LoggingApplicationServiceInterceptor> logger,
    ICurrentUser currentUser)
    : IApplicationServiceInterceptor
{
    public async Task<object?> InterceptAsync(
        ApplicationServiceInvocationContext context,
        Func<Task<object?>> next,
        CancellationToken cancellationToken = default)
    {
        var started = Stopwatch.GetTimestamp();

        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["Service"] = context.ServiceName,
            ["Method"] = context.MethodName,
            ["UserId"] = currentUser.IsAuthenticated ? MaskUserId(currentUser.UserId) : "anonymous",
            ["UserName"] = currentUser.IsAuthenticated ? MaskUserName(currentUser.UserName) : "anonymous",
            ["TraceId"] = Activity.Current?.TraceId.ToString() ?? string.Empty,
            ["SpanId"] = Activity.Current?.SpanId.ToString() ?? string.Empty
        });

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

    private static string? MaskUserId(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return "anonymous";

        if (Guid.TryParse(userId, out var guid))
        {
            var str = guid.ToString("N");
            return str[..8] + "***" + str[^8..];
        }

        if (userId.Length <= 4)
            return "***";

        return userId[..2] + "***" + userId[^2..];
    }

    private static string? MaskUserName(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return "anonymous";

        if (userName.Contains('@'))
        {
            var parts = userName.Split('@');
            var local = parts[0];
            if (local.Length <= 2)
                return "***@" + parts[1];
            return local[..2] + "***" + "@" + parts[1];
        }

        if (userName.Length <= 2)
            return "***";

        return userName[..2] + "***" + userName[^2..];
    }
}
