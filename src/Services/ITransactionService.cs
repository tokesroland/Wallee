using Wallee.Dtos;
using Wallee.Models;

namespace Wallee.Services;

public interface ITransactionService
{
    Task<List<Transaction>> GetAsync(TransactionFilter filter);

    /// <summary>A mai napon elköltött összeg (csak kiadás).
    /// walletId == null → összes tárca.</summary>
    Task<int> GetTodaySpendingAsync(int? walletId = null);

    Task AddAsync(Transaction tx);
}