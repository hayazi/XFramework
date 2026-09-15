namespace XFramework.Core.Results;

public class Result
{
    public bool IsSuccess { get; protected set; }

    public string? ErrorCode { get; protected set; }

    public string? ErrorMessage { get; protected set; }

    public static Result Success()
        => new()
        {
            IsSuccess = true
        };

    public static Result Failure(
        string code,
        string message)
        => new()
        {
            IsSuccess = false,
            ErrorCode = code,
            ErrorMessage = message
        };
}