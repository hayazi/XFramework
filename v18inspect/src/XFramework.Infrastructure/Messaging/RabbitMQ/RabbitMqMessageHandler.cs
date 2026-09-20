using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using XFramework.Application.Contracts.Events;
using XFramework.Application.Events;
namespace XFramework.Infrastructure.Messaging.RabbitMQ;
public sealed class RabbitMqMessageHandler(IEventProcessor processor,IEventRetryPolicy retryPolicy,IEventRetryPublisher retryPublisher,IEventDeadLetterPublisher dlq,ILogger<RabbitMqMessageHandler> logger)
{ public async Task HandleAsync(IChannel channel,BasicDeliverEventArgs args,CancellationToken ct){var body=args.Body.ToArray();EventEnvelope? env=null;try{env=JsonSerializer.Deserialize<EventEnvelope>(body);}catch(JsonException ex){await dlq.PublishRawAsync(body,args.RoutingKey,ex,ct);await channel.BasicAckAsync(args.DeliveryTag,false,ct);return;}if(env is null){var ex=new InvalidOperationException("Event envelope is null.");await dlq.PublishRawAsync(body,args.RoutingKey,ex,ct);await channel.BasicAckAsync(args.DeliveryTag,false,ct);return;}try{await processor.ProcessAsync(env,ct);await channel.BasicAckAsync(args.DeliveryTag,false,ct);}catch(OperationCanceledException) when(ct.IsCancellationRequested){throw;}catch(Exception ex){if(retryPolicy.ShouldRetry(env.RetryCount,ex)){await retryPublisher.PublishRetryAsync(env,args.RoutingKey,retryPolicy.GetDelay(env.RetryCount),ct);await channel.BasicAckAsync(args.DeliveryTag,false,ct);logger.LogWarning(ex,"Event {EventId} scheduled for retry.",env.EventId);}else{await dlq.PublishAsync(env,args.RoutingKey,ex,ct);await channel.BasicAckAsync(args.DeliveryTag,false,ct);logger.LogError(ex,"Event {EventId} moved to DLQ.",env.EventId);}}} }
