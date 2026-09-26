using XFramework.Application.Contracts.Abstractions;
using XFramework.Application.Contracts.Dtos;
using XFramework.Domain.Parties;

namespace XFramework.Application.Contracts.Parties;

public interface IPartyAppService : ICrudAppService<PartyDto, Guid, PartyCreateDto, PartyUpdateDto>
{
    Task<PartyDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<List<PartyDto>> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<List<PartyDto>> GetByRoleAsync(PartyRole role, CancellationToken cancellationToken = default);
    Task<List<PartyDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<PartyDto> AssignRoleAsync(Guid partyId, PartyRole role, DateTime? validFrom = null, DateTime? validTo = null, CancellationToken cancellationToken = default);
    Task<PartyDto> RemoveRoleAsync(Guid partyId, PartyRole role, CancellationToken cancellationToken = default);
    Task<PartyDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PartyDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}