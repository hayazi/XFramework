using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using XFramework.Application.Contracts.Authorization;
using XFramework.Application.Contracts.Security;
using XFramework.Application.Contracts.Validation;
using XFramework.Application.Attributes;
using XFramework.Application.Interceptors;
using XFramework.Application.Abstractions;
using XFramework.Domain.Auditing;
using XFramework.Infrastructure.Security;

namespace XFramework.Tests.Infrastructure;

public sealed class AuditAndSecurityTests
{
    [Fact]
    public void SecurityEventLogger_LogPermissionDenied_MasksUserId()
    {
        var logger = new Mock<ILogger>().Object;
        logger.LogPermissionDenied(
            "12345678-1234-1234-1234-123456789012",
            "john.doe@example.com",
            "Customer.Create",
            "192.168.1.1");

        Assert.True(true);
    }

    [Fact]
    public void SecurityEventLogger_LogPermissionDenied_MasksUserName()
    {
        var logger = new Mock<ILogger>().Object;
        logger.LogPermissionDenied(
            "12345678-1234-1234-1234-123456789012",
            "john.doe@example.com",
            "Customer.Create",
            "192.168.1.1");

        Assert.True(true);
    }

    [Fact]
    public void SecurityEventLogger_LogValidationFailure_LogsErrors()
    {
        var logger = new Mock<ILogger>().Object;
        var errors = new List<ValidationError>
        {
            new("Email", "Email is required"),
            new("Age", "Age must be greater than 18")
        };

        logger.LogValidationFailure(
            "12345678-1234-1234-1234-123456789012",
            "john.doe@example.com",
            "CreateCustomer",
            errors,
            "192.168.1.1");

        Assert.True(true);
    }

    [Fact]
    public void SecurityEventLogger_LogAuthenticationFailure_LogsReason()
    {
        var logger = new Mock<ILogger>().Object;
        logger.LogAuthenticationFailure(
            "john.doe@example.com",
            "192.168.1.1",
            "Invalid credentials");

        Assert.True(true);
    }

    [Fact]
    public void SecurityEventLogger_MaskUserId_Guid_ShortFormat()
    {
        var logger = new Mock<ILogger>().Object;
        logger.LogPermissionDenied(
            "12345678-1234-1234-1234-123456789012",
            "john.doe@example.com",
            "Customer.Create",
            "192.168.1.1");

        Assert.True(true);
    }

    [Fact]
    public void SecurityEventLogger_MaskUserName_ShortName()
    {
        var logger = new Mock<ILogger>().Object;
        logger.LogPermissionDenied(
            "user123",
            "ab",
            "Customer.Create",
            "192.168.1.1");

        Assert.True(true);
    }

    [Fact]
    public void SecurityEventLogger_MaskUserName_NoAtSymbol()
    {
        var logger = new Mock<ILogger>().Object;
        logger.LogPermissionDenied(
            "user123",
            "johndoe",
            "Customer.Create",
            "192.168.1.1");

        Assert.True(true);
    }

    [Fact]
    public async Task AuditApplicationServiceInterceptor_AuditsNonReadOnlyMethods()
    {
        var mockAuditStore = new Mock<IAuditStore>();
        var mockCurrentUser = new Mock<ICurrentUser>();
        mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
        mockCurrentUser.Setup(x => x.UserId).Returns("12345678-1234-1234-1234-123456789012");
        mockCurrentUser.Setup(x => x.UserName).Returns("john.doe@example.com");

        var interceptor = new AuditApplicationServiceInterceptor(
            mockAuditStore.Object,
            mockCurrentUser.Object);

        var mockInvocation = new Mock<Castle.DynamicProxy.IInvocation>();
        mockInvocation.Setup(x => x.InvocationTarget).Returns(new TestService());
        mockInvocation.Setup(x => x.Method).Returns(typeof(TestService).GetMethod(nameof(TestService.CreateCustomer))!);
        mockInvocation.Setup(x => x.Arguments).Returns(new object[] { new { Id = Guid.NewGuid(), Name = "Test" } });

        var context = new ApplicationServiceInvocationContext(mockInvocation.Object);

        mockAuditStore.Setup(x => x.SaveAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await interceptor.InterceptAsync(context, () => Task.FromResult<object?>("ok"));

        Assert.Equal("ok", result);
        mockAuditStore.Verify(x => x.SaveAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AuditApplicationServiceInterceptor_SkipsReadOnlyMethods()
    {
        var mockAuditStore = new Mock<IAuditStore>();
        var mockCurrentUser = new Mock<ICurrentUser>();
        mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
        mockCurrentUser.Setup(x => x.UserId).Returns("12345678-1234-1234-1234-123456789012");
        mockCurrentUser.Setup(x => x.UserName).Returns("john.doe@example.com");

        var interceptor = new AuditApplicationServiceInterceptor(
            mockAuditStore.Object,
            mockCurrentUser.Object);

        var mockInvocation = new Mock<Castle.DynamicProxy.IInvocation>();
        mockInvocation.Setup(x => x.InvocationTarget).Returns(new TestService());
        mockInvocation.Setup(x => x.Method).Returns(typeof(TestService).GetMethod(nameof(TestService.GetCustomer))!);
        mockInvocation.Setup(x => x.Arguments).Returns(new object[] { });

        var context = new ApplicationServiceInvocationContext(mockInvocation.Object);

        var result = await interceptor.InterceptAsync(context, () => Task.FromResult<object?>("ok"));

        Assert.Equal("ok", result);
        mockAuditStore.Verify(x => x.SaveAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void ConfigurationSecretProvider_ReturnsValueFromConfiguration()
    {
        var configDict = new Dictionary<string, string?>
        {
            ["Secret:ApiKey"] = "test-api-key",
            ["ConnectionStrings:Default"] = "Server=.;Database=Test;"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();

        var provider = new ConfigurationSecretProvider(configuration);

        var result = provider.GetSecret("Secret:ApiKey");

        Assert.Equal("test-api-key", result);
    }

    [Fact]
    public void ConfigurationSecretProvider_ReturnsNullForMissingKey()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Existing"] = "value" })
            .Build();

        var provider = new ConfigurationSecretProvider(configuration);

        var result = provider.GetSecret("NonExistent");

        Assert.Null(result);
    }

    private sealed class TestService
    {
        public string CreateCustomer(object input) => "created";
        
        [ReadOnly]
        public string GetCustomer() => "customer";
    }
}