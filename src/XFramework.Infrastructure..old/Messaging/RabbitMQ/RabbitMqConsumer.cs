public sealed class RabbitMqConsumer
{
    private readonly IEventSerializer _serializer;
    private readonly IEventProcessor _processor;

    public RabbitMqConsumer(
        IEventSerializer serializer,
        IEventProcessor processor)
    {
        _serializer = serializer;
        _processor = processor;
    }

    public async Task HandleAsync(
        EventEnvelope envelope,
        CancellationToken cancellationToken)
    {
        var domainEvent =
            _serializer.Deserialize(
                envelope.EventType,
                envelope.EventVersion,
                envelope.Payload);

        await _processor.ProcessAsync(
            domainEvent,
            cancellationToken);
    }
}