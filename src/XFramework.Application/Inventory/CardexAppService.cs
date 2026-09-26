using XFramework.Application.Abstractions;
using XFramework.Application.Attributes;
using XFramework.Application.Contracts.Abstractions;
using XFramework.Application.Contracts.Dtos;
using XFramework.Application.Contracts.Inventory;
using XFramework.Domain.Inventory;
using XFramework.Domain.SharedKernel;

namespace XFramework.Application.Inventory;

[Validate]
public sealed class CardexAppService(
    IRepository<CardexEntry, Guid> cardexRepository,
    IRepository<Item, Guid> itemRepository,
    IUnitOfWork unitOfWork)
    : IReadOnlyAppService<CardexEntryDto, Guid>, ICardexAppService
{
    private readonly IRepository<CardexEntry, Guid> _cardexRepository = cardexRepository;
    private readonly IRepository<Item, Guid> _itemRepository = itemRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<CardexEntryDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _cardexRepository.GetAsync(id, cancellationToken);
        return entity is null ? default : MapToDto(entity);
    }

    public async Task<PagedResult<CardexEntryDto>> GetListAsync(PagedAndSortedRequestDto input, CancellationToken cancellationToken = default)
    {
        input ??= new();
        var query = _cardexRepository.GetQueryable();
        query = ApplySorting(query, input.Sorting);
        var total = await _cardexRepository.CountAsync(query, cancellationToken);
        var entities = await _cardexRepository.ToListAsync(query.Skip(Math.Max(0, input.SkipCount)).Take(Math.Max(1, input.MaxResultCount)), cancellationToken);
        return new PagedResult<CardexEntryDto>(entities.Select(MapToDto).ToList(), total);
    }

    public async Task<PagedResult<CardexEntryDto>> GetByItemAsync(Guid itemId, PagedAndSortedRequestDto input, CancellationToken cancellationToken = default)
    {
        input ??= new();
        var query = _cardexRepository.GetQueryable().Where(x => x.ItemId == itemId);
        query = ApplySorting(query, input.Sorting);
        var total = await _cardexRepository.CountAsync(query, cancellationToken);
        var entities = await _cardexRepository.ToListAsync(query.Skip(Math.Max(0, input.SkipCount)).Take(Math.Max(1, input.MaxResultCount)), cancellationToken);
        return new PagedResult<CardexEntryDto>(entities.Select(MapToDto).ToList(), total);
    }

    public async Task<PagedResult<CardexEntryDto>> GetByWarehouseAsync(Guid warehouseId, PagedAndSortedRequestDto input, CancellationToken cancellationToken = default)
    {
        input ??= new();
        var query = _cardexRepository.GetQueryable().Where(x => x.WarehouseId == warehouseId);
        query = ApplySorting(query, input.Sorting);
        var total = await _cardexRepository.CountAsync(query, cancellationToken);
        var entities = await _cardexRepository.ToListAsync(query.Skip(Math.Max(0, input.SkipCount)).Take(Math.Max(1, input.MaxResultCount)), cancellationToken);
        return new PagedResult<CardexEntryDto>(entities.Select(MapToDto).ToList(), total);
    }

    public async Task<PagedResult<CardexEntryDto>> GetByDateRangeAsync(DateTime from, DateTime to, PagedAndSortedRequestDto input, CancellationToken cancellationToken = default)
    {
        input ??= new();
        var query = _cardexRepository.GetQueryable().Where(x => x.TransactionDate >= from && x.TransactionDate <= to);
        query = ApplySorting(query, input.Sorting);
        var total = await _cardexRepository.CountAsync(query, cancellationToken);
        var entities = await _cardexRepository.ToListAsync(query.Skip(Math.Max(0, input.SkipCount)).Take(Math.Max(1, input.MaxResultCount)), cancellationToken);
        return new PagedResult<CardexEntryDto>(entities.Select(MapToDto).ToList(), total);
    }

    public async Task<CardexEntryDto> CreateReceiptAsync(CardexEntryCreateDto input, CancellationToken cancellationToken = default)
    {
        var item = await _itemRepository.GetAsync(input.ItemId, cancellationToken) ?? throw new KeyNotFoundException($"Item with id '{input.ItemId}' not found.");

        var entry = CardexEntry.CreateReceipt(
            input.ItemId,
            input.WarehouseId,
            input.DocumentReference,
            input.TransactionDate,
            new Quantity(input.Quantity.Value, input.Quantity.Unit),
            new Money(input.UnitCost.Amount, input.UnitCost.Currency),
            input.CostingMethod,
            input.Description,
            input.ReferenceDocumentId,
            input.ReferenceDocumentLine);

        await _cardexRepository.AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entry);
    }

    public async Task<CardexEntryDto> CreateIssueAsync(CardexEntryCreateDto input, CancellationToken cancellationToken = default)
    {
        var item = await _itemRepository.GetAsync(input.ItemId, cancellationToken) ?? throw new KeyNotFoundException($"Item with id '{input.ItemId}' not found.");

        var query = _cardexRepository.GetQueryable().Where(x => x.ItemId == input.ItemId && x.WarehouseId == input.WarehouseId);
        var entries = await _cardexRepository.ToListAsync(query, cancellationToken);
        var runningBalance = entries.Any() ? entries.MaxBy(x => x.TransactionDate)!.RunningBalance : Quantity.Zero(item.BaseUnit);

        var entry = CardexEntry.CreateIssue(
            input.ItemId,
            input.WarehouseId,
            input.DocumentReference,
            input.TransactionDate,
            new Quantity(input.Quantity.Value, input.Quantity.Unit),
            new Money(input.UnitCost.Amount, input.UnitCost.Currency),
            runningBalance,
            input.CostingMethod,
            input.Description,
            input.ReferenceDocumentId,
            input.ReferenceDocumentLine);

        await _cardexRepository.AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entry);
    }

    public async Task<CardexEntryDto> CreateAdjustmentAsync(CardexEntryCreateDto input, CancellationToken cancellationToken = default)
    {
        var item = await _itemRepository.GetAsync(input.ItemId, cancellationToken) ?? throw new KeyNotFoundException($"Item with id '{input.ItemId}' not found.");

        var entry = CardexEntry.CreateAdjustment(
            input.ItemId,
            input.WarehouseId,
            input.DocumentReference,
            input.TransactionDate,
            new Quantity(input.Quantity.Value, input.Quantity.Unit),
            Quantity.Zero(item.BaseUnit),
            new Quantity(input.Quantity.Value, input.Quantity.Unit),
            new Money(input.UnitCost.Amount, input.UnitCost.Currency),
            input.CostingMethod,
            input.Description);

        await _cardexRepository.AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entry);
    }

    public async Task<MoneyDto> GetCurrentStockAsync(Guid itemId, Guid warehouseId, CancellationToken cancellationToken = default)
    {
        var query = _cardexRepository.GetQueryable().Where(x => x.ItemId == itemId && x.WarehouseId == warehouseId);
        var entries = await _cardexRepository.ToListAsync(query, cancellationToken);
        
        if (!entries.Any())
            return new MoneyDto { Amount = 0, Currency = Currency.IRR };

        var latest = entries.MaxBy(x => x.TransactionDate);
        return new MoneyDto { Amount = latest!.RunningBalance.Value, Currency = Currency.IRR };
    }

    private static CardexEntryDto MapToDto(CardexEntry e) => new()
    {
        Id = e.Id,
        ItemId = e.ItemId,
        ItemCode = e.ItemId.ToString(),
        ItemName = e.ItemId.ToString(),
        WarehouseId = e.WarehouseId,
        WarehouseCode = e.WarehouseId.ToString(),
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

    private static IQueryable<CardexEntry> ApplySorting(IQueryable<CardexEntry> query, string? sorting) => sorting?.ToLowerInvariant() switch
    {
        "transactiondate" => query.OrderBy(x => x.TransactionDate),
        "transactiondate desc" => query.OrderByDescending(x => x.TransactionDate),
        _ => query.OrderByDescending(x => x.TransactionDate)
    };
}