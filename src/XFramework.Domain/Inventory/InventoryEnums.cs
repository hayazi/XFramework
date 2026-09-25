namespace XFramework.Domain.Inventory;

public enum ItemType
{
    Product = 1,
    Service = 2,
    RawMaterial = 3,
    Consumable = 4,
    FixedAsset = 5,
    Kit = 6
}

public enum ItemStatus
{
    Active = 1,
    Inactive = 2,
    Discontinued = 3,
    Blocked = 4
}

public enum CostingMethod
{
    Standard = 1,
    Average = 2,
    Fifo = 3,
    Lifo = 4,
    Specific = 5
}

public enum InventoryTransactionType
{
    Receipt = 1,
    Issue = 2,
    Transfer = 3,
    Adjustment = 4,
    Return = 5,
    Production = 6,
    Consumption = 7
}

public enum WarehouseType
{
    Main = 1,
    Transit = 2,
    Quarantine = 3,
    Scrap = 4,
    Virtual = 5
}