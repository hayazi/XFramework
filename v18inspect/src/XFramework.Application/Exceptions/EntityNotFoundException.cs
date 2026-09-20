namespace XFramework.Application.Exceptions;

public sealed class EntityNotFoundException : XFrameworkException
{
    public string EntityName { get; }

    public object EntityId { get; }

    public EntityNotFoundException(
        string entityName,
        object entityId)
        : base($"{entityName} with id '{entityId}' was not found.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}