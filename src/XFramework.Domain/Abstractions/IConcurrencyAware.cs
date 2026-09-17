namespace XFramework.Domain.Abstractions;

public interface IConcurrencyAware
{
    byte[] RowVersion { get; }
}