using System.Text.Json;
using XFramework.Application.Abstractions;
using XFramework.Application.Contracts.Authorization;
using XFramework.Domain.Auditing;

namespace XFramework.Application.Interceptors;

public sealed class AuditApplicationServiceInterceptor(
    IAuditStore auditStore,
    ICurrentUser currentUser)
    : IApplicationServiceInterceptor
{
    public async Task<object?> InterceptAsync(
        ApplicationServiceInvocationContext context,
        Func<Task<object?>> next,
        CancellationToken cancellationToken = default)
    {
        var timestamp = DateTime.UtcNow;
        var entityName = DetermineEntityName(context);
        var action = context.MethodName;

        object? result = null;
        Exception? exception = null;

        try
        {
            result = await next().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            if (ShouldAudit(context))
            {
                var entityId = DetermineEntityId(context, result);
                var changesJson = SerializeChanges(context, result);

                var entry = new AuditEntry
                {
                    UserId = currentUser.IsAuthenticated ? currentUser.UserId : "anonymous",
                    UserName = currentUser.IsAuthenticated ? MaskUserName(currentUser.UserName) : "anonymous",
                    EntityName = entityName,
                    EntityId = entityId,
                    Action = action,
                    Timestamp = timestamp,
                    IpAddress = null,
                    ChangesJson = changesJson
                };

                try
                {
                    await auditStore.SaveAsync(entry, cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    // Audit failure should not affect the main operation
                }
            }
        }

        return result;
    }

    private static bool ShouldAudit(ApplicationServiceInvocationContext context)
    {
        // Skip read-only operations
        if (context.IsReadOnly)
            return false;

        // Skip if method is marked with [NoAudit] attribute (if we add one later)
        return true;
    }

    private static string DetermineEntityName(ApplicationServiceInvocationContext context)
    {
        // Try to get entity name from method name (e.g., CreateCustomer -> Customer)
        var methodName = context.MethodName;
        if (methodName.StartsWith("Create", StringComparison.Ordinal))
            return methodName[6..];
        if (methodName.StartsWith("Update", StringComparison.Ordinal))
            return methodName[6..];
        if (methodName.StartsWith("Delete", StringComparison.Ordinal))
            return methodName[6..];

        // Fallback to service name
        return context.ServiceName;
    }

    private static string DetermineEntityId(ApplicationServiceInvocationContext context, object? result)
    {
        // Try to get ID from result (e.g., DTO with Id property)
        if (result is not null)
        {
            var idProp = result.GetType().GetProperty("Id");
            if (idProp is not null)
            {
                var idValue = idProp.GetValue(result);
                if (idValue is not null)
                    return idValue.ToString() ?? string.Empty;
            }
        }

        // Try to get ID from first argument (e.g., CreateCustomerDto with Id)
        if (context.Arguments.Length > 0)
        {
            var firstArg = context.Arguments[0];
            if (firstArg is not null)
            {
                var idProp = firstArg.GetType().GetProperty("Id");
                if (idProp is not null)
                {
                    var idValue = idProp.GetValue(firstArg);
                    if (idValue is not null)
                        return idValue.ToString() ?? string.Empty;
                }
            }
        }

        return Guid.NewGuid().ToString();
    }

    private static string? SerializeChanges(ApplicationServiceInvocationContext context, object? result)
    {
        var changes = new Dictionary<string, object?>();

        // Add input arguments (excluding sensitive data)
        for (int i = 0; i < context.Arguments.Length; i++)
        {
            var arg = context.Arguments[i];
            if (arg is not null)
            {
                var paramName = context.Method.GetParameters()[i].Name ?? $"arg{i}";
                if (!IsSensitiveParameter(paramName))
                {
                    changes[$"input.{paramName}"] = SanitizeForAudit(arg);
                }
            }
        }

        // Add result (excluding sensitive data)
        if (result is not null)
        {
            changes["result"] = SanitizeForAudit(result);
        }

        try
        {
            return JsonSerializer.Serialize(changes, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false
            });
        }
        catch
        {
            return "{\"serialization_error\": true}";
        }
    }

    private static bool IsSensitiveParameter(string paramName)
    {
        var sensitive = new[]
        {
            "password", "pwd", "secret", "token", "key", "credential",
            "auth", "pass", "pin", "otp", "code"
        };
        return sensitive.Any(s => paramName.Contains(s, StringComparison.OrdinalIgnoreCase));
    }

    private static object? SanitizeForAudit(object value)
    {
        if (value is string str && IsSensitiveProperty(value.GetType().Name))
        {
            return "***";
        }
        return value;
    }

    private static bool IsSensitiveProperty(string typeName)
    {
        var sensitiveTypes = new[]
        {
            "password", "secret", "token", "credential"
        };
        return sensitiveTypes.Any(s => typeName.Contains(s, StringComparison.OrdinalIgnoreCase));
    }

    private static string MaskUserName(string? userName)
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