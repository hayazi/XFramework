using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using XFramework.Domain.SharedKernel;
using System.Text.Json;

namespace XFramework.EntityFrameworkCore.ValueConverters;

public class MoneyConverter : ValueConverter<Money, string>
{
    public MoneyConverter() : base(
        v => JsonSerializer.Serialize(v),
        v => JsonSerializer.Deserialize<Money>(v)!) { }
}

public class MoneyNullableConverter : ValueConverter<Money?, string>
{
    public MoneyNullableConverter() : base(
        v => v.HasValue ? JsonSerializer.Serialize(v.Value) : "null",
        v => v == "null" ? null : JsonSerializer.Deserialize<Money>(v)) { }
}

public class QuantityConverter : ValueConverter<Quantity, string>
{
    public QuantityConverter() : base(
        v => JsonSerializer.Serialize(v),
        v => JsonSerializer.Deserialize<Quantity>(v)!) { }
}

public class AddressConverter : ValueConverter<Address, string>
{
    public AddressConverter() : base(
        v => JsonSerializer.Serialize(v),
        v => JsonSerializer.Deserialize<Address>(v)!) { }
}

public class PercentageConverter : ValueConverter<Percentage, string>
{
    public PercentageConverter() : base(
        v => JsonSerializer.Serialize(v),
        v => JsonSerializer.Deserialize<Percentage>(v)!) { }
}