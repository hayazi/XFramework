using XFramework.Domain.Aggregates;

namespace XFramework.Domain.Numbering;

public sealed class NumberSequence : AggregateRoot<Guid>
{
    private NumberSequence() { }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Prefix { get; private set; } = string.Empty;
    public string Suffix { get; private set; } = string.Empty;
    public int CurrentNumber { get; private set; }
    public int MinimumDigits { get; private set; }
    public int IncrementBy { get; private set; }
    public int? MaximumNumber { get; private set; }
    public NumberingScope Scope { get; private set; }
    public string? ScopeIdentifier { get; private set; }
    public NumberingStatus Status { get; private set; }
    public DateTime? ResetDate { get; private set; }
    public bool AutoReset { get; private set; }
    public string? FormatTemplate { get; private set; }

    public static NumberSequence Create(
        string code,
        string name,
        string prefix,
        int minimumDigits = 6,
        int incrementBy = 1,
        int? maximumNumber = null,
        NumberingScope scope = NumberingScope.Global,
        string? scopeIdentifier = null,
        string? suffix = null,
        bool autoReset = false,
        DateTime? resetDate = null,
        string? formatTemplate = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Sequence code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Sequence name is required.", nameof(name));
        if (minimumDigits < 1 || minimumDigits > 20)
            throw new ArgumentException("Minimum digits must be between 1 and 20.", nameof(minimumDigits));
        if (incrementBy < 1)
            throw new ArgumentException("Increment must be at least 1.", nameof(incrementBy));
        if (maximumNumber.HasValue && maximumNumber.Value <= 0)
            throw new ArgumentException("Maximum number must be positive.", nameof(maximumNumber));

        var sequence = new NumberSequence
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Prefix = prefix?.Trim() ?? string.Empty,
            Suffix = suffix?.Trim() ?? string.Empty,
            CurrentNumber = 0,
            MinimumDigits = minimumDigits,
            IncrementBy = incrementBy,
            MaximumNumber = maximumNumber,
            Scope = scope,
            ScopeIdentifier = scopeIdentifier?.Trim(),
            Status = NumberingStatus.Active,
            AutoReset = autoReset,
            ResetDate = resetDate,
            FormatTemplate = formatTemplate?.Trim()
        };

        sequence.AddDomainEvent(new NumberSequenceCreated(sequence.Id, sequence.Code, sequence.Name));
        return sequence;
    }

    public string GetNextNumber()
    {
        if (Status != NumberingStatus.Active)
            throw new InvalidOperationException($"Sequence {Code} is not active.");

        CheckAndAutoReset();

        if (MaximumNumber.HasValue && CurrentNumber >= MaximumNumber.Value)
        {
            Status = NumberingStatus.Exhausted;
            AddDomainEvent(new NumberSequenceExhausted(Id, Code));
            throw new InvalidOperationException($"Sequence {Code} has been exhausted.");
        }

        CurrentNumber += IncrementBy;
        var number = FormatNumber(CurrentNumber);
        
        AddDomainEvent(new NumberSequenceNumberGenerated(Id, Code, number, CurrentNumber));
        return number;
    }

    public string PeekNextNumber()
    {
        CheckAndAutoReset();
        
        var nextNumber = CurrentNumber + IncrementBy;
        if (MaximumNumber.HasValue && nextNumber > MaximumNumber.Value)
            throw new InvalidOperationException($"Next number would exceed maximum for sequence {Code}.");
        
        return FormatNumber(nextNumber);
    }

    private string FormatNumber(int number)
    {
        var numberPart = number.ToString($"D{MinimumDigits}");
        
        if (!string.IsNullOrEmpty(FormatTemplate))
        {
            return FormatTemplate
                .Replace("{PREFIX}", Prefix)
                .Replace("{NUMBER}", numberPart)
                .Replace("{SUFFIX}", Suffix)
                .Replace("{SCOPE}", ScopeIdentifier ?? string.Empty);
        }

        return $"{Prefix}{numberPart}{Suffix}";
    }

    private void CheckAndAutoReset()
    {
        if (AutoReset && ResetDate.HasValue && DateTime.UtcNow.Date >= ResetDate.Value.Date)
        {
            Reset();
        }
    }

    public void Reset()
    {
        CurrentNumber = 0;
        if (ResetDate.HasValue)
        {
            ResetDate = ResetDate.Value.AddYears(1);
        }
        AddDomainEvent(new NumberSequenceReset(Id, Code));
    }

    public void UpdateDetails(
        string name,
        string? prefix = null,
        string? suffix = null,
        int? minimumDigits = null,
        int? incrementBy = null,
        int? maximumNumber = null,
        NumberingStatus? status = null,
        bool? autoReset = null,
        DateTime? resetDate = null,
        string? formatTemplate = null)
    {
        Name = name.Trim();
        
        if (prefix != null) Prefix = prefix.Trim();
        if (suffix != null) Suffix = suffix.Trim();
        if (minimumDigits.HasValue)
        {
            if (minimumDigits.Value < 1 || minimumDigits.Value > 20)
                throw new ArgumentException("Minimum digits must be between 1 and 20.", nameof(minimumDigits));
            MinimumDigits = minimumDigits.Value;
        }
        if (incrementBy.HasValue)
        {
            if (incrementBy.Value < 1)
                throw new ArgumentException("Increment must be at least 1.", nameof(incrementBy));
            IncrementBy = incrementBy.Value;
        }
        if (maximumNumber.HasValue)
        {
            if (maximumNumber.Value <= 0)
                throw new ArgumentException("Maximum number must be positive.", nameof(maximumNumber));
            if (CurrentNumber > maximumNumber.Value)
                throw new InvalidOperationException("Cannot set maximum below current number.");
            MaximumNumber = maximumNumber.Value;
        }
        if (status.HasValue) Status = status.Value;
        if (autoReset.HasValue) AutoReset = autoReset.Value;
        if (resetDate.HasValue) ResetDate = resetDate.Value;
        if (formatTemplate != null) FormatTemplate = formatTemplate.Trim();

        AddDomainEvent(new NumberSequenceUpdated(Id, Code, Name));
    }

    public void Activate()
    {
        if (Status == NumberingStatus.Active) return;
        if (Status == NumberingStatus.Exhausted && !MaximumNumber.HasValue)
            throw new InvalidOperationException("Cannot activate exhausted sequence without maximum number.");
        
        Status = NumberingStatus.Active;
        AddDomainEvent(new NumberSequenceActivated(Id));
    }

    public void Deactivate()
    {
        if (Status == NumberingStatus.Inactive) return;
        Status = NumberingStatus.Inactive;
        AddDomainEvent(new NumberSequenceDeactivated(Id));
    }

    public void SetScope(string? scopeIdentifier)
    {
        ScopeIdentifier = scopeIdentifier?.Trim();
    }
}