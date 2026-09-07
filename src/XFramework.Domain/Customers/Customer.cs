using XFramework.Core.Domain;

namespace XFramework.Domain.Customers;

public class Customer : Entity<Guid>
{
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Phone { get; private set; }

    protected Customer()
    {
    }

    public Customer(
        string code,
        string name,
        string? phone = null)
    {
        Id = Guid.NewGuid();

        Code = code;
        Name = name;
        Phone = phone;
    }

    public void Update(
        string name,
        string? phone)
    {
        Name = name;
        Phone = phone;
    }
}