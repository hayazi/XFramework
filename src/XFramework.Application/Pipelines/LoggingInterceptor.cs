using Microsoft.Extensions.Logging;
namespace XFramework.Application.Pipelines;

public sealed class LoggingInterceptor(ILogger<LoggingInterceptor> logger) : IApplicationServiceInterceptor
{
    public async Task InvokeAsync(ApplicationServiceInvocationContext context, Func<Task> next)
    {
        var name = $"{context.ServiceInstance.GetType().Name}.{context.Method.Name}";
        var started = Stopwatch.GetTimestamp();
        try
        {
            logger.LogInformation("Application service started: {Name}", name);
            await next();
            logger.LogInformation("Application service completed: {Name}, {ElapsedMs} ms", name, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Application service failed: {Name}", name);
            throw;
        }
    }
}