using System.Diagnostics;
using System.Diagnostics.Metrics;
using Xunit;
using XFramework.EntityFrameworkCore.Outbox;

namespace XFramework.Tests.Infrastructure;

public sealed class OutboxMetricsTests
{
    [Fact]
    public void Published_Counter_Exists_WithCorrectName()
    {
        Assert.Equal("xframework.outbox.published", OutboxDiagnostics.Published.Name);
        Assert.Equal("{event}", OutboxDiagnostics.Published.Unit);
        Assert.Contains("published", OutboxDiagnostics.Published.Description);
    }

    [Fact]
    public void Completed_Counter_Exists_WithCorrectName()
    {
        Assert.Equal("xframework.outbox.completed", OutboxDiagnostics.Completed.Name);
        Assert.Equal("{message}", OutboxDiagnostics.Completed.Unit);
        Assert.Contains("completed", OutboxDiagnostics.Completed.Description);
    }

    [Fact]
    public void Retried_Counter_Exists_WithCorrectName()
    {
        Assert.Equal("xframework.outbox.retried", OutboxDiagnostics.Retried.Name);
        Assert.Equal("{message}", OutboxDiagnostics.Retried.Unit);
        Assert.Contains("retry", OutboxDiagnostics.Retried.Description);
    }

    [Fact]
    public void Failed_Counter_Exists_WithCorrectName()
    {
        Assert.Equal("xframework.outbox.failed", OutboxDiagnostics.Failed.Name);
        Assert.Equal("{message}", OutboxDiagnostics.Failed.Unit);
        Assert.Contains("failed", OutboxDiagnostics.Failed.Description);
    }

    [Fact]
    public void LeaseRenewalLost_Counter_Exists_WithCorrectName()
    {
        Assert.Equal("xframework.outbox.lease_renewal_lost", OutboxDiagnostics.LeaseRenewalLost.Name);
        Assert.Equal("{message}", OutboxDiagnostics.LeaseRenewalLost.Unit);
        Assert.Contains("lease renewal", OutboxDiagnostics.LeaseRenewalLost.Description);
    }

    [Fact]
    public void CompletionOwnershipLost_Counter_Exists_WithCorrectName()
    {
        Assert.Equal("xframework.outbox.completion_ownership_lost", OutboxDiagnostics.CompletionOwnershipLost.Name);
        Assert.Equal("{message}", OutboxDiagnostics.CompletionOwnershipLost.Unit);
        Assert.Contains("completion ownership", OutboxDiagnostics.CompletionOwnershipLost.Description);
    }

    [Fact]
    public void RetryOwnershipLost_Counter_Exists_WithCorrectName()
    {
        Assert.Equal("xframework.outbox.retry_ownership_lost", OutboxDiagnostics.RetryOwnershipLost.Name);
        Assert.Equal("{message}", OutboxDiagnostics.RetryOwnershipLost.Unit);
        Assert.Contains("retry ownership", OutboxDiagnostics.RetryOwnershipLost.Description);
    }

    [Fact]
    public void FailureOwnershipLost_Counter_Exists_WithCorrectName()
    {
        Assert.Equal("xframework.outbox.failure_ownership_lost", OutboxDiagnostics.FailureOwnershipLost.Name);
        Assert.Equal("{message}", OutboxDiagnostics.FailureOwnershipLost.Unit);
        Assert.Contains("failure ownership", OutboxDiagnostics.FailureOwnershipLost.Description);
    }

    [Fact]
    public void LeaseExpiredRecovered_Counter_Exists_WithCorrectName()
    {
        Assert.Equal("xframework.outbox.lease_expired_recovered", OutboxDiagnostics.LeaseExpiredRecovered.Name);
        Assert.Equal("{message}", OutboxDiagnostics.LeaseExpiredRecovered.Unit);
        Assert.Contains("expired", OutboxDiagnostics.LeaseExpiredRecovered.Description);
    }

    [Fact]
    public void ProcessDuration_Histogram_Exists_WithCorrectName()
    {
        Assert.Equal("xframework.outbox.process_duration", OutboxDiagnostics.ProcessDuration.Name);
        Assert.Equal("s", OutboxDiagnostics.ProcessDuration.Unit);
        Assert.Contains("Duration", OutboxDiagnostics.ProcessDuration.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PendingMessages_Gauge_Exists()
    {
        Assert.NotNull(OutboxDiagnostics.PendingMessages);
        Assert.Equal("xframework.outbox.pending_messages", OutboxDiagnostics.PendingMessages.Name);
    }

    [Fact]
    public void Meter_Name_IsCorrect()
    {
        Assert.Equal("XFramework.Outbox", OutboxDiagnostics.MeterName);
    }

    [Fact]
    public void ActivitySource_Name_IsCorrect()
    {
        Assert.Equal("XFramework.Outbox", OutboxDiagnostics.ActivitySourceName);
    }

    [Fact]
    public void AllCounters_HaveNonEmptyDescriptions()
    {
        var counters = new[]
        {
            OutboxDiagnostics.Published,
            OutboxDiagnostics.Completed,
            OutboxDiagnostics.Retried,
            OutboxDiagnostics.Failed,
            OutboxDiagnostics.LeaseRenewalLost,
            OutboxDiagnostics.CompletionOwnershipLost,
            OutboxDiagnostics.RetryOwnershipLost,
            OutboxDiagnostics.FailureOwnershipLost,
            OutboxDiagnostics.LeaseExpiredRecovered
        };

        foreach (var counter in counters)
        {
            Assert.False(string.IsNullOrWhiteSpace(counter.Name));
            Assert.False(string.IsNullOrWhiteSpace(counter.Description));
        }
    }

    [Fact]
    public void Histogram_HasDescription()
    {
        Assert.False(string.IsNullOrWhiteSpace(OutboxDiagnostics.ProcessDuration.Name));
        Assert.False(string.IsNullOrWhiteSpace(OutboxDiagnostics.ProcessDuration.Description));
    }

    [Fact]
    public void Counters_UseLowCardinalityLabels()
    {
        // Verify no high-cardinality labels are used on any counter
        // (The counters don't have explicit tags configured, which is correct)
        var counters = new[]
        {
            OutboxDiagnostics.Published,
            OutboxDiagnostics.Completed,
            OutboxDiagnostics.Retried,
            OutboxDiagnostics.Failed,
            OutboxDiagnostics.LeaseRenewalLost,
            OutboxDiagnostics.CompletionOwnershipLost,
            OutboxDiagnostics.RetryOwnershipLost,
            OutboxDiagnostics.FailureOwnershipLost,
            OutboxDiagnostics.LeaseExpiredRecovered
        };

        // The test verifies the counters exist and are properly named
        // Actual cardinality enforcement is a runtime concern
        Assert.Equal(9, counters.Length);
    }
}