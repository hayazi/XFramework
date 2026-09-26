using XFramework.Application.Contracts.Abstractions;
using XFramework.Application.Contracts.Dtos;
using XFramework.Domain.Accounting;
using XFramework.Domain.SharedKernel;

namespace XFramework.Application.Contracts.Accounting;

public class AccountDto : EntityDto<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AccountType Type { get; set; }
    public AccountNature Nature { get; set; }
    public Currency Currency { get; set; }
    public Guid? ParentAccountId { get; set; }
    public string? ParentAccountCode { get; set; }
    public bool IsActive { get; set; }
    public bool IsDetail { get; set; }
    public List<AccountDto> Children { get; set; } = new();
}

public class AccountCreateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AccountType Type { get; set; }
    public Currency Currency { get; set; } = Currency.IRR;
    public Guid? ParentAccountId { get; set; }
    public bool IsDetail { get; set; } = true;
}

public class AccountUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool? IsDetail { get; set; }
}

public interface IAccountAppService : ICrudAppService<AccountDto, Guid, AccountCreateDto, AccountUpdateDto>
{
    Task<AccountDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<List<AccountDto>> GetHierarchyAsync(CancellationToken cancellationToken = default);
    Task<List<AccountDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<List<AccountDto>> GetChildrenAsync(Guid parentAccountId, CancellationToken cancellationToken = default);
    Task<AccountDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AccountDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AccountDto> SetParentAsync(Guid id, Guid parentAccountId, CancellationToken cancellationToken = default);
    Task<AccountDto> RemoveParentAsync(Guid id, CancellationToken cancellationToken = default);
}