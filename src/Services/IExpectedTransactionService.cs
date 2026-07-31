using Wallee.Dtos;
using Wallee.Models;

namespace Wallee.Services;

public interface IExpectedTransactionService
{
    Task<List<ExpectedTransaction>> GetAllAsync(ExpectedTransactionFilter filter);
    Task<ExpectedTransaction?> GetByIdAsync(int id);
    Task<ExpectedTransaction> CreateAsync(ExpectedTransactionCreateDto dto);
    Task<ExpectedTransaction> UpdateAsync(int id, ExpectedTransactionUpdateDto dto);

    /// <summary>Pending tételt jóváhagy: létrehoz egy valós Transaction-t,
    /// frissíti a tárca egyenlegét, lezárja a rekordot (approved), és
    /// ismétlődő tétel esetén legenerálja a következő pendinget.</summary>
    Task ApproveAsync(int id);

    /// <summary>Pending tételt kihagy (skipped). Nem keletkezik Transaction
    /// és nem változik egyenleg; ismétlődő esetén generálódik a következő pending.</summary>
    Task SkipAsync(int id);

    Task<MonthlySummaryDto> GetMonthlySummaryAsync();
}
