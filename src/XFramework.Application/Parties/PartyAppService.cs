using XFramework.Application.Abstractions;
using XFramework.Application.Attributes;
using XFramework.Application.Contracts.Dtos;
using XFramework.Application.Contracts.Parties;
using XFramework.Application.Services;
using XFramework.Domain.Parties;
using XFramework.Domain.SharedKernel;
using AddressDom = XFramework.Domain.SharedKernel.Address;
using AddressDto = XFramework.Application.Contracts.Dtos.AddressDto;
using ContactInfoDom = XFramework.Domain.SharedKernel.ContactInfo;

namespace XFramework.Application.Parties;

[Validate]
public sealed class PartyAppService(IRepository<Party, Guid> repository, IUnitOfWork unitOfWork)
    : CrudAppService<Party, PartyDto, Guid, PartyCreateDto, PartyUpdateDto>(repository, unitOfWork), IPartyAppService
{
    protected override PartyDto MapToDto(Party e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name,
        TaxId = e.TaxId,
        NationalId = e.NationalId,
        IsActive = e.IsActive,
        Contact = new ContactInfoDto
        {
            Name = e.Contact.Name,
            Phone = e.Contact.Phone,
            Email = e.Contact.Email,
            Address = e.Contact.Address != null ? new AddressDto
            {
                Street = e.Contact.Address.Value.Street,
                City = e.Contact.Address.Value.City,
                State = e.Contact.Address.Value.State,
                PostalCode = e.Contact.Address.Value.PostalCode,
                Country = e.Contact.Address.Value.Country
            } : null
        },
        Roles = e.Roles.Select(r => new PartyRoleAssignmentDto
        {
            Id = r.Id,
            PartyId = r.PartyId,
            Role = r.Role,
            ValidFrom = r.ValidFrom,
            ValidTo = r.ValidTo,
            IsActive = r.IsActive
        }).ToList()
    };

    protected override Task<Party> MapToEntityAsync(PartyCreateDto i, CancellationToken ct)
    {
        var address = i.Contact.Address != null ? new AddressDom(
            i.Contact.Address.Street,
            i.Contact.Address.City,
            i.Contact.Address.State,
            i.Contact.Address.PostalCode,
            i.Contact.Address.Country) : (AddressDom?)null;

        var party = Party.Create(
            i.Code,
            i.Name,
            new ContactInfoDom(i.Contact.Name, i.Contact.Phone ?? string.Empty, i.Contact.Email ?? string.Empty, address),
            i.TaxId,
            i.NationalId);

        return Task.FromResult(party);
    }

    protected override Task MapToEntityAsync(PartyUpdateDto i, Party e, CancellationToken ct)
    {
        var address = i.Contact.Address != null ? new AddressDom(
            i.Contact.Address.Street,
            i.Contact.Address.City,
            i.Contact.Address.State,
            i.Contact.Address.PostalCode,
            i.Contact.Address.Country) : (AddressDom?)null;

        e.UpdateDetails(
            i.Name,
            new ContactInfoDom(i.Contact.Name, i.Contact.Phone ?? string.Empty, i.Contact.Email ?? string.Empty, address),
            i.TaxId,
            i.NationalId);
        return Task.CompletedTask;
    }

    public async Task<PartyDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Code == code.ToUpperInvariant());
        var entity = await Repository.FirstOrDefaultAsync(query, cancellationToken);
        return entity is null ? default : MapToDto(entity);
    }

    public async Task<List<PartyDto>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Name.Contains(name));
        var entities = await Repository.ToListAsync(query, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<List<PartyDto>> GetByRoleAsync(PartyRole role, CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.Roles.Any(r => r.Role == role && r.IsActive));
        var entities = await Repository.ToListAsync(query, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<List<PartyDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var query = Repository.GetQueryable().Where(x => x.IsActive);
        var entities = await Repository.ToListAsync(query, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<PartyDto> AssignRoleAsync(Guid partyId, PartyRole role, DateTime? validFrom = null, DateTime? validTo = null, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(partyId, cancellationToken) ?? throw new KeyNotFoundException($"Party with id '{partyId}' was not found.");
        entity.AssignRole(role, validFrom, validTo);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<PartyDto> RemoveRoleAsync(Guid partyId, PartyRole role, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(partyId, cancellationToken) ?? throw new KeyNotFoundException($"Party with id '{partyId}' was not found.");
        entity.RemoveRole(role);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<PartyDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Party with id '{id}' was not found.");
        entity.Activate();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<PartyDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Party with id '{id}' was not found.");
        entity.Deactivate();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }
}