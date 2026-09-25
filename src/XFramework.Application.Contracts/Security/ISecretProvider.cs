namespace XFramework.Application.Contracts.Security;

public interface ISecretProvider
{
    string? GetSecret(string key);
    Task<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default);
}