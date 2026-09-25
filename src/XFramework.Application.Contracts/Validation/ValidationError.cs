namespace XFramework.Application.Contracts.Validation;

public sealed record ValidationError(
    string PropertyName,
    string Message,
    string? Code = null);