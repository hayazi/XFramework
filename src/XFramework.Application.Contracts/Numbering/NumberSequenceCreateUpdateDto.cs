using XFramework.Application.Contracts.Dtos;
using XFramework.Domain.Numbering;

namespace XFramework.Application.Contracts.Numbering;

public class NumberSequenceCreateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public string Suffix { get; set; } = string.Empty;
    public int MinimumDigits { get; set; } = 6;
    public int IncrementBy { get; set; } = 1;
    public int? MaximumNumber { get; set; }
    public NumberingScope Scope { get; set; } = NumberingScope.Global;
    public string? ScopeIdentifier { get; set; }
    public bool AutoReset { get; set; } = false;
    public DateTime? ResetDate { get; set; }
    public string? FormatTemplate { get; set; }
}

public class NumberSequenceUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Prefix { get; set; }
    public string? Suffix { get; set; }
    public int? MinimumDigits { get; set; }
    public int? IncrementBy { get; set; }
    public int? MaximumNumber { get; set; }
    public NumberingStatus? Status { get; set; }
    public bool? AutoReset { get; set; }
    public DateTime? ResetDate { get; set; }
    public string? FormatTemplate { get; set; }
}