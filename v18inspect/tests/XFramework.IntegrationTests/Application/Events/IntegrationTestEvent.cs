using XFramework.Application.Contracts.Events;
using XFramework.Domain.Events;

namespace XFramework.IntegrationTests.Application.Events;

[EventType("Integration.Test", 1, "integration.test")]
public sealed record IntegrationTestEvent(
    Guid TestId,
    string Message) : DomainEvent;
