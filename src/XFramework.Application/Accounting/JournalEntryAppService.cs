using XFramework.Application.Abstractions;
using XFramework.Application.Attributes;
using XFramework.Application.Contracts.Accounting;
using XFramework.Application.Contracts.Dtos;
using XFramework.Application.Services;
using XFramework.Domain.Accounting;
using XFramework.Domain.SharedKernel;

namespace XFramework.Application.Accounting;

[Validate]
public sealed class JournalEntryAppService(IRepository<JournalEntry, Guid> repository, IUnitOfWork unitOfWork, IRepository<Account, Guid> accountRepository)
    : CrudAppService<JournalEntry, JournalEntryDto, Guid, JournalEntryCreateDto, JournalEntryUpdateDto>(repository, unitOfWork), IJournalEntryAppService
{
    private readonly IRepository<Account, Guid> _accountRepository = accountRepository;

    protected override JournalEntryDto MapToDto(JournalEntry e)
    {
        var accountIds = e.Lines.Select(l => l.AccountId).Distinct().ToList();
        var accounts = _accountRepository.GetQueryable().Where(a => accountIds.Contains(a.Id)).ToList();
        var accountDict = accounts.ToDictionary(a => a.Id);

        var lines = new List<JournalLineDto>();
        foreach (var line in e.Lines)
        {
            var account = accountDict.GetValueOrDefault(line.AccountId);
            lines.Add(new JournalLineDto
            {
                AccountId = line.AccountId,
                AccountCode = account?.Code ?? string.Empty,
                Side = line.Side,
                Amount = new MoneyDto { Amount = line.Amount.Amount, Currency = line.Amount.Currency },
                Description = line.Description,
                DimensionValueId = line.DimensionValueId
            });
        }

        return new JournalEntryDto
        {
            Id = e.Id,
            Reference = e.Reference,
            Date = e.Date,
            Description = e.Description,
            Status = e.Status,
            PostingStatus = e.PostingStatus,
            PartyId = e.PartyId,
            Lines = lines,
            TotalDebit = new MoneyDto { Amount = e.TotalDebit.Amount, Currency = e.TotalDebit.Currency },
            TotalCredit = new MoneyDto { Amount = e.TotalCredit.Amount, Currency = e.TotalCredit.Currency },
            IsBalanced = e.IsBalanced,
            PostedOnUtc = e.PostedOnUtc,
            PostedBy = e.PostedBy
        };
    }

    protected override Task<JournalEntry> MapToEntityAsync(JournalEntryCreateDto i, CancellationToken ct)
    {
        var entry = JournalEntry.Create(i.Reference, i.Date, i.Description, i.PartyId);

        foreach (var line in i.Lines)
        {
            entry.AddLine(line.AccountId, line.Side, new Money(line.Amount.Amount, line.Amount.Currency), line.Description, line.DimensionValueId);
        }

        return Task.FromResult(entry);
    }

    protected override Task MapToEntityAsync(JournalEntryUpdateDto i, JournalEntry e, CancellationToken ct)
    {
        if (e.Status != DocumentStatus.Draft)
            throw new InvalidOperationException("Only draft entries can be updated.");

        e.ClearLines();

        foreach (var line in i.Lines)
        {
            e.AddLine(line.AccountId, line.Side, new Money(line.Amount.Amount, line.Amount.Currency), line.Description, line.DimensionValueId);
        }

        // Note: Description and PartyId are immutable after creation in the domain model.
        // To change them, the entry should be cancelled and a new one created.

        return Task.CompletedTask;
    }

    public async Task<JournalEntryDto?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Reference == reference);
        var entity = await Repository.FirstOrDefaultAsync(query, cancellationToken);
        return entity is null ? default : MapToDto(entity);
    }

    public async Task<List<JournalEntryDto>> GetByPartyAsync(Guid partyId, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.PartyId == partyId);
        var entities = await Repository.ToListAsync(query, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<List<JournalEntryDto>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Date >= fromDate && x.Date <= toDate);
        var entities = await Repository.ToListAsync(query, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<List<JournalEntryDto>> GetByStatusAsync(DocumentStatus status, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Status == status);
        var entities = await Repository.ToListAsync(query, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<List<JournalEntryDto>> GetPostedAsync(CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Status == DocumentStatus.Posted);
        var entities = await Repository.ToListAsync(query, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<JournalEntryDto> SubmitAsync(Guid id, JournalEntrySubmitDto input, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Journal entry with id '{id}' was not found.");
        entity.Submit();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<JournalEntryDto> ApproveAsync(Guid id, JournalEntryApproveDto input, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Journal entry with id '{id}' was not found.");
        entity.Approve(input.ApprovedBy);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<JournalEntryDto> RejectAsync(Guid id, JournalEntryRejectDto input, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Journal entry with id '{id}' was not found.");
        entity.Reject(input.Reason);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<JournalEntryDto> PostAsync(Guid id, JournalEntryPostDto input, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Journal entry with id '{id}' was not found.");
        entity.Post(input.PostedBy);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<JournalEntryDto> ReverseAsync(Guid id, JournalEntryReverseDto input, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Journal entry with id '{id}' was not found.");
        entity.Reverse(input.ReversedBy, input.Reason);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<JournalEntryDto> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Journal entry with id '{id}' was not found.");
        entity.Cancel();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }
}