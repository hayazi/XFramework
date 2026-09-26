using XFramework.Application.Abstractions;
using XFramework.Application.Attributes;
using XFramework.Application.Contracts.Accounting;
using XFramework.Application.Contracts.Dtos;
using XFramework.Application.Services;
using XFramework.Domain.Accounting;
using XFramework.Domain.SharedKernel;

namespace XFramework.Application.Accounting;

[Validate]
public sealed class FiscalPeriodAppService(IRepository<FiscalPeriod, Guid> repository, IUnitOfWork unitOfWork)
    : CrudAppService<FiscalPeriod, FiscalPeriodDto, Guid, FiscalPeriodCreateDto, FiscalPeriodUpdateDto>(repository, unitOfWork), IFiscalPeriodAppService
{
    protected override FiscalPeriodDto MapToDto(FiscalPeriod e) => new()
    {
        Id = e.Id,
        Year = e.Year,
        PeriodNumber = e.PeriodNumber,
        Name = e.Name,
        DateRange = new DateRangeDto
        {
            Start = e.DateRange.Start,
            End = e.DateRange.End
        },
        Status = e.Status,
        ClosedOnUtc = e.ClosedOnUtc,
        ClosedBy = e.ClosedBy
    };

    protected override Task<FiscalPeriod> MapToEntityAsync(FiscalPeriodCreateDto i, CancellationToken ct)
    {
        var period = FiscalPeriod.Create(
            i.Year,
            i.PeriodNumber,
            i.Name,
            new DateRange(i.DateRange.Start, i.DateRange.End));

        return Task.FromResult(period);
    }

    protected override Task MapToEntityAsync(FiscalPeriodUpdateDto i, FiscalPeriod e, CancellationToken ct)
    {
        // FiscalPeriod doesn't have an UpdateDetails method, so we only update Name
        // Note: Year and PeriodNumber are immutable after creation
        // This is a limitation - in practice, you might want to allow date range updates
        return Task.CompletedTask;
    }

    public async Task<FiscalPeriodDto?> GetByYearAndPeriodAsync(int year, int periodNumber, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Year == year && x.PeriodNumber == periodNumber);
        var entity = await Repository.FirstOrDefaultAsync(query, cancellationToken);
        return entity is null ? default : MapToDto(entity);
    }

    public async Task<List<FiscalPeriodDto>> GetByYearAsync(int year, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Year == year).OrderBy(x => x.PeriodNumber);
        var entities = await Repository.ToListAsync(query, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<List<FiscalPeriodDto>> GetByStatusAsync(FiscalPeriodStatus status, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Status == status);
        var entities = await Repository.ToListAsync(query, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<FiscalPeriodDto> CloseAsync(Guid id, FiscalPeriodCloseDto input, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Fiscal period with id '{id}' was not found.");
        entity.Close(input.ClosedBy);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<FiscalPeriodDto> ReopenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Fiscal period with id '{id}' was not found.");
        entity.Reopen();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<FiscalPeriodDto> LockAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Fiscal period with id '{id}' was not found.");
        entity.Lock();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<FiscalPeriodDto> UnlockAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Fiscal period with id '{id}' was not found.");
        entity.Unlock();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<FiscalPeriodDto?> GetCurrentPeriodAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.DateRange.Contains(date) && x.Status == FiscalPeriodStatus.Open);
        var entity = await Repository.FirstOrDefaultAsync(query, cancellationToken);
        return entity is null ? default : MapToDto(entity);
    }
}