using Microsoft.Extensions.Configuration;
using XFramework.Application.Contracts.Security;

namespace XFramework.Infrastructure.Security;

public sealed class ConfigurationSecretProvider(IConfiguration configuration) : ISecretProvider
{
    public string? GetSecret(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return configuration[key];
    }

    public Task<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return Task.FromResult(configuration[key]);
    }
}