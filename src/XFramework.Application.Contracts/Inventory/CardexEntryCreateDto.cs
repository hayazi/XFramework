using XFramework.Application.Contracts.Dtos;
using XFramework.Domain.Inventory;
using XFramework.Domain.SharedKernel;

namespace XFramework.Application.Contracts.Inventory;

public class CardexEntryCreateDto
{
    public Guid ItemId { get; set; }
    public Guid WarehouseId { get; set; }
    public InventoryTransactionType TransactionType { get; set; }
    public string DocumentReference { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public QuantityDto Quantity { get; set; } = new();
    public MoneyDto UnitCost { get; set; } = new();
    public CostingMethod CostingMethod { get; set; }
    public string? Description { get; set; }
    public Guid? ReferenceDocumentId { get; set; }
    public int? ReferenceDocumentLine { get; set; }
}