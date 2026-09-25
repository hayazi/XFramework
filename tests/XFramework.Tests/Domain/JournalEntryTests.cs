using XFramework.Domain.Accounting;
using XFramework.Domain.SharedKernel;
using Xunit;

namespace XFramework.Tests.Domain;

public class JournalEntryTests
{
    [Fact]
    public void Create_ValidData_CreatesJournalEntry()
    {
        var entry = JournalEntry.Create("JE-001", DateTime.UtcNow, "Test entry");

        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Equal("JE-001", entry.Reference);
        Assert.Equal("Test entry", entry.Description);
        Assert.Equal(DocumentStatus.Draft, entry.Status);
        Assert.Equal(PostingStatus.Unposted, entry.PostingStatus);
        Assert.Empty(entry.Lines);
    }

    [Fact]
    public void Create_EmptyReference_Throws()
    {
        Assert.Throws<ArgumentException>(() => JournalEntry.Create("", DateTime.UtcNow, "Test entry"));
    }

    [Fact]
    public void AddLine_DraftEntry_AddsLine()
    {
        var entry = JournalEntry.Create("JE-001", DateTime.UtcNow, "Test entry");
        entry.AddLine(Guid.NewGuid(), AccountNature.Debit, new Money(100m, Currency.IRR), "Line 1");

        Assert.Single(entry.Lines);
    }

    [Fact]
    public void AddLine_NonDraftEntry_Throws()
    {
        var entry = JournalEntry.Create("JE-001", DateTime.UtcNow, "Test entry");
        var account1 = Guid.NewGuid();
        var account2 = Guid.NewGuid();
        entry.AddLine(account1, AccountNature.Debit, new Money(100m, Currency.IRR), "Line 1");
        entry.AddLine(account2, AccountNature.Credit, new Money(100m, Currency.IRR), "Line 2");
        entry.Submit();

        Assert.Throws<InvalidOperationException>(() =>
            entry.AddLine(Guid.NewGuid(), AccountNature.Credit, new Money(100m, Currency.IRR), "Line 3"));
    }

    [Fact]
    public void RemoveLine_ExistingLine_RemovesLine()
    {
        var accountId = Guid.NewGuid();
        var entry = JournalEntry.Create("JE-001", DateTime.UtcNow, "Test entry");
        entry.AddLine(accountId, AccountNature.Debit, new Money(100m, Currency.IRR), "Line 1");

        entry.RemoveLine(accountId, AccountNature.Debit);

        Assert.Empty(entry.Lines);
    }

    [Fact]
    public void RemoveLine_NonExistingLine_DoesNothing()
    {
        var entry = JournalEntry.Create("JE-001", DateTime.UtcNow, "Test entry");
        entry.AddLine(Guid.NewGuid(), AccountNature.Debit, new Money(100m, Currency.IRR), "Line 1");

        entry.RemoveLine(Guid.NewGuid(), AccountNature.Debit);

        Assert.Single(entry.Lines);
    }

    [Fact]
    public void IsBalanced_EqualDebitsAndCredits_ReturnsTrue()
    {
        var entry = JournalEntry.Create("JE-001", DateTime.UtcNow, "Test entry");
        var account1 = Guid.NewGuid();
        var account2 = Guid.NewGuid();

        entry.AddLine(account1, AccountNature.Debit, new Money(100m, Currency.IRR), "Debit line");
        entry.AddLine(account2, AccountNature.Credit, new Money(100m, Currency.IRR), "Credit line");

        Assert.True(entry.IsBalanced);
    }

    [Fact]
    public void IsBalanced_UnequalDebitsAndCredits_ReturnsFalse()
    {
        var entry = JournalEntry.Create("JE-001", DateTime.UtcNow, "Test entry");
        var account1 = Guid.NewGuid();
        var account2 = Guid.NewGuid();

        entry.AddLine(account1, AccountNature.Debit, new Money(100m, Currency.IRR), "Debit line");
        entry.AddLine(account2, AccountNature.Credit, new Money(50m, Currency.IRR), "Credit line");

        Assert.False(entry.IsBalanced);
    }

    [Fact]
    public void IsBalanced_MixedCurrencies_Throws()
    {
        var entry = JournalEntry.Create("JE-001", DateTime.UtcNow, "Test entry");
        var account1 = Guid.NewGuid();
        var account2 = Guid.NewGuid();

        entry.AddLine(account1, AccountNature.Debit, new Money(100m, Currency.IRR), "Debit line");
        entry.AddLine(account2, AccountNature.Credit, new Money(100m, Currency.USD), "Credit line");

        Assert.Throws<InvalidOperationException>(() => _ = entry.IsBalanced);
    }

    [Fact]
    public void Submit_BalancedEntry_ChangesStatusToSubmitted()
    {
        var entry = JournalEntry.Create("JE-001", DateTime.UtcNow, "Test entry");
        var account1 = Guid.NewGuid();
        var account2 = Guid.NewGuid();

        entry.AddLine(account1, AccountNature.Debit, new Money(100m, Currency.IRR), "Debit line");
        entry.AddLine(account2, AccountNature.Credit, new Money(100m, Currency.IRR), "Credit line");

        entry.Submit();

        Assert.Equal(DocumentStatus.Submitted, entry.Status);
    }

    [Fact]
    public void Submit_UnbalancedEntry_Throws()
    {
        var entry = JournalEntry.Create("JE-001", DateTime.UtcNow, "Test entry");
        var account1 = Guid.NewGuid();
        var account2 = Guid.NewGuid();

        entry.AddLine(account1, AccountNature.Debit, new Money(100m, Currency.IRR), "Debit line");
        entry.AddLine(account2, AccountNature.Credit, new Money(50m, Currency.IRR), "Credit line");

        Assert.Throws<InvalidOperationException>(() => entry.Submit());
    }

    [Fact]
    public void Submit_EmptyEntry_Throws()
    {
        var entry = JournalEntry.Create("JE-001", DateTime.UtcNow, "Test entry");

        Assert.Throws<InvalidOperationException>(() => entry.Submit());
    }

    [Fact]
    public void Approve_SubmittedEntry_ChangesStatusToApproved()
    {
        var entry = JournalEntry.Create("JE-001", DateTime.UtcNow, "Test entry");
        var account1 = Guid.NewGuid();
        var account2 = Guid.NewGuid();

        entry.AddLine(account1, AccountNature.Debit, new Money(100m, Currency.IRR), "Debit line");
        entry.AddLine(account2, AccountNature.Credit, new Money(100m, Currency.IRR), "Credit line");
        entry.Submit();

        entry.Approve(Guid.NewGuid());

        Assert.Equal(DocumentStatus.Approved, entry.Status);
    }

    [Fact]
    public void Approve_NonSubmittedEntry_Throws()
    {
        var entry = JournalEntry.Create("JE-001", DateTime.UtcNow, "Test entry");

        Assert.Throws<InvalidOperationException>(() => entry.Approve(Guid.NewGuid()));
    }

    [Fact]
    public void Post_ApprovedEntry_ChangesStatusToPosted()
    {
        var entry = JournalEntry.Create("JE-001", DateTime.UtcNow, "Test entry");
        var account1 = Guid.NewGuid();
        var account2 = Guid.NewGuid();

        entry.AddLine(account1, AccountNature.Debit, new Money(100m, Currency.IRR), "Debit line");
        entry.AddLine(account2, AccountNature.Credit, new Money(100m, Currency.IRR), "Credit line");
        entry.Submit();
        entry.Approve(Guid.NewGuid());

        entry.Post(Guid.NewGuid());

        Assert.Equal(DocumentStatus.Posted, entry.Status);
        Assert.Equal(PostingStatus.Posted, entry.PostingStatus);
        Assert.NotNull(entry.PostedOnUtc);
    }

    [Fact]
    public void Post_NonApprovedEntry_Throws()
    {
        var entry = JournalEntry.Create("JE-001", DateTime.UtcNow, "Test entry");
        var account1 = Guid.NewGuid();
        var account2 = Guid.NewGuid();

        entry.AddLine(account1, AccountNature.Debit, new Money(100m, Currency.IRR), "Debit line");
        entry.AddLine(account2, AccountNature.Credit, new Money(100m, Currency.IRR), "Credit line");
        entry.Submit();

        Assert.Throws<InvalidOperationException>(() => entry.Post(Guid.NewGuid()));
    }

    [Fact]
    public void Reverse_PostedEntry_ChangesPostingStatusToReversed()
    {
        var entry = JournalEntry.Create("JE-001", DateTime.UtcNow, "Test entry");
        var account1 = Guid.NewGuid();
        var account2 = Guid.NewGuid();

        entry.AddLine(account1, AccountNature.Debit, new Money(100m, Currency.IRR), "Debit line");
        entry.AddLine(account2, AccountNature.Credit, new Money(100m, Currency.IRR), "Credit line");
        entry.Submit();
        entry.Approve(Guid.NewGuid());
        entry.Post(Guid.NewGuid());

        entry.Reverse(Guid.NewGuid(), "Test reversal");

        Assert.Equal(PostingStatus.Reversed, entry.PostingStatus);
    }

    [Fact]
    public void Reverse_NonPostedEntry_Throws()
    {
        var entry = JournalEntry.Create("JE-001", DateTime.UtcNow, "Test entry");
        var account1 = Guid.NewGuid();
        var account2 = Guid.NewGuid();

        entry.AddLine(account1, AccountNature.Debit, new Money(100m, Currency.IRR), "Debit line");
        entry.AddLine(account2, AccountNature.Credit, new Money(100m, Currency.IRR), "Credit line");
        entry.Submit();
        entry.Approve(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => entry.Reverse(Guid.NewGuid(), "Test reversal"));
    }

    [Fact]
    public void ClearLines_DraftEntry_ClearsLines()
    {
        var entry = JournalEntry.Create("JE-001", DateTime.UtcNow, "Test entry");
        var account1 = Guid.NewGuid();
        var account2 = Guid.NewGuid();

        entry.AddLine(account1, AccountNature.Debit, new Money(100m, Currency.IRR), "Debit line");
        entry.AddLine(account2, AccountNature.Credit, new Money(100m, Currency.IRR), "Credit line");

        entry.ClearLines();

        Assert.Empty(entry.Lines);
    }

    [Fact]
    public void ClearLines_NonDraftEntry_Throws()
    {
        var entry = JournalEntry.Create("JE-001", DateTime.UtcNow, "Test entry");
        var account1 = Guid.NewGuid();
        var account2 = Guid.NewGuid();
        entry.AddLine(account1, AccountNature.Debit, new Money(100m, Currency.IRR), "Line 1");
        entry.AddLine(account2, AccountNature.Credit, new Money(100m, Currency.IRR), "Line 2");
        entry.Submit();

        Assert.Throws<InvalidOperationException>(() => entry.ClearLines());
    }
}