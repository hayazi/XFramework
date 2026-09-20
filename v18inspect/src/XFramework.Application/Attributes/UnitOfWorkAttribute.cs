namespace XFramework.Application.Attributes;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,
    Inherited = true,
    AllowMultiple = false)]
public sealed class UnitOfWorkAttribute : Attribute
{
    public bool IsTransactional { get; }

    public UnitOfWorkAttribute(
        bool isTransactional = true)
    {
        IsTransactional = isTransactional;
    }
}