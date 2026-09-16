namespace XFramework.Infrastructure.Messaging.RabbitMQ;
public sealed class RabbitMqTopologyHostedService
    : IHostedService
{
    private readonly RabbitMqTopology _topology;

    public RabbitMqTopologyHostedService(
        RabbitMqTopology topology)
    {
        _topology = topology;
    }

    public async Task StartAsync(
        CancellationToken cancellationToken)
    {
        await _topology.InitializeAsync(
            new[]
            {
                "accounting",
                "inventory",
                "sales"
            },
            cancellationToken);
    }

    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}