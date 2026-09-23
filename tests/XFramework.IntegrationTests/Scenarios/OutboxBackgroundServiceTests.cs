using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XFramework.Application.Outbox;
using XFramework.Infrastructure.Outbox;
using Xunit;

namespace XFramework.IntegrationTests.Scenarios;

public sealed class OutboxBackgroundServiceTests
{
    [Fact]
    public async Task Worker_should_create_a_scope_and_process_until_shutdown()
    {
        var firstCall = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var processor = new RecordingOutboxProcessor(firstCall);
        await using var provider = BuildProvider(processor);
        using var service = CreateService(provider);

        await service.StartAsync(CancellationToken.None);
        await firstCall.Task.WaitAsync(TimeSpan.FromSeconds(2));

        await service.StopAsync(CancellationToken.None);

        Assert.Equal(1, processor.CallCount);
        Assert.Equal(100, processor.BatchSizes.Single());
    }

    [Fact]
    public async Task Worker_should_continue_after_a_transient_processor_failure()
    {
        var secondCall = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var processor = new RecordingOutboxProcessor(secondCall)
        {
            FailFirstCall = true
        };

        await using var provider = BuildProvider(processor);
        using var service = CreateService(provider);

        await service.StartAsync(CancellationToken.None);
        await secondCall.Task.WaitAsync(TimeSpan.FromSeconds(4));

        await service.StopAsync(CancellationToken.None);

        Assert.True(processor.CallCount >= 2);
        Assert.All(processor.BatchSizes, batchSize => Assert.Equal(100, batchSize));
    }

    [Fact]
    public async Task Worker_should_stop_cleanly_when_processor_observes_cancellation()
    {
        var processor = new CancellationAwareOutboxProcessor();
        await using var provider = BuildProvider(processor);
        using var service = CreateService(provider);

        await service.StartAsync(CancellationToken.None);
        await processor.Started.Task.WaitAsync(TimeSpan.FromSeconds(2));

        await service.StopAsync(CancellationToken.None);

        Assert.True(processor.CancellationObserved);
        Assert.Equal(1, processor.CallCount);
    }

    private static ServiceProvider BuildProvider(IOutboxProcessor processor)
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.None));
        services.AddScoped<IOutboxProcessor>(_ => processor);
        return services.BuildServiceProvider();
    }

    private static OutboxBackgroundService CreateService(IServiceProvider provider)
    {
        return new OutboxBackgroundService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<ILogger<OutboxBackgroundService>>(),
            Options.Create(new OutboxOptions
            {
                BatchSize = 100,
                PollingIntervalSeconds = 1,
                LeaseMinutes = 2
            }));
    }

    private sealed class RecordingOutboxProcessor(
        TaskCompletionSource completion) : IOutboxProcessor
    {
        private int _callCount;
        private readonly List<int> _batchSizes = [];

        public bool FailFirstCall { get; set; }
        public int CallCount => Volatile.Read(ref _callCount);
        public IReadOnlyList<int> BatchSizes => _batchSizes;

        public Task ProcessBatchAsync(
            int batchSize,
            CancellationToken cancellationToken = default)
        {
            var call = Interlocked.Increment(ref _callCount);
            lock (_batchSizes)
            {
                _batchSizes.Add(batchSize);
            }

            if (call == 1 && FailFirstCall)
                throw new InvalidOperationException("Transient outbox failure.");

            completion.TrySetResult();
            return Task.CompletedTask;
        }
    }

    private sealed class CancellationAwareOutboxProcessor : IOutboxProcessor
    {
        public TaskCompletionSource Started { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int CallCount { get; private set; }
        public bool CancellationObserved { get; private set; }

        public async Task ProcessBatchAsync(
            int batchSize,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            Started.TrySetResult();

            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                CancellationObserved = true;
                throw;
            }
        }
    }
}
