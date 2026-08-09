using Microsoft.EntityFrameworkCore;
using Wallee.Data;
using Wallee.Dtos;
using Wallee.Models;

namespace Wallee.Services;

public class TransactionService(IDbContextFactory<WalleeDbContext> factory) : ITransactionService
{
    public async Task<List<Transaction>> GetAsync(TransactionFilter filter)
    {
        await using var db = await factory.CreateDbContextAsync();

        var q = db.Transactions
            .Include(t => t.Wallet)
            .Include(t => t.Category)
            .AsQueryable();

        if (filter.WalletID is int wid) q = q.Where(t => t.WalletID == wid);
        if (filter.CategoryID is int cid) q = q.Where(t => t.CategoryID == cid);
        if (filter.From is DateTime from) q = q.Where(t => t.Date >= from.Date);
        if (filter.To is DateTime to) q = q.Where(t => t.Date < to.Date.AddDays(1));
        if (filter.MaxAmount is int max) q = q.Where(t => t.Amount <= max);
        if (filter.Type is TransactionType type) q = q.Where(t => t.Type == type);
        if (filter.Description is string desc) q = q.Where(t => t.Description.Contains(desc));

        return await q.OrderByDescending(t => t.Date).ToListAsync();
    }

    public async Task<int> GetTodaySpendingAsync(int? walletId = null)
    {
        await using var db = await factory.CreateDbContextAsync();
        var today = DateTime.Today;

        var q = db.Transactions
            .Where(t => t.Type == TransactionType.expense)
            .Where(t => t.Date >= today && t.Date < today.AddDays(1));

        if (walletId is int wid) q = q.Where(t => t.WalletID == wid);

        return await q.SumAsync(t => (int?)t.Amount) ?? 0;
    }

    public async Task AddAsync(Transaction tx)
    {
        if (tx.Amount <= 0)
        {
            throw new InvalidOperationException("Az összegnek pozitívnak kell lennie.");
        }

        await using var db = await factory.CreateDbContextAsync();

        if (tx.CategoryID == 0) tx.CategoryID = null; // védőháló a select bind ellen

        var wallet = await db.Wallets.FirstOrDefaultAsync(w => w.ID == tx.WalletID);
        if (wallet is null || !wallet.Active)
        {
            throw new InvalidOperationException("A kiválasztott tárca nem található vagy inaktív.");
        }

        if (tx.Type == TransactionType.expense && wallet.Balance < tx.Amount)
        {
            throw new InvalidOperationException($"Nincs elegendő fedezet! A(z) {wallet.Name} tárca egyenlege: {wallet.Balance:N0} Ft.");
        }

        db.Transactions.Add(tx);
        if (tx.Type == TransactionType.income)
        {
            wallet.Balance += tx.Amount;
        }
        else if (tx.Type == TransactionType.expense)
        {
            wallet.Balance -= tx.Amount;
        }

        await db.SaveChangesAsync();
    }
}