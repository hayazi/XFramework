using XFramework.Application.Abstractions;
using XFramework.Application.Attributes;
using XFramework.Application.Contracts.Numbering;
using XFramework.Application.Services;
using XFramework.Domain.Numbering;

namespace XFramework.Application.Numbering;

[Validate]
public sealed class NumberSequenceAppService(IRepository<NumberSequence, Guid> repository, IUnitOfWork unitOfWork)
    : CrudAppService<NumberSequence, NumberSequenceDto, Guid, NumberSequenceCreateDto, NumberSequenceUpdateDto>(repository, unitOfWork), INumberSequenceAppService
{
    protected override NumberSequenceDto MapToDto(NumberSequence e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name,
        Prefix = e.Prefix,
        Suffix = e.Suffix,
        CurrentNumber = e.CurrentNumber,
        MinimumDigits = e.MinimumDigits,
        IncrementBy = e.IncrementBy,
        MaximumNumber = e.MaximumNumber,
        Scope = e.Scope,
        ScopeIdentifier = e.ScopeIdentifier,
        Status = e.Status,
        ResetDate = e.ResetDate,
        AutoReset = e.AutoReset,
        FormatTemplate = e.FormatTemplate
    };

    protected override Task<NumberSequence> MapToEntityAsync(NumberSequenceCreateDto i, CancellationToken ct)
    {
        var seq = NumberSequence.Create(i.Code, i.Name, i.Prefix, i.MinimumDigits, i.IncrementBy, i.MaximumNumber, i.Scope, i.ScopeIdentifier, i.Suffix, i.AutoReset, i.ResetDate, i.FormatTemplate);
        return Task.FromResult(seq);
    }

    protected override Task MapToEntityAsync(NumberSequenceUpdateDto i, NumberSequence e, CancellationToken ct)
    {
        e.UpdateDetails(i.Name, i.Prefix, i.Suffix, i.MinimumDigits, i.IncrementBy, i.MaximumNumber, i.Status, i.AutoReset, i.ResetDate, i.FormatTemplate);
        return Task.CompletedTask;
    }

    public async Task<NumberSequenceDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Code == code.ToUpperInvariant());
        var entity = await Repository.FirstOrDefaultAsync(query, cancellationToken);
        return entity is null ? default : MapToDto(entity);
    }

    public async Task<NumberSequenceDto?> GetByScopeAsync(string code, string? scopeIdentifier, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Code == code.ToUpperInvariant());
        
        if (scopeIdentifier != null)
        {
            query = query.Where(x => x.ScopeIdentifier == scopeIdentifier);
        }
        else
        {
            query = query.Where(x => x.ScopeIdentifier == null || x.ScopeIdentifier == "");
        }
        
        var entity = await Repository.FirstOrDefaultAsync(query, cancellationToken);
        return entity is null ? default : MapToDto(entity);
    }

    public async Task<string> GetNextNumberAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"NumberSequence with id '{id}' was not found.");
        var number = entity.GetNextNumber();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return number;
    }

    public async Task<string> PeekNextNumberAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"NumberSequence with id '{id}' was not found.");
        return entity.PeekNextNumber();
    }

    public async Task<NumberSequenceDto> ResetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"NumberSequence with id '{id}' was not found.");
        entity.Reset();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<NumberSequenceDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"NumberSequence with id '{id}' was not found.");
        entity.Activate();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<NumberSequenceDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"NumberSequence with id '{id}' was not found.");
        entity.Deactivate();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }
}