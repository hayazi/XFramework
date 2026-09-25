using XFramework.Domain.Aggregates;
using XFramework.Domain.SharedKernel;

namespace XFramework.Domain.Accounting;

public sealed class JournalEntry : AggregateRoot<Guid>
{
    private readonly List<JournalLine> _lines = new();

    private JournalEntry() { }

    public string Reference { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DocumentStatus Status { get; private set; }
    public PostingStatus PostingStatus { get; private set; }
    public Guid? PartyId { get; private set; }
    public IReadOnlyCollection<JournalLine> Lines => _lines.AsReadOnly();
    public Money TotalDebit => CalculateTotal(AccountNature.Debit);
    public Money TotalCredit => CalculateTotal(AccountNature.Credit);
    public bool IsBalanced => TotalDebit == TotalCredit;
    public DateTime? PostedOnUtc { get; private set; }
    public Guid? PostedBy { get; private set; }

    public static JournalEntry Create(
        string reference,
        DateTime date,
        string description,
        Guid? partyId = null)
    {
        if (string.IsNullOrWhiteSpace(reference))
            throw new ArgumentException("Reference is required.", nameof(reference));

        var entry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            Reference = reference.Trim(),
            Date = date.Date,
            Description = description?.Trim() ?? string.Empty,
            Status = DocumentStatus.Draft,
            PostingStatus = PostingStatus.Unposted,
            PartyId = partyId
        };

        entry.AddDomainEvent(new JournalEntryCreated(entry.Id, entry.Reference, entry.Date));
        return entry;
    }

    public void AddLine(
        Guid accountId,
        AccountNature side,
        Money amount,
        string description = "",
        Guid? dimensionValueId = null)
    {
        if (amount.Amount <= 0m)
            throw new ArgumentException("Line amount must be positive.", nameof(amount));

        if (Status != DocumentStatus.Draft)
            throw new InvalidOperationException("Cannot add lines to non-draft journal entry.");

        var line = new JournalLine(accountId, side, amount, description, dimensionValueId);
        _lines.Add(line);
    }

public void RemoveLine(Guid accountId, AccountNature side)
        {
            if (Status != DocumentStatus.Draft)
                throw new InvalidOperationException("Cannot remove lines from non-draft journal entry.");

            var line = _lines.FirstOrDefault(l => l.AccountId == accountId && l.Side == side);
            if (!line.Equals(default(JournalLine)))
                _lines.Remove(line);
        }

    public void ClearLines()
    {
        if (Status != DocumentStatus.Draft)
            throw new InvalidOperationException("Cannot clear lines from non-draft journal entry.");

        _lines.Clear();
    }

    public void Submit()
    {
        if (Status != DocumentStatus.Draft)
            throw new InvalidOperationException("Only draft entries can be submitted.");

        if (!IsBalanced)
            throw new InvalidOperationException("Journal entry must be balanced before submission.");

        if (_lines.Count == 0)
            throw new InvalidOperationException("Journal entry must have at least one line.");

        Status = DocumentStatus.Submitted;
        AddDomainEvent(new JournalEntrySubmitted(Id, Reference));
    }

    public void Approve(Guid approvedBy)
    {
        if (Status != DocumentStatus.Submitted)
            throw new InvalidOperationException("Only submitted entries can be approved.");

        Status = DocumentStatus.Approved;
        AddDomainEvent(new JournalEntryApproved(Id, approvedBy));
    }

    public void Reject(string reason)
    {
        if (Status != DocumentStatus.Submitted)
            throw new InvalidOperationException("Only submitted entries can be rejected.");

        Status = DocumentStatus.Rejected;
        AddDomainEvent(new JournalEntryRejected(Id, reason));
    }

    public void Post(Guid postedBy)
    {
        if (Status != DocumentStatus.Approved)
            throw new InvalidOperationException("Only approved entries can be posted.");

        if (!IsBalanced)
            throw new InvalidOperationException("Journal entry must be balanced before posting.");

        Status = DocumentStatus.Posted;
        PostingStatus = PostingStatus.Posted;
        PostedOnUtc = DateTime.UtcNow;
        PostedBy = postedBy;

        AddDomainEvent(new JournalEntryPosted(Id, Reference, postedBy, DateTime.UtcNow));
    }

    public void Reverse(Guid reversedBy, string reason)
    {
        if (PostingStatus != PostingStatus.Posted)
            throw new InvalidOperationException("Only posted entries can be reversed.");

        PostingStatus = PostingStatus.Reversed;
        AddDomainEvent(new JournalEntryReversed(Id, reversedBy, reason));
    }

    public void Cancel()
    {
        if (Status == DocumentStatus.Posted)
            throw new InvalidOperationException("Cannot cancel posted entry. Use reverse instead.");

        Status = DocumentStatus.Cancelled;
        AddDomainEvent(new JournalEntryCancelled(Id));
    }

    private Money CalculateTotal(AccountNature side)
    {
        var firstLine = _lines.FirstOrDefault();
        var currency = !firstLine.Equals(default(JournalLine)) ? firstLine.Amount.Currency : Currency.IRR;
        var total = Money.Zero(currency);

        foreach (var line in _lines.Where(l => l.Side == side))
        {
            if (line.Amount.Currency != currency)
                throw new InvalidOperationException("All lines must be in the same currency.");
            total += line.Amount;
        }

        return total;
    }
}