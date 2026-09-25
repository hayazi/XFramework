namespace XFramework.Domain.SharedKernel;

public enum Currency
{
    IRR = 1,
    USD = 2,
    EUR = 3,
    GBP = 4,
    AED = 5
}

public enum UnitOfMeasure
{
    Piece = 1,
    Kilogram = 2,
    Gram = 3,
    Liter = 4,
    Milliliter = 5,
    Meter = 6,
    Centimeter = 7,
    SquareMeter = 8,
    CubicMeter = 9,
    Pack = 10,
    Box = 11,
    Roll = 12
}

public enum PartyType
{
    Customer = 1,
    Supplier = 2,
    Employee = 3,
    Prospect = 4,
    Carrier = 5,
    Bank = 6
}

public enum DocumentStatus
{
    Draft = 1,
    Submitted = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5,
    Posted = 6
}

public enum PostingStatus
{
    Unposted = 1,
    Posted = 2,
    Reversed = 3
}