namespace XFramework.Domain.SharedKernel;

public readonly record struct ContactInfo : IEquatable<ContactInfo>
{
    public string Name { get; init; }
    public string Phone { get; init; }
    public string Email { get; init; }
    public Address? Address { get; init; }

    public ContactInfo(
        string name,
        string phone = "",
        string email = "",
        Address? address = null)
    {
        Name = name?.Trim() ?? string.Empty;
        Phone = phone?.Trim() ?? string.Empty;
        Email = email?.Trim() ?? string.Empty;
        Address = address;
    }

    public bool HasPhone => !string.IsNullOrWhiteSpace(Phone);
    public bool HasEmail => !string.IsNullOrWhiteSpace(Email);
    public bool HasAddress => Address.HasValue && !Address.Value.IsEmpty;

    public override string ToString() =>
        string.Join(" | ", new[] { Name, Phone, Email }.Where(s => !string.IsNullOrWhiteSpace(s)));
}