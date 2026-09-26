using XFramework.Application.Abstractions;
using XFramework.Application.Attributes;
using XFramework.Application.Contracts.Accounting;
using XFramework.Application.Contracts.Dtos;
using XFramework.Application.Services;
using XFramework.Domain.Accounting;
using XFramework.Domain.SharedKernel;

namespace XFramework.Application.Accounting;

[Validate]
public sealed class AccountAppService(IRepository<Account, Guid> repository, IUnitOfWork unitOfWork)
    : CrudAppService<Account, AccountDto, Guid, AccountCreateDto, AccountUpdateDto>(repository, unitOfWork), IAccountAppService
{
    protected override AccountDto MapToDto(Account e)
    {
        var dto = new AccountDto
        {
            Id = e.Id,
            Code = e.Code,
            Name = e.Name,
            Description = e.Description,
            Type = e.Type,
            Nature = e.Nature,
            Currency = e.Currency,
            ParentAccountId = e.ParentAccountId,
            IsActive = e.IsActive,
            IsDetail = e.IsDetail
        };

        if (e.ParentAccount != null)
        {
            dto.ParentAccountCode = e.ParentAccount.Code;
        }

        dto.Children = e.Children.Select(c => new AccountDto
        {
            Id = c.Id,
            Code = c.Code,
            Name = c.Name,
            Description = c.Description,
            Type = c.Type,
            Nature = c.Nature,
            Currency = c.Currency,
            ParentAccountId = c.ParentAccountId,
            IsActive = c.IsActive,
            IsDetail = c.IsDetail
        }).ToList();

        return dto;
    }

    protected override Task<Account> MapToEntityAsync(AccountCreateDto i, CancellationToken ct)
    {
        var account = Account.Create(
            i.Code,
            i.Name,
            i.Type,
            i.Currency,
            i.IsDetail,
            i.ParentAccountId,
            i.Description);

        return Task.FromResult(account);
    }

    protected override Task MapToEntityAsync(AccountUpdateDto i, Account e, CancellationToken ct)
    {
        e.UpdateDetails(i.Name, i.Description, i.IsDetail);
        return Task.CompletedTask;
    }

    public async Task<AccountDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Code == code.Trim().ToUpperInvariant());
        var entity = await Repository.FirstOrDefaultAsync(query, cancellationToken);
        return entity is null ? default : MapToDto(entity);
    }

    public async Task<List<AccountDto>> GetHierarchyAsync(CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.ParentAccountId == null);
        var entities = await Repository.ToListAsync(query, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<List<AccountDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.IsActive);
        var entities = await Repository.ToListAsync(query, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<List<AccountDto>> GetChildrenAsync(Guid parentAccountId, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.ParentAccountId == parentAccountId);
        var entities = await Repository.ToListAsync(query, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<AccountDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Account with id '{id}' was not found.");
        entity.Activate();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<AccountDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Account with id '{id}' was not found.");
        entity.Deactivate();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<AccountDto> SetParentAsync(Guid id, Guid parentAccountId, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Account with id '{id}' was not found.");
        var parent = await Repository.GetAsync(parentAccountId, cancellationToken) ?? throw new KeyNotFoundException($"Parent account with id '{parentAccountId}' was not found.");
        
        entity.SetParent(parent);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<AccountDto> RemoveParentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Account with id '{id}' was not found.");
        entity.RemoveParent();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }
}