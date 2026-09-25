using XFramework.Domain.Accounting;
using XFramework.Domain.SharedKernel;
using Xunit;

namespace XFramework.Tests.Domain;

public class AccountTests
{
    [Fact]
    public void Create_ValidData_CreatesAccount()
    {
        var account = Account.Create("1000", "Cash", AccountType.Asset, Currency.IRR);

        Assert.NotEqual(Guid.Empty, account.Id);
        Assert.Equal("1000", account.Code);
        Assert.Equal("Cash", account.Name);
        Assert.Equal(AccountType.Asset, account.Type);
        Assert.Equal(AccountNature.Debit, account.Nature);
        Assert.Equal(Currency.IRR, account.Currency);
        Assert.True(account.IsActive);
        Assert.True(account.IsDetail);
    }

    [Fact]
    public void Create_EmptyCode_Throws()
    {
        Assert.Throws<ArgumentException>(() => Account.Create("", "Cash", AccountType.Asset, Currency.IRR));
    }

    [Fact]
    public void Create_EmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() => Account.Create("1000", "", AccountType.Asset, Currency.IRR));
    }

    [Fact]
    public void Create_LiabilityType_SetsCreditNature()
    {
        var account = Account.Create("2000", "Accounts Payable", AccountType.Liability, Currency.IRR);

        Assert.Equal(AccountNature.Credit, account.Nature);
    }

    [Fact]
    public void Create_RevenueType_SetsCreditNature()
    {
        var account = Account.Create("4000", "Sales Revenue", AccountType.Revenue, Currency.IRR);

        Assert.Equal(AccountNature.Credit, account.Nature);
    }

    [Fact]
    public void Create_ExpenseType_SetsDebitNature()
    {
        var account = Account.Create("5000", "Rent Expense", AccountType.Expense, Currency.IRR);

        Assert.Equal(AccountNature.Debit, account.Nature);
    }

    [Fact]
    public void Create_EquityType_SetsCreditNature()
    {
        var account = Account.Create("3000", "Owner's Equity", AccountType.Equity, Currency.IRR);

        Assert.Equal(AccountNature.Credit, account.Nature);
    }

    [Fact]
    public void Create_NonDetailAccount_SetsIsDetailFalse()
    {
        var account = Account.Create("1000", "Assets", AccountType.Asset, Currency.IRR, isDetail: false);

        Assert.False(account.IsDetail);
    }

    [Fact]
    public void Create_WithParent_SetsParentAccountId()
    {
        var parent = Account.Create("1000", "Assets", AccountType.Asset, Currency.IRR, isDetail: false);
        var child = Account.Create("1100", "Current Assets", AccountType.Asset, Currency.IRR, parentAccountId: parent.Id);

        Assert.Equal(parent.Id, child.ParentAccountId);
    }

    [Fact]
    public void GetBalance_DebitNature_ReturnsCorrectBalance()
    {
        var account = Account.Create("1000", "Cash", AccountType.Asset, Currency.IRR);
        var line1 = new JournalLine(account.Id, AccountNature.Debit, new Money(1000m, Currency.IRR), "Receipt");
        var line2 = new JournalLine(account.Id, AccountNature.Credit, new Money(300m, Currency.IRR), "Payment");

        var balance = account.GetBalance(new[] { line1, line2 });

        Assert.Equal(new Money(700m, Currency.IRR), balance);
    }

    [Fact]
    public void GetBalance_CreditNature_ReturnsCorrectBalance()
    {
        var account = Account.Create("2000", "Accounts Payable", AccountType.Liability, Currency.IRR);
        var line1 = new JournalLine(account.Id, AccountNature.Credit, new Money(500m, Currency.IRR), "Purchase");
        var line2 = new JournalLine(account.Id, AccountNature.Debit, new Money(200m, Currency.IRR), "Payment");

        var balance = account.GetBalance(new[] { line1, line2 });

        Assert.Equal(new Money(300m, Currency.IRR), balance);
    }

    [Fact]
    public void GetBalance_NoLines_ReturnsZero()
    {
        var account = Account.Create("1000", "Cash", AccountType.Asset, Currency.IRR);

        var balance = account.GetBalance(Array.Empty<JournalLine>());

        Assert.Equal(Money.Zero(Currency.IRR), balance);
    }

    [Fact]
    public void UpdateDetails_ValidData_UpdatesAccount()
    {
        var account = Account.Create("1000", "Cash", AccountType.Asset, Currency.IRR);

        account.UpdateDetails("Cash on Hand", "Main cash account", true);

        Assert.Equal("Cash on Hand", account.Name);
        Assert.Equal("Main cash account", account.Description);
        Assert.True(account.IsDetail);
    }

    [Fact]
    public void Activate_DeactivatedAccount_ActivatesAccount()
    {
        var account = Account.Create("1000", "Cash", AccountType.Asset, Currency.IRR);
        account.Deactivate();

        account.Activate();

        Assert.True(account.IsActive);
    }

    [Fact]
    public void Deactivate_ActiveAccount_DeactivatesAccount()
    {
        var account = Account.Create("1000", "Cash", AccountType.Asset, Currency.IRR);

        account.Deactivate();

        Assert.False(account.IsActive);
    }

    [Fact]
    public void Deactivate_AccountWithActiveChildren_Throws()
    {
        var parent = Account.Create("1000", "Assets", AccountType.Asset, Currency.IRR, isDetail: false);
        var child = Account.Create("1100", "Current Assets", AccountType.Asset, Currency.IRR);
        child.SetParent(parent);

        Assert.Throws<InvalidOperationException>(() => parent.Deactivate());
    }

    [Fact]
    public void Deactivate_AccountWithoutActiveChildren_Succeeds()
    {
        var parent = Account.Create("1000", "Assets", AccountType.Asset, Currency.IRR, isDetail: false);
        var child = Account.Create("1100", "Current Assets", AccountType.Asset, Currency.IRR);
        child.SetParent(parent);
        child.Deactivate();

        parent.Deactivate();

        Assert.False(parent.IsActive);
    }

    [Fact]
    public void SetParent_ValidParent_SetsParent()
    {
        var parent = Account.Create("1000", "Assets", AccountType.Asset, Currency.IRR, isDetail: false);
        var child = Account.Create("1100", "Current Assets", AccountType.Asset, Currency.IRR);

        child.SetParent(parent);

        Assert.Equal(parent.Id, child.ParentAccountId);
        Assert.Contains(child, parent.Children);
    }

    [Fact]
    public void SetParent_SelfReference_Throws()
    {
        var account = Account.Create("1000", "Assets", AccountType.Asset, Currency.IRR);

        Assert.Throws<InvalidOperationException>(() => account.SetParent(account));
    }

    [Fact]
    public void SetParent_CircularReference_Throws()
    {
        var parent = Account.Create("1000", "Assets", AccountType.Asset, Currency.IRR, isDetail: false);
        var child = Account.Create("1100", "Current Assets", AccountType.Asset, Currency.IRR);
        var grandChild = Account.Create("1110", "Cash", AccountType.Asset, Currency.IRR);

        child.SetParent(parent);
        grandChild.SetParent(child);

        Assert.Throws<InvalidOperationException>(() => parent.SetParent(grandChild));
    }

    [Fact]
    public void RemoveParent_RemovesParentRelationship()
    {
        var parent = Account.Create("1000", "Assets", AccountType.Asset, Currency.IRR, isDetail: false);
        var child = Account.Create("1100", "Current Assets", AccountType.Asset, Currency.IRR);
        child.SetParent(parent);

        child.RemoveParent();

        Assert.Null(child.ParentAccountId);
        Assert.Null(child.ParentAccount);
        Assert.DoesNotContain(child, parent.Children);
    }
}