namespace XFramework.Application.Contracts.Errors;

public sealed class ErrorInfo
{
    public string Code { get; init; } = "XFramework.Error";

    public string Message { get; init; } = string.Empty;

    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; init; }

    public string? TraceId { get; init; }
}