using Microsoft.Extensions.Logging;
using XFramework.Application.Contracts.Validation;

namespace XFramework.Application.Contracts.Security;

public static class SecurityEventLogger
{
    public static void LogAuthenticationFailure(
        this ILogger logger,
        string? userName,
        string? ipAddress,
        string reason)
    {
        logger.LogWarning(
            "Authentication failed for user {UserName} from {IpAddress}. Reason: {Reason}",
            MaskUserName(userName), ipAddress, reason);
    }

    public static void LogPermissionDenied(
        this ILogger logger,
        string? userId,
        string? userName,
        string permission,
        string? ipAddress)
    {
        logger.LogWarning(
            "Permission denied for user {UserId} ({UserName}) for permission {Permission} from {IpAddress}",
            MaskUserId(userId), MaskUserName(userName), permission, ipAddress);
    }

    public static void LogValidationFailure(
        this ILogger logger,
        string? userId,
        string? userName,
        string methodName,
        IReadOnlyList<ValidationError> errors,
        string? ipAddress)
    {
        logger.LogWarning(
            "Validation failed for user {UserId} ({UserName}) in {MethodName}. Errors: {Errors} from {IpAddress}",
            MaskUserId(userId), MaskUserName(userName), methodName, errors, ipAddress);
    }

    public static void LogAuthorizationException(
        this ILogger logger,
        string? userId,
        string? userName,
        string permission,
        string? ipAddress)
    {
        logger.LogWarning(
            "Authorization exception for user {UserId} ({UserName}). Required permission: {Permission} from {IpAddress}",
            MaskUserId(userId), MaskUserName(userName), permission, ipAddress);
    }

    public static void LogSuspiciousActivity(
        this ILogger logger,
        string? userId,
        string? userName,
        string activity,
        string? ipAddress,
        object? details = null)
    {
        logger.LogWarning(
            "Suspicious activity detected for user {UserId} ({UserName}): {Activity} from {IpAddress}. Details: {Details}",
            MaskUserId(userId), MaskUserName(userName), activity, ipAddress, details);
    }

    private static string? MaskUserId(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return "anonymous";

        if (Guid.TryParse(userId, out var guid))
        {
            var str = guid.ToString("N");
            return str[..8] + "***" + str[^8..];
        }

        if (userId.Length <= 4)
            return "***";

        return userId[..2] + "***" + userId[^2..];
    }

    private static string? MaskUserName(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return "anonymous";

        if (userName.Contains('@'))
        {
            var parts = userName.Split('@');
            var local = parts[0];
            if (local.Length <= 2)
                return "***@" + parts[1];
            return local[..2] + "***" + "@" + parts[1];
        }

        if (userName.Length <= 2)
            return "***";

        return userName[..2] + "***" + userName[^2..];
    }
}