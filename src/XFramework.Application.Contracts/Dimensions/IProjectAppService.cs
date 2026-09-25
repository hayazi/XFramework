using XFramework.Application.Contracts.Abstractions;
using XFramework.Domain.Dimensions;

namespace XFramework.Application.Contracts.Dimensions;

public interface IProjectAppService : ICrudAppService<ProjectDto, Guid, ProjectCreateDto, ProjectUpdateDto>
{
    Task<ProjectDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<List<ProjectDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<List<ProjectDto>> GetByManagerAsync(Guid managerId, CancellationToken cancellationToken = default);
    Task<List<ProjectDto>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<ProjectDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProjectDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProjectDto> CloseAsync(Guid id, DateTime actualEndDate, CancellationToken cancellationToken = default);
}