using XFramework.Domain.Parties;
using XFramework.Domain.SharedKernel;
using Xunit;

namespace XFramework.Tests.Domain;

public class PartyTests
{
    [Fact]
    public void Create_ValidData_CreatesParty()
    {
        var contact = new ContactInfo("Test Contact", "+989123456789", "test@example.com");
        var party = Party.Create("P001", "Test Party", contact);

        Assert.NotEqual(Guid.Empty, party.Id);
        Assert.Equal("P001", party.Code);
        Assert.Equal("Test Party", party.Name);
        Assert.Equal(contact, party.Contact);
        Assert.True(party.IsActive);
        Assert.Empty(party.Roles);
    }

    [Fact]
    public void Create_EmptyCode_Throws()
    {
        var contact = new ContactInfo("Test", "+989123456789", "test@example.com");
        Assert.Throws<ArgumentException>(() => Party.Create("", "Test Party", contact));
    }

    [Fact]
    public void Create_EmptyName_Throws()
    {
        var contact = new ContactInfo("Test", "+989123456789", "test@example.com");
        Assert.Throws<ArgumentException>(() => Party.Create("P001", "", contact));
    }

    [Fact]
    public void UpdateDetails_ValidData_UpdatesParty()
    {
        var contact = new ContactInfo("Test Contact", "+989123456789", "test@example.com");
        var party = Party.Create("P001", "Test Party", contact);
        var newContact = new ContactInfo("New Contact", "+989987654321", "new@example.com");

        party.UpdateDetails("Updated Party", newContact, "TAX123", "NAT123");

        Assert.Equal("Updated Party", party.Name);
        Assert.Equal(newContact, party.Contact);
        Assert.Equal("TAX123", party.TaxId);
        Assert.Equal("NAT123", party.NationalId);
    }

    [Fact]
    public void AssignRole_NewRole_AddsRole()
    {
        var contact = new ContactInfo("Test Contact", "+989123456789", "test@example.com");
        var party = Party.Create("P001", "Test Party", contact);

        party.AssignRole(PartyRole.Customer);

        Assert.Single(party.Roles);
        Assert.Contains(party.Roles, r => r.Role == PartyRole.Customer && r.IsActive);
    }

    [Fact]
    public void AssignRole_DuplicateActiveRole_Throws()
    {
        var contact = new ContactInfo("Test Contact", "+989123456789", "test@example.com");
        var party = Party.Create("P001", "Test Party", contact);
        party.AssignRole(PartyRole.Customer);

        Assert.Throws<InvalidOperationException>(() => party.AssignRole(PartyRole.Customer));
    }

    [Fact]
    public void AssignRole_WithValidFromAndValidTo_SetsDates()
    {
        var contact = new ContactInfo("Test Contact", "+989123456789", "test@example.com");
        var party = Party.Create("P001", "Test Party", contact);
        var validFrom = DateTime.UtcNow.AddDays(1);
        var validTo = DateTime.UtcNow.AddDays(30);

        party.AssignRole(PartyRole.Supplier, validFrom, validTo);

        var role = party.Roles.First(r => r.Role == PartyRole.Supplier);
        Assert.Equal(validFrom.Date, role.ValidFrom);
        Assert.Equal(validTo.Date, role.ValidTo!.Value);
    }

    [Fact]
    public void RemoveRole_ActiveRole_RemovesRole()
    {
        var contact = new ContactInfo("Test Contact", "+989123456789", "test@example.com");
        var party = Party.Create("P001", "Test Party", contact);
        party.AssignRole(PartyRole.Customer);

        party.RemoveRole(PartyRole.Customer);

        Assert.Single(party.Roles);
        var role = party.Roles.First(r => r.Role == PartyRole.Customer);
        Assert.NotNull(role.ValidTo); // ValidTo should be set
        // Role is still active for today, becomes inactive tomorrow
    }

    [Fact]
    public void RemoveRole_InactiveRole_Throws()
    {
        var contact = new ContactInfo("Test Contact", "+989123456789", "test@example.com");
        var party = Party.Create("P001", "Test Party", contact);

        Assert.Throws<InvalidOperationException>(() => party.RemoveRole(PartyRole.Customer));
    }

    [Fact]
    public void HasRole_ActiveRole_ReturnsTrue()
    {
        var contact = new ContactInfo("Test Contact", "+989123456789", "test@example.com");
        var party = Party.Create("P001", "Test Party", contact);
        party.AssignRole(PartyRole.Customer);

        Assert.True(party.HasRole(PartyRole.Customer));
    }

    [Fact]
    public void HasRole_InactiveRole_ReturnsFalse()
    {
        var contact = new ContactInfo("Test Contact", "+989123456789", "test@example.com");
        var party = Party.Create("P001", "Test Party", contact);

        Assert.False(party.HasRole(PartyRole.Customer));
    }

    [Fact]
    public void Activate_DeactivatedParty_ActivatesParty()
    {
        var contact = new ContactInfo("Test Contact", "+989123456789", "test@example.com");
        var party = Party.Create("P001", "Test Party", contact);
        party.Deactivate();

        party.Activate();

        Assert.True(party.IsActive);
    }

    [Fact]
    public void Deactivate_ActiveParty_DeactivatesParty()
    {
        var contact = new ContactInfo("Test Contact", "+989123456789", "test@example.com");
        var party = Party.Create("P001", "Test Party", contact);

        party.Deactivate();

        Assert.False(party.IsActive);
    }

    [Fact]
    public void Deactivate_AlreadyDeactivatedParty_DoesNothing()
    {
        var contact = new ContactInfo("Test Contact", "+989123456789", "test@example.com");
        var party = Party.Create("P001", "Test Party", contact);
        party.Deactivate();

        party.Deactivate();

        Assert.False(party.IsActive);
    }
}