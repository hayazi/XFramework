namespace XFramework.Core.Abstractions;

public interface IModificationAudited
{
    DateTime? LastModificationTime { get; }
    Guid? LastModifierId { get; }
}