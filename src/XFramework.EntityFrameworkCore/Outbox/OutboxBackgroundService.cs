using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace XFramework.EntityFrameworkCore.Outbox;

public sealed class OutboxBackgroundService
    : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxBackgroundService> _logger;

    public OutboxBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "XFramework Outbox background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var processor =
                    scope.ServiceProvider
                        .GetRequiredService<OutboxProcessor>();

                await processor.ProcessAsync(
                    stoppingToken);

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error in XFramework Outbox worker.");

                await Task.Delay(
                    TimeSpan.FromSeconds(10),
                    stoppingToken);
            }
        }

        _logger.LogInformation(
            "XFramework Outbox background service stopped.");
    }
}