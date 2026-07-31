using Wallee.Models;

namespace Wallee.Services;

public interface IWalletService
{
    Task<List<Wallet>> GetActiveAsync();
    Task<Wallet?> GetByIdAsync(int id);
    Task<int> GetTotalBalanceAsync();

    /// <summary>Egyenleg %-os változása az előző hónap végéhez képest.
    /// walletId == null → összes tárca.</summary>
    Task<decimal?> GetMonthlyChangePercentAsync(int? walletId);
}