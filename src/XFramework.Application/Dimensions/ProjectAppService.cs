using XFramework.Application.Abstractions;
using XFramework.Application.Attributes;
using XFramework.Application.Contracts.Dtos;
using XFramework.Application.Contracts.Dimensions;
using XFramework.Application.Services;
using XFramework.Domain.Dimensions;
using XFramework.Domain.SharedKernel;

namespace XFramework.Application.Dimensions;

[Validate]
public sealed class ProjectAppService(IRepository<Project, Guid> repository, IUnitOfWork unitOfWork)
    : CrudAppService<Project, ProjectDto, Guid, ProjectCreateDto, ProjectUpdateDto>(repository, unitOfWork), IProjectAppService
{
    protected override ProjectDto MapToDto(Project e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name,
        Description = e.Description,
        Status = e.Status,
        StartDate = e.StartDate,
        EndDate = e.EndDate,
        ActualEndDate = e.ActualEndDate,
        Budget = e.Budget != null ? new MoneyDto { Amount = e.Budget.Value.Amount, Currency = e.Budget.Value.Currency } : null,
        ManagerId = e.ManagerId,
        CustomerId = e.CustomerId
    };

    protected override Task<Project> MapToEntityAsync(ProjectCreateDto i, CancellationToken ct)
    {
        var project = Project.Create(i.Code, i.Name, i.StartDate, i.Description, i.EndDate, 
            i.Budget != null ? new Money(i.Budget.Amount, i.Budget.Currency) : null, i.ManagerId, i.CustomerId);
        return Task.FromResult(project);
    }

    protected override Task MapToEntityAsync(ProjectUpdateDto i, Project e, CancellationToken ct)
    {
        e.UpdateDetails(i.Name, i.Description, i.Status, i.StartDate, i.EndDate, i.ActualEndDate,
            i.Budget != null ? new Money(i.Budget.Amount, i.Budget.Currency) : null, i.ManagerId, i.CustomerId);
        return Task.CompletedTask;
    }

    public async Task<ProjectDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Code == code.ToUpperInvariant());
        var entity = await Repository.FirstOrDefaultAsync(query, cancellationToken);
        return entity is null ? default : MapToDto(entity);
    }

    public async Task<List<ProjectDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Status == DimensionStatus.Active);
        var entities = await Repository.ToListAsync(query, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<List<ProjectDto>> GetByManagerAsync(Guid managerId, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.ManagerId == managerId);
        var entities = await Repository.ToListAsync(query, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<List<ProjectDto>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.CustomerId == customerId);
        var entities = await Repository.ToListAsync(query, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<ProjectDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Project with id '{id}' was not found.");
        entity.Activate();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<ProjectDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Project with id '{id}' was not found.");
        entity.Deactivate();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<ProjectDto> CloseAsync(Guid id, DateTime actualEndDate, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Project with id '{id}' was not found.");
        entity.Close(actualEndDate);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }
}