namespace XFramework.Application.Contracts.Customers;

public class CustomerCreateDto
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }
}