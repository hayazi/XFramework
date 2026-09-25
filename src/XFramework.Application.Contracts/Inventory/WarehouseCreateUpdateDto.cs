using XFramework.Application.Contracts.Dtos;
using XFramework.Domain.Inventory;
using XFramework.Domain.SharedKernel;

namespace XFramework.Application.Contracts.Inventory;

public class WarehouseCreateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WarehouseType Type { get; set; }
    public AddressDto Address { get; set; } = new();
    public bool AllowNegativeStock { get; set; } = false;
    public Guid? ManagerId { get; set; }
}

public class WarehouseUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WarehouseType? Type { get; set; }
    public AddressDto? Address { get; set; }
    public bool? AllowNegativeStock { get; set; }
    public Guid? ManagerId { get; set; }
}