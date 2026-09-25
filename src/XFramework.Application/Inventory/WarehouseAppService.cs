using XFramework.Application.Abstractions;
using XFramework.Application.Attributes;
using XFramework.Application.Contracts.Dtos;
using XFramework.Application.Contracts.Inventory;
using XFramework.Application.Services;
using XFramework.Domain.Inventory;
using XFramework.Domain.SharedKernel;

namespace XFramework.Application.Inventory;

[Validate]
public sealed class WarehouseAppService(IRepository<Warehouse, Guid> repository, IUnitOfWork unitOfWork)
    : CrudAppService<Warehouse, WarehouseDto, Guid, WarehouseCreateDto, WarehouseUpdateDto>(repository, unitOfWork), IWarehouseAppService
{
    protected override WarehouseDto MapToDto(Warehouse e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name,
        Description = e.Description,
        Type = e.Type,
        Address = new AddressDto
        {
            Street = e.Address.Street,
            City = e.Address.City,
            State = e.Address.State,
            PostalCode = e.Address.PostalCode,
            Country = e.Address.Country
        },
        IsActive = e.IsActive,
        AllowNegativeStock = e.AllowNegativeStock,
        ManagerId = e.ManagerId
    };

    protected override Task<Warehouse> MapToEntityAsync(WarehouseCreateDto i, CancellationToken ct)
    {
        var warehouse = Warehouse.Create(
            i.Code, i.Name, i.Type, 
            new Address(i.Address.Street, i.Address.City, i.Address.State, i.Address.PostalCode, i.Address.Country),
            i.AllowNegativeStock, i.ManagerId, i.Description);
        return Task.FromResult(warehouse);
    }

    protected override Task MapToEntityAsync(WarehouseUpdateDto i, Warehouse e, CancellationToken ct)
    {
        e.UpdateDetails(
            i.Name,
            i.Description,
            i.Type,
            i.Address != null ? new Address(i.Address.Street, i.Address.City, i.Address.State, i.Address.PostalCode, i.Address.Country) : null,
            i.AllowNegativeStock,
            i.ManagerId);
        return Task.CompletedTask;
    }

    public async Task<WarehouseDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Code == code.ToUpperInvariant());
        var entity = await Repository.FirstOrDefaultAsync(query, cancellationToken);
        return entity is null ? default : MapToDto(entity);
    }

    public async Task<List<WarehouseDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.IsActive);
        var entities = await Repository.ToListAsync(query, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<WarehouseDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Warehouse with id '{id}' was not found.");
        entity.Activate();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<WarehouseDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Warehouse with id '{id}' was not found.");
        entity.Deactivate();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }
}