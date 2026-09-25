using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using XFramework.Application.Contracts.Outbox;
using XFramework.Application.Abstractions;
using XFramework.EntityFrameworkCore.Outbox;
using EFOutboxMessageStatus = XFramework.EntityFrameworkCore.Outbox.OutboxMessageStatus;
using ContractOutboxMessageStatus = XFramework.Application.Contracts.Outbox.OutboxMessageStatus;

namespace XFramework.Tests.Infrastructure;

public sealed class OutboxAdminServiceTests
{
    private readonly Mock<IOutboxRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<OutboxAdminService>> _loggerMock = new();
    private readonly OutboxAdminService _service;

    public OutboxAdminServiceTests()
    {
        _service = new OutboxAdminService(_repositoryMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetPendingAsync_ReturnsMappedSummaries()
    {
        var messages = new List<OutboxMessage>
        {
            CreateMessage(EFOutboxMessageStatus.Pending),
            CreateMessage(EFOutboxMessageStatus.Pending)
        };
        _repositoryMock.Setup(r => r.GetPendingAsync(0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages);

        var result = await _service.GetPendingAsync(0, 10);

        Assert.Equal(2, result.Count);
        Assert.All(result, m => Assert.Equal(ContractOutboxMessageStatus.Pending, m.Status));
    }

    [Fact]
    public async Task GetFailedAsync_ReturnsMappedSummaries()
    {
        var messages = new List<OutboxMessage>
        {
            CreateMessage(EFOutboxMessageStatus.Failed)
        };
        _repositoryMock.Setup(r => r.GetFailedAsync(0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages);

        var result = await _service.GetFailedAsync(0, 10);

        Assert.Single(result);
        Assert.Equal(ContractOutboxMessageStatus.Failed, result[0].Status);
    }

    [Fact]
    public async Task GetProcessingAsync_ReturnsMappedSummaries()
    {
        var messages = new List<OutboxMessage>
        {
            CreateMessage(EFOutboxMessageStatus.Processing)
        };
        _repositoryMock.Setup(r => r.GetProcessingAsync(0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages);

        var result = await _service.GetProcessingAsync(0, 10);

        Assert.Single(result);
        Assert.Equal(ContractOutboxMessageStatus.Processing, result[0].Status);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDetail_WhenFound()
    {
        var message = CreateMessage(EFOutboxMessageStatus.Failed, retryCount: 3, error: "Test error");
        _repositoryMock.Setup(r => r.GetByIdAsync(message.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);

        var result = await _service.GetByIdAsync(message.Id);

        Assert.NotNull(result);
        Assert.Equal(message.Id, result!.Id);
        Assert.Equal(message.EventId, result.EventId);
        Assert.Equal(message.RetryCount, result.RetryCount);
        Assert.Equal(message.LastError, result.LastError);
        Assert.Equal(message.Payload, result.Payload);
        Assert.Equal(message.Headers, result.Headers);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OutboxMessage?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task RetryAsync_ReturnsFalse_WhenMessageNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OutboxMessage?)null);

        var result = await _service.RetryAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task RetryAsync_ReturnsFalse_WhenNotInFailedStatus()
    {
        var message = CreateMessage(EFOutboxMessageStatus.Pending);
        _repositoryMock.Setup(r => r.GetByIdAsync(message.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);

        var result = await _service.RetryAsync(message.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task RetryAsync_CallsMarkRetryAsync_WhenValidFailedMessage()
    {
        var message = CreateMessage(EFOutboxMessageStatus.Failed, lockId: "lock-123");
        _repositoryMock.Setup(r => r.GetByIdAsync(message.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);
        _repositoryMock.Setup(r => r.MarkRetryAsync(
            message.Id, "lock-123", It.IsAny<DateTime>(), It.IsAny<DateTime>(), "Manual retry triggered by admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var mockTransaction = new Mock<IUnitOfWorkTransaction>();
        mockTransaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockTransaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        var result = await _service.RetryAsync(message.Id);

        Assert.True(result);
        _repositoryMock.Verify(r => r.MarkRetryAsync(
            message.Id, "lock-123", It.IsAny<DateTime>(), It.IsAny<DateTime>(), "Manual retry triggered by admin", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RetryAsync_ReturnsFalse_WhenMarkRetryFails()
    {
        var message = CreateMessage(EFOutboxMessageStatus.Failed, lockId: "lock-123");
        _repositoryMock.Setup(r => r.GetByIdAsync(message.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);
        _repositoryMock.Setup(r => r.MarkRetryAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var mockTransaction = new Mock<IUnitOfWorkTransaction>();
        mockTransaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        var result = await _service.RetryAsync(message.Id);

        Assert.False(result);
        mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ForceCompleteAsync_ReturnsFalse_WhenMessageNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OutboxMessage?)null);

        var result = await _service.ForceCompleteAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task ForceCompleteAsync_ReturnsTrue_WhenAlreadyCompleted()
    {
        var message = CreateMessage(EFOutboxMessageStatus.Completed);
        _repositoryMock.Setup(r => r.GetByIdAsync(message.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);

        var result = await _service.ForceCompleteAsync(message.Id);

        Assert.True(result);
        _repositoryMock.Verify(r => r.MarkCompletedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ForceCompleteAsync_CallsMarkCompletedAsync_WhenNotCompleted()
    {
        var message = CreateMessage(EFOutboxMessageStatus.Failed, lockId: "lock-123");
        _repositoryMock.Setup(r => r.GetByIdAsync(message.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);
        _repositoryMock.Setup(r => r.MarkCompletedAsync(
            message.Id, "lock-123", It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var mockTransaction = new Mock<IUnitOfWorkTransaction>();
        mockTransaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockTransaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        var result = await _service.ForceCompleteAsync(message.Id);

        Assert.True(result);
        _repositoryMock.Verify(r => r.MarkCompletedAsync(
            message.Id, "lock-123", It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ForceCompleteAsync_UsesAdminLockId_WhenLockIdIsNull()
    {
        var message = CreateMessage(EFOutboxMessageStatus.Failed, lockId: null);
        _repositoryMock.Setup(r => r.GetByIdAsync(message.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);
        _repositoryMock.Setup(r => r.MarkCompletedAsync(
            message.Id, "admin-override", It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var mockTransaction = new Mock<IUnitOfWorkTransaction>();
        mockTransaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockTransaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        var result = await _service.ForceCompleteAsync(message.Id);

        Assert.True(result);
        _repositoryMock.Verify(r => r.MarkCompletedAsync(
            message.Id, "admin-override", It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStatsAsync_ReturnsCorrectCounts()
    {
        _repositoryMock.Setup(r => r.GetCountByStatusAsync(EFOutboxMessageStatus.Pending, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);
        _repositoryMock.Setup(r => r.GetCountByStatusAsync(EFOutboxMessageStatus.Processing, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);
        _repositoryMock.Setup(r => r.GetCountByStatusAsync(EFOutboxMessageStatus.Completed, It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);
        _repositoryMock.Setup(r => r.GetCountByStatusAsync(EFOutboxMessageStatus.Failed, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var result = await _service.GetStatsAsync();

        Assert.Equal(5, result.PendingCount);
        Assert.Equal(3, result.ProcessingCount);
        Assert.Equal(100, result.CompletedCount);
        Assert.Equal(2, result.FailedCount);
    }

    private static OutboxMessage CreateMessage(
        EFOutboxMessageStatus status,
        int retryCount = 0,
        string? lockId = "lock-123",
        string? error = null)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            EventType = "Test.Event",
            EventVersion = 1,
            Payload = "{}",
            Status = status,
            RetryCount = retryCount,
            NextAttemptOnUtc = status == EFOutboxMessageStatus.Pending ? DateTime.UtcNow.AddMinutes(5) : null,
            LockId = lockId,
            LockedUntilUtc = status == EFOutboxMessageStatus.Processing ? DateTime.UtcNow.AddMinutes(2) : null,
            LastError = error,
            CreatedOnUtc = DateTime.UtcNow.AddMinutes(-10),
            OccurredOnUtc = DateTime.UtcNow.AddMinutes(-10),
            ProcessedOnUtc = status == EFOutboxMessageStatus.Completed ? DateTime.UtcNow : null,
            FailedOnUtc = status == EFOutboxMessageStatus.Failed ? DateTime.UtcNow : null,
            CorrelationId = Guid.NewGuid().ToString(),
            CausationId = Guid.NewGuid().ToString(),
            AggregateType = "TestAggregate",
            AggregateId = Guid.NewGuid().ToString(),
            ModuleName = "TestModule",
            Headers = "{}"
        };
    }
}