using XFramework.Application.Contracts.Dtos;
using XFramework.Domain.Inventory;
using XFramework.Domain.SharedKernel;

namespace XFramework.Application.Contracts.Inventory;

public class KardexEntryDto : EntityDto<Guid>
{
    public Guid ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public InventoryTransactionType TransactionType { get; set; }
    public string DocumentReference { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public QuantityDto QuantityIn { get; set; } = new();
    public QuantityDto QuantityOut { get; set; } = new();
    public QuantityDto RunningBalance { get; set; } = new();
    public MoneyDto UnitCost { get; set; } = new();
    public MoneyDto TotalCost { get; set; } = new();
    public CostingMethod CostingMethod { get; set; }
    public string? Description { get; set; }
    public Guid? ReferenceDocumentId { get; set; }
    public int? ReferenceDocumentLine { get; set; }
}