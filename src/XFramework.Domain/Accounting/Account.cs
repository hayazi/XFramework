using XFramework.Domain.Aggregates;
using XFramework.Domain.SharedKernel;

namespace XFramework.Domain.Accounting;

public sealed class Account : AggregateRoot<Guid>
{
    private Account() { }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public AccountType Type { get; private set; }
    public AccountNature Nature { get; private set; }
    public Currency Currency { get; private set; }
    public Guid? ParentAccountId { get; private set; }
    public Account? ParentAccount { get; private set; }
    private readonly List<Account> _children = new();
    public IReadOnlyCollection<Account> Children => _children.AsReadOnly();
    public bool IsActive { get; private set; }
    public bool IsDetail { get; private set; }

    public static Account Create(
        string code,
        string name,
        AccountType type,
        Currency currency,
        bool isDetail = true,
        Guid? parentAccountId = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Account code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Account name is required.", nameof(name));

        var nature = type switch
        {
            AccountType.Asset => AccountNature.Debit,
            AccountType.Expense => AccountNature.Debit,
            AccountType.Liability => AccountNature.Credit,
            AccountType.Equity => AccountNature.Credit,
            AccountType.Revenue => AccountNature.Credit,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        var account = new Account
        {
            Id = Guid.NewGuid(),
            Code = code.Trim(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Type = type,
            Nature = nature,
            Currency = currency,
            ParentAccountId = parentAccountId,
            IsActive = true,
            IsDetail = isDetail
        };

        account.AddDomainEvent(new AccountCreated(account.Id, account.Code, account.Name, account.Type));
        return account;
    }

    public void SetParent(Account parent)
    {
        if (parent.Id == Id)
            throw new InvalidOperationException("Account cannot be its own parent.");

        if (IsAncestorOf(parent))
            throw new InvalidOperationException("Circular reference detected in account hierarchy.");

        ParentAccount = parent;
        ParentAccountId = parent.Id;
        if (!parent._children.Contains(this))
            parent._children.Add(this);
    }

    public void RemoveParent()
    {
        if (ParentAccount is not null)
        {
            ParentAccount._children.Remove(this);
            ParentAccount = null;
            ParentAccountId = null;
        }
    }

    public bool IsAncestorOf(Account potentialDescendant)
    {
        var current = potentialDescendant.ParentAccount;
        while (current is not null)
        {
            if (current.Id == Id)
                return true;
            current = current.ParentAccount;
        }
        return false;
    }

    public void UpdateDetails(string name, string? description, bool? isDetail = null)
    {
        Name = name.Trim();
        Description = description?.Trim();
        if (isDetail.HasValue)
            IsDetail = isDetail.Value;

        AddDomainEvent(new AccountUpdated(Id, Name));
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        AddDomainEvent(new AccountActivated(Id));
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        if (_children.Any(c => c.IsActive))
            throw new InvalidOperationException("Cannot deactivate account with active children.");
        IsActive = false;
        AddDomainEvent(new AccountDeactivated(Id));
    }

    public Money GetBalance(IEnumerable<JournalLine> lines)
    {
        var balance = Money.Zero(Currency);
        foreach (var line in lines.Where(l => l.AccountId == Id))
        {
            balance = line.Side switch
            {
                AccountNature.Debit => balance + line.Amount,
                AccountNature.Credit => balance - line.Amount,
                _ => balance
            };
        }
        return Nature == AccountNature.Debit ? balance : -balance;
    }
}