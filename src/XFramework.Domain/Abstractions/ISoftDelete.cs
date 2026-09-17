namespace XFramework.Domain.Abstractions;

public interface ISoftDelete
{
    bool IsDeleted { get; }
}