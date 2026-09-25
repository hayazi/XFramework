namespace XFramework.Domain.Tax;

public enum TaxType
{
    VAT = 1,
    SalesTax = 2,
    WithholdingTax = 3,
    ExciseTax = 4,
    CustomDuty = 5,
    Other = 99
}

public enum TaxCalculationMethod
{
    Percentage = 1,
    FixedAmount = 2,
    Tiered = 3,
    Custom = 99
}

public enum TaxApplication
{
    OnNetAmount = 1,
    OnGrossAmount = 2,
    OnQuantity = 3
}

public enum TaxStatus
{
    Active = 1,
    Inactive = 2,
    Expired = 3
}