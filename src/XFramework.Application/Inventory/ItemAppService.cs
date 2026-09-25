using XFramework.Application.Abstractions;
using XFramework.Application.Attributes;
using XFramework.Application.Contracts.Dtos;
using XFramework.Application.Contracts.Inventory;
using XFramework.Application.Services;
using XFramework.Domain.Inventory;
using XFramework.Domain.SharedKernel;

namespace XFramework.Application.Inventory;

[Validate]
public sealed class ItemAppService(IRepository<Item, Guid> repository, IUnitOfWork unitOfWork)
    : CrudAppService<Item, ItemDto, Guid, ItemCreateDto, ItemUpdateDto>(repository, unitOfWork), IItemAppService
{
    protected override ItemDto MapToDto(Item e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name,
        Description = e.Description,
        Type = e.Type,
        Status = e.Status,
        BaseUnit = e.BaseUnit,
        CostingMethod = e.CostingMethod,
        StandardCost = new MoneyDto { Amount = e.StandardCost.Amount, Currency = e.StandardCost.Currency },
        IsStocked = e.IsStocked,
        IsPurchasable = e.IsPurchasable,
        IsSellable = e.IsSellable,
        IsProducible = e.IsProducible,
        MinimumStock = e.MinimumStock,
        MaximumStock = e.MaximumStock,
        ReorderPoint = e.ReorderPoint,
        DefaultWarehouseId = e.DefaultWarehouseId,
        Barcode = e.Barcode
    };

    protected override Task<Item> MapToEntityAsync(ItemCreateDto i, CancellationToken ct)
    {
        var item = Item.Create(
            i.Code, i.Name, i.Type, i.BaseUnit, i.CostingMethod, i.Currency,
            i.IsStocked, i.IsPurchasable, i.IsSellable, i.IsProducible, i.Description);
        
        if (i.StandardCost != null)
        {
            item.UpdateDetails(
                i.Name, i.Description, null, null,
                new Money(i.StandardCost.Amount, i.StandardCost.Currency),
                i.IsStocked, i.IsPurchasable, i.IsSellable, i.IsProducible,
                i.MinimumStock, i.MaximumStock, i.ReorderPoint,
                i.DefaultWarehouseId, i.Barcode);
        }
        
        return Task.FromResult(item);
    }

    protected override Task MapToEntityAsync(ItemUpdateDto i, Item e, CancellationToken ct)
    {
        e.UpdateDetails(
            i.Name,
            i.Description,
            i.Status,
            i.CostingMethod,
            i.StandardCost != null ? new Money(i.StandardCost.Amount, i.StandardCost.Currency) : null,
            i.IsStocked,
            i.IsPurchasable,
            i.IsSellable,
            i.IsProducible,
            i.MinimumStock,
            i.MaximumStock,
            i.ReorderPoint,
            i.DefaultWarehouseId,
            i.Barcode);
        return Task.CompletedTask;
    }

    public async Task<ItemDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Code == code.ToUpperInvariant());
        var entity = await Repository.FirstOrDefaultAsync(query, cancellationToken);
        return entity is null ? default : MapToDto(entity);
    }

    public async Task<List<ItemDto>> GetByTypeAsync(ItemType type, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Type == type);
        var entities = await Repository.ToListAsync(query, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<List<ItemDto>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.IsStocked && x.ReorderPoint.HasValue && x.MinimumStock.HasValue);
        var entities = await Repository.ToListAsync(query, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<ItemDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Item with id '{id}' was not found.");
        entity.Activate();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<ItemDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Item with id '{id}' was not found.");
        entity.Deactivate();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<ItemDto> DiscontinueAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Item with id '{id}' was not found.");
        entity.Discontinue();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<ItemDto> BlockAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Item with id '{id}' was not found.");
        entity.Block();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<ItemDto> UnblockAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Item with id '{id}' was not found.");
        entity.Unblock();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }
}