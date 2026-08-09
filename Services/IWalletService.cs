using Wallee.Models;

namespace Wallee.Services;

public interface IWalletService
{
    Task<List<Wallet>> GetActiveAsync();
    Task<Wallet?> GetByIdAsync(int id);
    Task<Wallet?> GetMainWalletAsync();
    Task<int> GetTotalBalanceAsync();
    Task<int> GetAllocatedBalanceAsync();

    Task<decimal?> GetMonthlyChangePercentAsync(int? walletId);

    Task<Wallet> CreateAsync(Wallet wallet);
    Task UpdateAsync(Wallet wallet);
    Task DeleteAsync(int walletId);
    Task MoveFromMainAsync(int walletId, int amount, bool withdraw);
    Task TransferBalanceAsync(int sourceWalletId, int targetWalletId, int amount);
}