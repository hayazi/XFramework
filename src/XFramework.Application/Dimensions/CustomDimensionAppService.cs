using XFramework.Application.Abstractions;
using XFramework.Application.Attributes;
using XFramework.Application.Contracts.Dimensions;
using XFramework.Application.Services;
using XFramework.Domain.Dimensions;

namespace XFramework.Application.Dimensions;

[Validate]
public sealed class CustomDimensionAppService(IRepository<CustomDimension, Guid> repository, IUnitOfWork unitOfWork)
    : CrudAppService<CustomDimension, CustomDimensionDto, Guid, CustomDimensionCreateDto, CustomDimensionUpdateDto>(repository, unitOfWork), ICustomDimensionAppService
{
    protected override CustomDimensionDto MapToDto(CustomDimension e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name,
        Description = e.Description,
        DimensionKey = e.DimensionKey,
        Status = e.Status,
        IsRequired = e.IsRequired,
        AllowHierarchy = e.AllowHierarchy,
        ParentDimensionId = e.ParentDimensionId,
        ParentDimensionCode = e.ParentDimension?.Code,
        Attributes = new Dictionary<string, string>(e.Attributes),
        Children = e.Children.Select(MapToDto).ToList()
    };

    protected override Task<CustomDimension> MapToEntityAsync(CustomDimensionCreateDto i, CancellationToken ct)
    {
        var dim = CustomDimension.Create(i.Code, i.Name, i.DimensionKey, i.Description, i.IsRequired, i.AllowHierarchy, i.ParentDimensionId, i.Attributes);
        return Task.FromResult(dim);
    }

    protected override Task MapToEntityAsync(CustomDimensionUpdateDto i, CustomDimension e, CancellationToken ct)
    {
        e.UpdateDetails(i.Name, i.Description, i.Status, i.IsRequired, i.AllowHierarchy, i.ParentDimensionId, i.Attributes);
        return Task.CompletedTask;
    }

    public async Task<CustomDimensionDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Code == code.ToUpperInvariant());
        var entity = await Repository.FirstOrDefaultAsync(query, cancellationToken);
        return entity is null ? default : MapToDto(entity);
    }

    public async Task<CustomDimensionDto?> GetByDimensionKeyAsync(string dimensionKey, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.DimensionKey == dimensionKey.ToLowerInvariant());
        var entity = await Repository.FirstOrDefaultAsync(query, cancellationToken);
        return entity is null ? default : MapToDto(entity);
    }

    public async Task<CustomDimensionDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"CustomDimension with id '{id}' was not found.");
        entity.Activate();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<CustomDimensionDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"CustomDimension with id '{id}' was not found.");
        entity.Deactivate();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }
}