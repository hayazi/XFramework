using XFramework.Domain.SharedKernel;

namespace XFramework.Domain.Tax;

public sealed class TaxTier
{
    public decimal ThresholdFrom { get; init; }
    public decimal ThresholdTo { get; init; }
    public Percentage Rate { get; init; }

    public TaxTier(decimal thresholdFrom, decimal thresholdTo, Percentage rate)
    {
        if (thresholdFrom < 0)
            throw new ArgumentException("Threshold from cannot be negative.", nameof(thresholdFrom));
        if (thresholdTo <= thresholdFrom)
            throw new ArgumentException("Threshold to must be greater than threshold from.", nameof(thresholdTo));
        
        ThresholdFrom = thresholdFrom;
        ThresholdTo = thresholdTo;
        Rate = rate;
    }
}