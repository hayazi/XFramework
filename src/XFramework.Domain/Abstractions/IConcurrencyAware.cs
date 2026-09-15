namespace XFramework.Core.Abstractions;

public interface IConcurrencyAware
{
    byte[] RowVersion { get; }
}