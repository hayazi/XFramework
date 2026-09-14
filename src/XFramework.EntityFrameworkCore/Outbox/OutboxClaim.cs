namespace XFramework.EntityFrameworkCore.Outbox;

public sealed record OutboxClaim(
    OutboxMessage Message,
    string LockId);