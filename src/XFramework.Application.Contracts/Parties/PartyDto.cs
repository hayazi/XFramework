using XFramework.Application.Contracts.Dtos;
using XFramework.Domain.Parties;
using XFramework.Domain.SharedKernel;

namespace XFramework.Application.Contracts.Parties;

public class PartyDto : EntityDto<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? NationalId { get; set; }
    public bool IsActive { get; set; }
    public ContactInfoDto Contact { get; set; } = new();
    public List<PartyRoleAssignmentDto> Roles { get; set; } = new();
}

public class ContactInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public AddressDto? Address { get; set; }
    public bool HasPhone => !string.IsNullOrWhiteSpace(Phone);
    public bool HasEmail => !string.IsNullOrWhiteSpace(Email);
    public bool HasAddress => Address is not null;
}

public class PartyRoleAssignmentDto : EntityDto<Guid>
{
    public Guid PartyId { get; set; }
    public PartyRole Role { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
}

public class PartyCreateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? NationalId { get; set; }
    public ContactInfoDto Contact { get; set; } = new();
}

public class PartyUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? NationalId { get; set; }
    public ContactInfoDto Contact { get; set; } = new();
}