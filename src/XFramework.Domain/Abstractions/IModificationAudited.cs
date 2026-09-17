namespace XFramework.Domain.Abstractions;

public interface IModificationAudited
{
    DateTime? LastModificationTime { get; }
    Guid? LastModifierId { get; }
}