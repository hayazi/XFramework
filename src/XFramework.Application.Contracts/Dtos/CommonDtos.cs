using XFramework.Domain.SharedKernel;

namespace XFramework.Application.Contracts.Dtos;

public class MoneyDto
{
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
}

public class QuantityDto
{
    public decimal Value { get; set; }
    public UnitOfMeasure Unit { get; set; }
}

public class AddressDto
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = "Iran";
}