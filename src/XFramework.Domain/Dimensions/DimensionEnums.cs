namespace XFramework.Domain.Dimensions;

public enum DimensionType
{
    CostCenter = 1,
    Project = 2,
    Department = 3,
    Region = 4,
    ProductLine = 5,
    Custom = 99
}

public enum DimensionStatus
{
    Active = 1,
    Inactive = 2,
    Closed = 3
}