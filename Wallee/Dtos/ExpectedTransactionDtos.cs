using Wallee.Models;

namespace Wallee.Dtos;

public record ExpectedTransactionCreateDto(
    string Title,
    string? Description,
    int Amount,
    ExpectedTransactionType Type,
    int? CategoryID,
    RecurrenceType Recurrence,
    int? RecurrenceDay,
    DateTime ExpectedDate
);

public record ExpectedTransactionUpdateDto(
    string Title,
    string? Description,
    int Amount,
    int? CategoryID,
    RecurrenceType Recurrence,
    int? RecurrenceDay,
    DateTime ExpectedDate
);

public enum StatusViewMode { PendingOnly, All }

public record ExpectedTransactionFilter(
    ExpectedTransactionType? TypeFilter = null,
    int? CategoryID = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    int? MaxAmount = null,
    string? TitleSearch = null,
    StatusViewMode StatusMode = StatusViewMode.PendingOnly
);

public record MonthlySummaryDto
{
    public int TotalIncome { get; init; }
    public int TotalExpense { get; init; }
    public int IncomeCount { get; init; }
    public int ExpenseCount { get; init; }
    public int Count { get; init; }
    public ExpectedTransaction? NextEvent { get; init; }

    public int Net => TotalIncome - TotalExpense;
}
