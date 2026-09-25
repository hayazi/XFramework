namespace XFramework.Domain.Numbering;

public enum NumberingScope
{
    Company = 1,
    Branch = 2,
    Warehouse = 3,
    User = 4,
    Global = 5
}

public enum NumberingStatus
{
    Active = 1,
    Inactive = 2,
    Exhausted = 3
}