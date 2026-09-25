using XFramework.Application.Abstractions;
using XFramework.Application.Attributes;
using XFramework.Application.Contracts.Abstractions;
using XFramework.Application.Contracts.Inventory;
using XFramework.Application.Contracts.Dtos;
using XFramework.Application.Services;
using XFramework.Domain.Inventory;
using XFramework.Domain.SharedKernel;

namespace XFramework.Application.Inventory;

[Validate]
public sealed class KardexAppService(
    IRepository<KardexEntry, Guid> kardexRepository,
    IRepository<Item, Guid> itemRepository,
    IRepository<Warehouse, Guid> warehouseRepository,
    IUnitOfWork unitOfWork)
    : IReadOnlyAppService<KardexEntryDto, Guid>, IKardexAppService
{
    public async Task<KardexEntryDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await kardexRepository.GetAsync(id, cancellationToken);
        return entity is null ? default : MapToDto(entity);
    }

    public async Task<PagedResult<KardexEntryDto>> GetListAsync(PagedAndSortedRequestDto input, CancellationToken cancellationToken = default)
    {
        input ??= new PagedAndSortedRequestDto();
        var query = kardexRepository.GetQueryable();
        query = ApplySorting(query, input.Sorting);
        var total = await kardexRepository.CountAsync(query, cancellationToken);
        var entities = await kardexRepository.ToListAsync(
            query.Skip(Math.Max(0, input.SkipCount)).Take(Math.Max(1, input.MaxResultCount)), 
            cancellationToken);
        return new PagedResult<KardexEntryDto>(entities.Select(MapToDto).ToList(), total);
    }

    public async Task<PagedResult<KardexEntryDto>> GetByItemAsync(Guid itemId, PagedAndSortedRequestDto input, CancellationToken cancellationToken = default)
    {
        input ??= new PagedAndSortedRequestDto();
        var query = kardexRepository.GetQueryable().Where(x => x.ItemId == itemId);
        query = ApplySorting(query, input.Sorting);
        var total = await kardexRepository.CountAsync(query, cancellationToken);
        var entities = await kardexRepository.ToListAsync(
            query.Skip(Math.Max(0, input.SkipCount)).Take(Math.Max(1, input.MaxResultCount)), 
            cancellationToken);
        return new PagedResult<KardexEntryDto>(entities.Select(MapToDto).ToList(), total);
    }

    public async Task<PagedResult<KardexEntryDto>> GetByWarehouseAsync(Guid warehouseId, PagedAndSortedRequestDto input, CancellationToken cancellationToken = default)
    {
        input ??= new PagedAndSortedRequestDto();
        var query = kardexRepository.GetQueryable().Where(x => x.WarehouseId == warehouseId);
        query = ApplySorting(query, input.Sorting);
        var total = await kardexRepository.CountAsync(query, cancellationToken);
        var entities = await kardexRepository.ToListAsync(
            query.Skip(Math.Max(0, input.SkipCount)).Take(Math.Max(1, input.MaxResultCount)), 
            cancellationToken);
        return new PagedResult<KardexEntryDto>(entities.Select(MapToDto).ToList(), total);
    }

    public async Task<PagedResult<KardexEntryDto>> GetByDateRangeAsync(DateTime from, DateTime to, PagedAndSortedRequestDto input, CancellationToken cancellationToken = default)
    {
        input ??= new PagedAndSortedRequestDto();
        var query = kardexRepository.GetQueryable().Where(x => x.TransactionDate >= from && x.TransactionDate <= to);
        query = ApplySorting(query, input.Sorting);
        var total = await kardexRepository.CountAsync(query, cancellationToken);
        var entities = await kardexRepository.ToListAsync(
            query.Skip(Math.Max(0, input.SkipCount)).Take(Math.Max(1, input.MaxResultCount)), 
            cancellationToken);
        return new PagedResult<KardexEntryDto>(entities.Select(MapToDto).ToList(), total);
    }

    public async Task<KardexEntryDto> CreateReceiptAsync(KardexEntryCreateDto input, CancellationToken cancellationToken = default)
    {
        var item = await itemRepository.GetAsync(input.ItemId, cancellationToken) ?? throw new KeyNotFoundException($"Item with id '{input.ItemId}' was not found.");
        var warehouse = await warehouseRepository.GetAsync(input.WarehouseId, cancellationToken) ?? throw new KeyNotFoundException($"Warehouse with id '{input.WarehouseId}' was not found.");
        
        var quantity = new Quantity(input.Quantity.Value, input.Quantity.Unit);
        var unitCost = new Money(input.UnitCost.Amount, input.UnitCost.Currency);
        
        var entry = KardexEntry.CreateReceipt(
            input.ItemId, input.WarehouseId, input.DocumentReference, input.TransactionDate,
            quantity, unitCost, input.CostingMethod, input.Description, input.ReferenceDocumentId, input.ReferenceDocumentLine);
        
        await kardexRepository.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entry);
    }

    public async Task<KardexEntryDto> CreateIssueAsync(KardexEntryCreateDto input, CancellationToken cancellationToken = default)
    {
        var item = await itemRepository.GetAsync(input.ItemId, cancellationToken) ?? throw new KeyNotFoundException($"Item with id '{input.ItemId}' was not found.");
        var warehouse = await warehouseRepository.GetAsync(input.WarehouseId, cancellationToken) ?? throw new KeyNotFoundException($"Warehouse with id '{input.WarehouseId}' was not found.");
        
        // Get current running balance
        var lastEntry = kardexRepository.GetQueryable()
            .Where(x => x.ItemId == input.ItemId && x.WarehouseId == input.WarehouseId)
            .OrderByDescending(x => x.TransactionDate)
            .FirstOrDefault();
        
        var runningBalance = lastEntry?.RunningBalance ?? Quantity.Zero(input.Quantity.Unit);
        
        var quantity = new Quantity(input.Quantity.Value, input.Quantity.Unit);
        var unitCost = new Money(input.UnitCost.Amount, input.UnitCost.Currency);
        
        var entry = KardexEntry.CreateIssue(
            input.ItemId, input.WarehouseId, input.DocumentReference, input.TransactionDate,
            quantity, unitCost, runningBalance, input.CostingMethod, input.Description, input.ReferenceDocumentId, input.ReferenceDocumentLine);
        
        await kardexRepository.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entry);
    }

    public async Task<KardexEntryDto> CreateAdjustmentAsync(KardexEntryCreateDto input, CancellationToken cancellationToken = default)
    {
        var item = await itemRepository.GetAsync(input.ItemId, cancellationToken) ?? throw new KeyNotFoundException($"Item with id '{input.ItemId}' was not found.");
        var warehouse = await warehouseRepository.GetAsync(input.WarehouseId, cancellationToken) ?? throw new KeyNotFoundException($"Warehouse with id '{input.WarehouseId}' was not found.");
        
        var lastEntry = kardexRepository.GetQueryable()
            .Where(x => x.ItemId == input.ItemId && x.WarehouseId == input.WarehouseId)
            .OrderByDescending(x => x.TransactionDate)
            .FirstOrDefault();
        
        var runningBalance = lastEntry?.RunningBalance ?? Quantity.Zero(input.Quantity.Unit);
        
        var quantityIn = new Quantity(input.Quantity.Value > 0 ? input.Quantity.Value : 0, input.Quantity.Unit);
        var quantityOut = new Quantity(input.Quantity.Value < 0 ? -input.Quantity.Value : 0, input.Quantity.Unit);
        var unitCost = new Money(input.UnitCost.Amount, input.UnitCost.Currency);
        
        var entry = KardexEntry.CreateAdjustment(
            input.ItemId, input.WarehouseId, input.DocumentReference, input.TransactionDate,
            quantityIn, quantityOut, runningBalance, unitCost, input.CostingMethod, input.Description);
        
        await kardexRepository.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entry);
    }

    public async Task<QuantityDto> GetCurrentStockAsync(Guid itemId, Guid warehouseId, CancellationToken cancellationToken = default)
    {
        var lastEntry = kardexRepository.GetQueryable()
            .Where(x => x.ItemId == itemId && x.WarehouseId == warehouseId)
            .OrderByDescending(x => x.TransactionDate)
            .FirstOrDefault();
        
        var balance = lastEntry?.RunningBalance ?? Quantity.Zero(UnitOfMeasure.Piece);
        return new QuantityDto { Value = balance.Value, Unit = balance.Unit };
    }

    private static KardexEntryDto MapToDto(KardexEntry e) => new()
    {
        Id = e.Id,
        ItemId = e.ItemId,
        WarehouseId = e.WarehouseId,
        TransactionType = e.TransactionType,
        DocumentReference = e.DocumentReference,
        TransactionDate = e.TransactionDate,
        QuantityIn = new QuantityDto { Value = e.QuantityIn.Value, Unit = e.QuantityIn.Unit },
        QuantityOut = new QuantityDto { Value = e.QuantityOut.Value, Unit = e.QuantityOut.Unit },
        RunningBalance = new QuantityDto { Value = e.RunningBalance.Value, Unit = e.RunningBalance.Unit },
        UnitCost = new MoneyDto { Amount = e.UnitCost.Amount, Currency = e.UnitCost.Currency },
        TotalCost = new MoneyDto { Amount = e.TotalCost.Amount, Currency = e.TotalCost.Currency },
        CostingMethod = e.CostingMethod,
        Description = e.Description,
        ReferenceDocumentId = e.ReferenceDocumentId,
        ReferenceDocumentLine = e.ReferenceDocumentLine
    };

    private static IQueryable<KardexEntry> ApplySorting(IQueryable<KardexEntry> query, string? sorting)
    {
        return sorting?.ToLowerInvariant() switch
        {
            "date desc" => query.OrderByDescending(x => x.TransactionDate),
            "date" => query.OrderBy(x => x.TransactionDate),
            "type" => query.OrderBy(x => x.TransactionType),
            _ => query.OrderByDescending(x => x.TransactionDate)
        };
    }
}