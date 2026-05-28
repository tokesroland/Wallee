using Microsoft.EntityFrameworkCore;
using Wallee.Data;
using Wallee.Models;

namespace Wallee.Services;

public class WalletService(IDbContextFactory<WalleeDbContext> factory) : IWalletService
{
    public async Task<List<Wallet>> GetActiveAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Wallets.Where(w => w.Active).OrderBy(w => w.ID).ToListAsync();
    }

    public async Task<Wallet?> GetByIdAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Wallets.FirstOrDefaultAsync(w => w.ID == id);
    }

    public async Task<int> GetTotalBalanceAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Wallets.Where(w => w.Active).SumAsync(w => (int?)w.Balance) ?? 0;
    }

    public async Task<decimal?> GetMonthlyChangePercentAsync(int? walletId)
    {
        await using var db = await factory.CreateDbContextAsync();

        // jelenlegi egyenleg
        var balanceQ = db.Wallets.Where(w => w.Active);
        if (walletId is int wid) balanceQ = balanceQ.Where(w => w.ID == wid);
        int currentBalance = await balanceQ.SumAsync(w => (int?)w.Balance) ?? 0;

        // folyó hónap eleje
        var now = DateTime.Now;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        // a hónap nettó pénzmozgása (transfer = semleges phase 1-ben)
        var txQ = db.Transactions.Where(t => t.Date >= monthStart);
        if (walletId is int wid2) txQ = txQ.Where(t => t.WalletID == wid2);

        int income = await txQ.Where(t => t.Type == TransactionType.income).SumAsync(t => (int?)t.Amount) ?? 0;
        int expense = await txQ.Where(t => t.Type == TransactionType.expense).SumAsync(t => (int?)t.Amount) ?? 0;
        int netThisMonth = income - expense;

        int prevBalance = currentBalance - netThisMonth;
        if (prevBalance == 0) return null; // nem értelmezhető %

        return Math.Round((decimal)netThisMonth / Math.Abs(prevBalance) * 100m, 1);
    }
}