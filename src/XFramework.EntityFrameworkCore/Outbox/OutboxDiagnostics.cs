using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace XFramework.EntityFrameworkCore.Outbox;

/// <summary>
/// OpenTelemetry-compatible diagnostics for the transactional outbox processor.
/// The framework emits metrics only; collection/export is configured by the host.
/// All metrics use low-cardinality labels only. Never tag with EventId, aggregate IDs, or user identifiers.
/// </summary>
public static class OutboxDiagnostics
{
    public const string MeterName = "XFramework.Outbox";
    public const string ActivitySourceName = "XFramework.Outbox";

    private static readonly Meter Meter = new(MeterName);

    public static readonly ActivitySource ActivitySource =
        new(ActivitySourceName);

    public static readonly Counter<long> Published =
        Meter.CreateCounter<long>("xframework.outbox.published", unit: "{event}", description: "Number of outbox events successfully published to the event bus");

    public static readonly Counter<long> Completed =
        Meter.CreateCounter<long>("xframework.outbox.completed", unit: "{message}", description: "Number of outbox messages successfully marked completed");

    public static readonly Counter<long> Retried =
        Meter.CreateCounter<long>("xframework.outbox.retried", unit: "{message}", description: "Number of outbox messages scheduled for retry");

    public static readonly Counter<long> Failed =
        Meter.CreateCounter<long>("xframework.outbox.failed", unit: "{message}", description: "Number of outbox messages moved to terminal failed state");

    public static readonly Counter<long> LeaseRenewalLost =
        Meter.CreateCounter<long>("xframework.outbox.lease_renewal_lost", unit: "{message}", description: "Number of times lease renewal failed due to lost ownership");

    public static readonly Counter<long> CompletionOwnershipLost =
        Meter.CreateCounter<long>("xframework.outbox.completion_ownership_lost", unit: "{message}", description: "Number of times publish succeeded but completion ownership was lost");

    public static readonly Counter<long> RetryOwnershipLost =
        Meter.CreateCounter<long>("xframework.outbox.retry_ownership_lost", unit: "{message}", description: "Number of times retry ownership was lost after publish failure");

    public static readonly Counter<long> FailureOwnershipLost =
        Meter.CreateCounter<long>("xframework.outbox.failure_ownership_lost", unit: "{message}", description: "Number of times failure ownership was lost after publish failure");

    public static readonly Counter<long> LeaseExpiredRecovered =
        Meter.CreateCounter<long>("xframework.outbox.lease_expired_recovered", unit: "{message}", description: "Number of expired processing leases recovered and re-claimed");

    public static readonly Histogram<double> ProcessDuration =
        Meter.CreateHistogram<double>("xframework.outbox.process_duration", unit: "s", description: "Duration of outbox message processing including publish and completion");

    public static readonly ObservableGauge<long> PendingMessages =
        Meter.CreateObservableGauge<long>("xframework.outbox.pending_messages", () => new Measurement<long>(0), description: "Current number of pending outbox messages awaiting processing");
}
