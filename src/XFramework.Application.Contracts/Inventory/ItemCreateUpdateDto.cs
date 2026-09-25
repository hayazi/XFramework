using XFramework.Application.Contracts.Dtos;
using XFramework.Domain.Inventory;
using XFramework.Domain.SharedKernel;

namespace XFramework.Application.Contracts.Inventory;

public class ItemCreateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ItemType Type { get; set; }
    public UnitOfMeasure BaseUnit { get; set; }
    public CostingMethod CostingMethod { get; set; }
    public Currency Currency { get; set; } = Currency.IRR;
    public bool IsStocked { get; set; } = true;
    public bool IsPurchasable { get; set; } = true;
    public bool IsSellable { get; set; } = true;
    public bool IsProducible { get; set; } = false;
    public decimal? MinimumStock { get; set; }
    public decimal? MaximumStock { get; set; }
    public decimal? ReorderPoint { get; set; }
    public Guid? DefaultWarehouseId { get; set; }
    public string? Barcode { get; set; }
    public MoneyDto? StandardCost { get; set; }
}

public class ItemUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ItemStatus? Status { get; set; }
    public CostingMethod? CostingMethod { get; set; }
    public MoneyDto? StandardCost { get; set; }
    public bool? IsStocked { get; set; }
    public bool? IsPurchasable { get; set; }
    public bool? IsSellable { get; set; }
    public bool? IsProducible { get; set; }
    public decimal? MinimumStock { get; set; }
    public decimal? MaximumStock { get; set; }
    public decimal? ReorderPoint { get; set; }
    public Guid? DefaultWarehouseId { get; set; }
    public string? Barcode { get; set; }
}