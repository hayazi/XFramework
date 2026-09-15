public sealed class RabbitMqRetryTopology
{
    private readonly RabbitMqOptions _options;

    public RabbitMqRetryTopology(
        RabbitMqOptions options)
    {
        _options = options;
    }

    public async Task DeclareAsync(
        IChannel channel,
        CancellationToken cancellationToken = default)
    {
        await DeclareDeadLetterExchangeAsync(
            channel,
            cancellationToken);

        await DeclareModuleAsync(
            channel,
            "accounting",
            cancellationToken);

        await DeclareModuleAsync(
            channel,
            "inventory",
            cancellationToken);

        await DeclareModuleAsync(
            channel,
            "sales",
            cancellationToken);
    }
}