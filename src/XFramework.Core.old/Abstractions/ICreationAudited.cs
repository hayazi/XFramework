namespace XFramework.Core.Abstractions;

public interface ICreationAudited
{
    DateTime CreationTime { get; }
    Guid? CreatorId { get; }
}