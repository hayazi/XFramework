using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace XFramework.EntityFrameworkCore.Outbox;

/// <summary>
/// OpenTelemetry-compatible diagnostics for the transactional outbox processor.
/// The framework emits metrics only; collection/export is configured by the host.
/// </summary>
public static class OutboxDiagnostics
{
    public const string MeterName = "XFramework.Outbox";
    public const string ActivitySourceName = "XFramework.Outbox";

    private static readonly Meter Meter = new(MeterName);

    public static readonly ActivitySource ActivitySource =
        new(ActivitySourceName);

    public static readonly Counter<long> Published =
        Meter.CreateCounter<long>("xframework.outbox.published", unit: "{event}");

    public static readonly Counter<long> Completed =
        Meter.CreateCounter<long>("xframework.outbox.completed", unit: "{message}");

    public static readonly Counter<long> Retried =
        Meter.CreateCounter<long>("xframework.outbox.retried", unit: "{message}");

    public static readonly Counter<long> Failed =
        Meter.CreateCounter<long>("xframework.outbox.failed", unit: "{message}");

    public static readonly Counter<long> LeaseRenewalLost =
        Meter.CreateCounter<long>("xframework.outbox.lease_renewal_lost", unit: "{message}");

    public static readonly Counter<long> CompletionOwnershipLost =
        Meter.CreateCounter<long>("xframework.outbox.completion_ownership_lost", unit: "{message}");

    public static readonly Counter<long> RetryOwnershipLost =
        Meter.CreateCounter<long>("xframework.outbox.retry_ownership_lost", unit: "{message}");

    public static readonly Counter<long> FailureOwnershipLost =
        Meter.CreateCounter<long>("xframework.outbox.failure_ownership_lost", unit: "{message}");
}
