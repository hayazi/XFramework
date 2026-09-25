namespace XFramework.Domain.SharedKernel;

public readonly record struct Address : IEquatable<Address>
{
    public string Street { get; init; }
    public string City { get; init; }
    public string State { get; init; }
    public string PostalCode { get; init; }
    public string Country { get; init; }

    public Address(
        string street,
        string city,
        string state,
        string postalCode,
        string country = "Iran")
    {
        Street = street?.Trim() ?? string.Empty;
        City = city?.Trim() ?? string.Empty;
        State = state?.Trim() ?? string.Empty;
        PostalCode = postalCode?.Trim() ?? string.Empty;
        Country = country?.Trim() ?? "Iran";
    }

    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(Street) &&
        string.IsNullOrWhiteSpace(City) &&
        string.IsNullOrWhiteSpace(State) &&
        string.IsNullOrWhiteSpace(PostalCode);

    public string FullAddress =>
        string.Join(", ", new[] { Street, City, State, PostalCode, Country }.Where(s => !string.IsNullOrWhiteSpace(s)));

    public override string ToString() =>
        FullAddress;
}