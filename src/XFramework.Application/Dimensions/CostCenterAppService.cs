using XFramework.Application.Abstractions;
using XFramework.Application.Attributes;
using XFramework.Application.Contracts.Dimensions;
using XFramework.Application.Services;
using XFramework.Domain.Dimensions;

namespace XFramework.Application.Dimensions;

[Validate]
public sealed class CostCenterAppService(IRepository<CostCenter, Guid> repository, IUnitOfWork unitOfWork)
    : CrudAppService<CostCenter, CostCenterDto, Guid, CostCenterCreateDto, CostCenterUpdateDto>(repository, unitOfWork), ICostCenterAppService
{
    protected override CostCenterDto MapToDto(CostCenter e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name,
        Description = e.Description,
        Status = e.Status,
        ParentCostCenterId = e.ParentCostCenterId,
        ParentCostCenterCode = e.ParentCostCenter?.Code,
        ManagerId = e.ManagerId,
        BudgetAmount = e.BudgetAmount,
        BudgetCurrency = e.BudgetCurrency,
        Children = e.Children.Select(MapToDto).ToList()
    };

    protected override Task<CostCenter> MapToEntityAsync(CostCenterCreateDto i, CancellationToken ct)
    {
        var cc = CostCenter.Create(i.Code, i.Name, i.Description, i.ParentCostCenterId, i.ManagerId, i.BudgetAmount, i.BudgetCurrency);
        return Task.FromResult(cc);
    }

    protected override Task MapToEntityAsync(CostCenterUpdateDto i, CostCenter e, CancellationToken ct)
    {
        e.UpdateDetails(i.Name, i.Description, i.Status, i.ParentCostCenterId, i.ManagerId, i.BudgetAmount, i.BudgetCurrency);
        return Task.CompletedTask;
    }

    public async Task<CostCenterDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Code == code.ToUpperInvariant());
        var entity = await Repository.FirstOrDefaultAsync(query, cancellationToken);
        return entity is null ? default : MapToDto(entity);
    }

    public async Task<List<CostCenterDto>> GetHierarchyAsync(CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.ParentCostCenterId == null);
        var roots = await Repository.ToListAsync(query, cancellationToken);
        return roots.Select(MapToDto).ToList();
    }

    public async Task<List<CostCenterDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Status == DimensionStatus.Active);
        var entities = await Repository.ToListAsync(query, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<CostCenterDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"CostCenter with id '{id}' was not found.");
        entity.Activate();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<CostCenterDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"CostCenter with id '{id}' was not found.");
        entity.Deactivate();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<CostCenterDto> CloseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"CostCenter with id '{id}' was not found.");
        entity.Close();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }
}