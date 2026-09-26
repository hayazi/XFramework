using XFramework.Application.Contracts.Outbox;

namespace XFramework.Application.Contracts.Dtos;

public sealed record OutboxStatsDto(
    int TotalMessages,
    int PendingMessages,
    int FailedMessages,
    int ProcessingMessages);