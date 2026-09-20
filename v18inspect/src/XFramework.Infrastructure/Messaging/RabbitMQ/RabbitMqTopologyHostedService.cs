using Microsoft.Extensions.Hosting;
namespace XFramework.Infrastructure.Messaging.RabbitMQ;
public sealed class RabbitMqTopologyHostedService(RabbitMqTopology topology):IHostedService{public Task StartAsync(CancellationToken ct)=>topology.InitializeAsync(new[]{"accounting","inventory","sales"},ct);public Task StopAsync(CancellationToken ct)=>Task.CompletedTask;}
