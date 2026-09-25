using XFramework.Domain.Aggregates;
using XFramework.Domain.Entities;
using XFramework.Domain.SharedKernel;

namespace XFramework.Domain.Parties;

public sealed class Party : AggregateRoot<Guid>
{
    private readonly List<PartyRoleAssignment> _roles = new();

    private Party() { }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? TaxId { get; private set; }
    public string? NationalId { get; private set; }
    public bool IsActive { get; private set; }
    public ContactInfo Contact { get; private set; }
    public IReadOnlyCollection<PartyRoleAssignment> Roles => _roles.AsReadOnly();

    public static Party Create(
        string code,
        string name,
        ContactInfo contact,
        string? taxId = null,
        string? nationalId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Party code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Party name is required.", nameof(name));

        var party = new Party
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Contact = contact,
            TaxId = taxId?.Trim(),
            NationalId = nationalId?.Trim(),
            IsActive = true
        };

        party.AddDomainEvent(new PartyCreated(party.Id, party.Code, party.Name));
        return party;
    }

    public void UpdateDetails(
        string name,
        ContactInfo contact,
        string? taxId = null,
        string? nationalId = null)
    {
        Name = name.Trim();
        Contact = contact;
        TaxId = taxId?.Trim();
        NationalId = nationalId?.Trim();

        AddDomainEvent(new PartyUpdated(Id, Name));
    }

    public void AssignRole(PartyRole role, DateTime? validFrom = null, DateTime? validTo = null)
    {
        if (_roles.Any(r => r.Role == role && r.IsActive))
            throw new InvalidOperationException($"Party already has active role: {role}");

        var assignment = new PartyRoleAssignment
        {
            PartyId = Id,
            Role = role,
            ValidFrom = validFrom?.Date ?? DateTime.UtcNow.Date,
        };
        assignment.SetValidTo(validTo);

        _roles.Add(assignment);
        AddDomainEvent(new PartyRoleAssigned(Id, role));
    }

    public void RemoveRole(PartyRole role)
    {
        var assignment = _roles.FirstOrDefault(r => r.Role == role && r.IsActive);
        if (assignment is null)
            throw new InvalidOperationException($"Party does not have active role: {role}");

        assignment.Deactivate();
        AddDomainEvent(new PartyRoleRemoved(Id, role));
    }

    public bool HasRole(PartyRole role) =>
        _roles.Any(r => r.Role == role && r.IsActive);

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        AddDomainEvent(new PartyActivated(Id));
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        AddDomainEvent(new PartyDeactivated(Id));
    }
}

public sealed class PartyRoleAssignment : Entity<Guid>
{
    public Guid PartyId { get; init; }
    public PartyRole Role { get; init; }
    public DateTime ValidFrom { get; init; }
    public DateTime? ValidTo { get; private set; }

    internal PartyRoleAssignment() { }

    public bool IsActive =>
        ValidFrom <= DateTime.UtcNow.Date &&
        (ValidTo == null || ValidTo >= DateTime.UtcNow.Date);

    public void SetValidTo(DateTime? validTo) =>
        ValidTo = validTo?.Date;

    public void Deactivate() =>
        ValidTo = DateTime.UtcNow.Date;
}