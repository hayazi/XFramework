using XFramework.Application.Contracts.Abstractions;
using XFramework.Application.Contracts.Dtos;
using XFramework.Domain.Accounting;
using XFramework.Domain.SharedKernel;

namespace XFramework.Application.Contracts.Accounting;

public class FiscalPeriodDto : EntityDto<Guid>
{
    public int Year { get; set; }
    public int PeriodNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateRangeDto DateRange { get; set; } = new();
    public FiscalPeriodStatus Status { get; set; }
    public DateTime? ClosedOnUtc { get; set; }
    public Guid? ClosedBy { get; set; }
}

public class DateRangeDto
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public bool Contains(DateTime date) => Start <= date && date <= End;
    public int DurationDays => (End - Start).Days + 1;
}

public class FiscalPeriodCreateDto
{
    public int Year { get; set; }
    public int PeriodNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateRangeDto DateRange { get; set; } = new();
}

public class FiscalPeriodUpdateDto
{
    public string Name { get; set; } = string.Empty;
}

public class FiscalPeriodCloseDto
{
    public Guid ClosedBy { get; set; }
}

public interface IFiscalPeriodAppService : ICrudAppService<FiscalPeriodDto, Guid, FiscalPeriodCreateDto, FiscalPeriodUpdateDto>
{
    Task<FiscalPeriodDto?> GetByYearAndPeriodAsync(int year, int periodNumber, CancellationToken cancellationToken = default);
    Task<List<FiscalPeriodDto>> GetByYearAsync(int year, CancellationToken cancellationToken = default);
    Task<List<FiscalPeriodDto>> GetByStatusAsync(FiscalPeriodStatus status, CancellationToken cancellationToken = default);
    Task<FiscalPeriodDto> CloseAsync(Guid id, FiscalPeriodCloseDto input, CancellationToken cancellationToken = default);
    Task<FiscalPeriodDto> ReopenAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FiscalPeriodDto> LockAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FiscalPeriodDto> UnlockAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FiscalPeriodDto?> GetCurrentPeriodAsync(DateTime date, CancellationToken cancellationToken = default);
}