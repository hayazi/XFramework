namespace XFramework.Application.Attributes;

[AttributeUsage(
    AttributeTargets.Method,
    Inherited = true,
    AllowMultiple = false)]
public sealed class ReadOnlyAttribute : Attribute
{
}