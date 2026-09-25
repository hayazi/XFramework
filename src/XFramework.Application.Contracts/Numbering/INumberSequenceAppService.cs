using XFramework.Application.Contracts.Abstractions;
using XFramework.Domain.Numbering;

namespace XFramework.Application.Contracts.Numbering;

public interface INumberSequenceAppService : ICrudAppService<NumberSequenceDto, Guid, NumberSequenceCreateDto, NumberSequenceUpdateDto>
{
    Task<NumberSequenceDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<NumberSequenceDto?> GetByScopeAsync(string code, string? scopeIdentifier, CancellationToken cancellationToken = default);
    Task<string> GetNextNumberAsync(Guid id, CancellationToken cancellationToken = default);
    Task<string> PeekNextNumberAsync(Guid id, CancellationToken cancellationToken = default);
    Task<NumberSequenceDto> ResetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<NumberSequenceDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<NumberSequenceDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}