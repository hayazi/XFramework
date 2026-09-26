using XFramework.Application.Contracts.Abstractions;
using XFramework.Application.Contracts.Dtos;
using XFramework.Domain.Accounting;
using XFramework.Domain.SharedKernel;

namespace XFramework.Application.Contracts.Accounting;

public class JournalLineDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public AccountNature Side { get; set; }
    public MoneyDto Amount { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public Guid? DimensionValueId { get; set; }
}

public class JournalEntryDto : EntityDto<Guid>
{
    public string Reference { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; }
    public PostingStatus PostingStatus { get; set; }
    public Guid? PartyId { get; set; }
    public string? PartyCode { get; set; }
    public List<JournalLineDto> Lines { get; set; } = new();
    public MoneyDto TotalDebit { get; set; } = new();
    public MoneyDto TotalCredit { get; set; } = new();
    public bool IsBalanced { get; set; }
    public DateTime? PostedOnUtc { get; set; }
    public Guid? PostedBy { get; set; }
}

public class JournalEntryCreateDto
{
    public string Reference { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? PartyId { get; set; }
    public List<JournalLineCreateDto> Lines { get; set; } = new();
}

public class JournalLineCreateDto
{
    public Guid AccountId { get; set; }
    public AccountNature Side { get; set; }
    public MoneyDto Amount { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public Guid? DimensionValueId { get; set; }
}

public class JournalEntryUpdateDto
{
    public string Description { get; set; } = string.Empty;
    public Guid? PartyId { get; set; }
    public List<JournalLineCreateDto> Lines { get; set; } = new();
}

public class JournalEntrySubmitDto
{
    public string Reference { get; set; } = string.Empty;
}

public class JournalEntryApproveDto
{
    public Guid ApprovedBy { get; set; }
}

public class JournalEntryRejectDto
{
    public string Reason { get; set; } = string.Empty;
}

public class JournalEntryPostDto
{
    public Guid PostedBy { get; set; }
}

public class JournalEntryReverseDto
{
    public Guid ReversedBy { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public interface IJournalEntryAppService : ICrudAppService<JournalEntryDto, Guid, JournalEntryCreateDto, JournalEntryUpdateDto>
{
    Task<JournalEntryDto?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);
    Task<List<JournalEntryDto>> GetByPartyAsync(Guid partyId, CancellationToken cancellationToken = default);
    Task<List<JournalEntryDto>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<List<JournalEntryDto>> GetByStatusAsync(DocumentStatus status, CancellationToken cancellationToken = default);
    Task<List<JournalEntryDto>> GetPostedAsync(CancellationToken cancellationToken = default);
    Task<JournalEntryDto> SubmitAsync(Guid id, JournalEntrySubmitDto input, CancellationToken cancellationToken = default);
    Task<JournalEntryDto> ApproveAsync(Guid id, JournalEntryApproveDto input, CancellationToken cancellationToken = default);
    Task<JournalEntryDto> RejectAsync(Guid id, JournalEntryRejectDto input, CancellationToken cancellationToken = default);
    Task<JournalEntryDto> PostAsync(Guid id, JournalEntryPostDto input, CancellationToken cancellationToken = default);
    Task<JournalEntryDto> ReverseAsync(Guid id, JournalEntryReverseDto input, CancellationToken cancellationToken = default);
    Task<JournalEntryDto> CancelAsync(Guid id, CancellationToken cancellationToken = default);
}