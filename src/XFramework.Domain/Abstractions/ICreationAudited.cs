namespace XFramework.Domain.Abstractions;

public interface ICreationAudited
{
    DateTime CreationTime { get; }
    Guid? CreatorId { get; }
}