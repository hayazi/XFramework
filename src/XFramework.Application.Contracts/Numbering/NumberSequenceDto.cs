using XFramework.Application.Contracts.Dtos;
using XFramework.Domain.Numbering;

namespace XFramework.Application.Contracts.Numbering;

public class NumberSequenceDto : EntityDto<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public string Suffix { get; set; } = string.Empty;
    public int CurrentNumber { get; set; }
    public int MinimumDigits { get; set; }
    public int IncrementBy { get; set; }
    public int? MaximumNumber { get; set; }
    public NumberingScope Scope { get; set; }
    public string? ScopeIdentifier { get; set; }
    public NumberingStatus Status { get; set; }
    public DateTime? ResetDate { get; set; }
    public bool AutoReset { get; set; }
    public string? FormatTemplate { get; set; }
}