using XFramework.Application.Contracts.Dtos;

namespace XFramework.Application.Contracts.Customers;

public class CustomerDto : EntityDto<Guid>
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }
}