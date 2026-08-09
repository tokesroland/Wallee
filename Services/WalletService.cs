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

    public async Task<Wallet?> GetMainWalletAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        // A Fő számlát az IsMain flag alapján azonosítjuk (vagy a legelső aktív tárca, ha nincs IsMain megjelölve)
        return await db.Wallets.FirstOrDefaultAsync(w => w.Active && w.IsMain) 
            ?? await db.Wallets.Where(w => w.Active).OrderBy(w => w.ID).FirstOrDefaultAsync();
    }

    public async Task<int> GetTotalBalanceAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Wallets.Where(w => w.Active).SumAsync(w => (int?)w.Balance) ?? 0;
    }

    public async Task<int> GetAllocatedBalanceAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        var mainWalletId = await db.Wallets
            .Where(w => w.Active && w.IsMain)
            .Select(w => (int?)w.ID)
            .FirstOrDefaultAsync()
            ?? await db.Wallets.Where(w => w.Active).OrderBy(w => w.ID).Select(w => (int?)w.ID).FirstOrDefaultAsync();

        if (mainWalletId is null) return 0;

        return await db.Wallets
            .Where(w => w.Active && w.ID != mainWalletId)
            .SumAsync(w => (int?)w.Balance) ?? 0;
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

        // a hónap nettó pénzmozgása
        var txQ = db.Transactions.Where(t => t.Date >= monthStart);
        if (walletId is int wid2) txQ = txQ.Where(t => t.WalletID == wid2);

        int income = await txQ.Where(t => t.Type == TransactionType.income).SumAsync(t => (int?)t.Amount) ?? 0;
        int expense = await txQ.Where(t => t.Type == TransactionType.expense).SumAsync(t => (int?)t.Amount) ?? 0;
        int netThisMonth = income - expense;

        int prevBalance = currentBalance - netThisMonth;
        if (prevBalance == 0) return null; // nem értelmezhető %

        return Math.Round((decimal)netThisMonth / Math.Abs(prevBalance) * 100m, 1);
    }

    public async Task<Wallet> CreateAsync(Wallet wallet)
    {
        await using var db = await factory.CreateDbContextAsync();
        wallet.Active = true;
        db.Wallets.Add(wallet);
        await db.SaveChangesAsync();
        return wallet;
    }

    public async Task UpdateAsync(Wallet wallet)
    {
        await using var db = await factory.CreateDbContextAsync();
        var existing = await db.Wallets.FirstOrDefaultAsync(w => w.ID == wallet.ID);
        if (existing is null) return;

        existing.Name = wallet.Name;
        existing.Description = wallet.Description;
        existing.Icon = wallet.Icon;
        existing.Color = wallet.Color;

        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int walletId)
    {
        await using var db = await factory.CreateDbContextAsync();
        var walletToDelete = await db.Wallets.FirstOrDefaultAsync(w => w.ID == walletId);
        
        if (walletToDelete is null || !walletToDelete.Active) return;

        // Fő számlát nem lehet törölni!
        if (walletToDelete.IsMain)
        {
            throw new InvalidOperationException("A Fő számla nem törölhető!");
        }

        var mainWallet = await db.Wallets.FirstOrDefaultAsync(w => w.Active && w.IsMain) 
            ?? await db.Wallets.Where(w => w.Active && w.ID != walletId).OrderBy(w => w.ID).FirstOrDefaultAsync();

        if (mainWallet is null)
        {
            throw new InvalidOperationException("Nem található Fő számla a törlendő összeg visszautalásához.");
        }

        // Ha van rajta pénz, átmozgatjuk a Fő számlára
        if (walletToDelete.Balance > 0)
        {
            mainWallet.Balance += walletToDelete.Balance;
            walletToDelete.Balance = 0;
        }

        // Soft delete (inaktiválás)
        walletToDelete.Active = false;

        await db.SaveChangesAsync();
    }

    public async Task MoveFromMainAsync(int walletId, int amount, bool withdraw)
    {
        if (amount <= 0)
        {
            throw new InvalidOperationException("Az átutalandó összegnek pozitívnak kell lennie.");
        }

        await using var db = await factory.CreateDbContextAsync();
        var mainWallet = await db.Wallets.FirstOrDefaultAsync(w => w.Active && w.IsMain)
            ?? await db.Wallets.Where(w => w.Active).OrderBy(w => w.ID).FirstOrDefaultAsync();
        var selectedWallet = await db.Wallets.FirstOrDefaultAsync(w => w.ID == walletId && w.Active);

        if (mainWallet is null || selectedWallet is null)
        {
            throw new InvalidOperationException("A kiválasztott tárca vagy a fő számla nem található.");
        }

        var sourceWallet = withdraw ? selectedWallet : mainWallet;
        var targetWallet = withdraw ? mainWallet : selectedWallet;
        if (sourceWallet.ID == targetWallet.ID)
        {
            throw new InvalidOperationException("A fő számlán ez a művelet nem hajtható végre.");
        }

        if (sourceWallet.Balance < amount)
        {
            throw new InvalidOperationException($"Nincs elegendő fedezet! A(z) {sourceWallet.Name} tárca egyenlege: {sourceWallet.Balance:N0} Ft.");
        }

        sourceWallet.Balance -= amount;
        targetWallet.Balance += amount;
        db.Transactions.AddRange(
            new Transaction
            {
                Amount = amount,
                Type = TransactionType.expense,
                Description = withdraw ? $"Kivét: {selectedWallet.Name}" : $"Feltöltés: {selectedWallet.Name}",
                WalletID = sourceWallet.ID,
                Date = DateTime.Now
            },
            new Transaction
            {
                Amount = amount,
                Type = TransactionType.income,
                Description = withdraw ? $"Kivét: {selectedWallet.Name}" : $"Feltöltés: {selectedWallet.Name}",
                WalletID = targetWallet.ID,
                Date = DateTime.Now
            });

        await db.SaveChangesAsync();
    }

    public async Task TransferBalanceAsync(int sourceWalletId, int targetWalletId, int amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Az átutalandó összegnek pozitívnak kell lennie.");
        }

        if (sourceWalletId == targetWalletId)
        {
            throw new InvalidOperationException("Nem utalhatsz pénzt ugyanarra a tárcára.");
        }

        await using var db = await factory.CreateDbContextAsync();

        var sourceWallet = await db.Wallets.FirstOrDefaultAsync(w => w.ID == sourceWalletId && w.Active);
        var targetWallet = await db.Wallets.FirstOrDefaultAsync(w => w.ID == targetWalletId && w.Active);

        if (sourceWallet is null || targetWallet is null)
        {
            throw new InvalidOperationException("A megadott tárcák egyike nem található vagy inaktív.");
        }

        // Mínusz egyenleg elleni ellenőrzés
        if (sourceWallet.Balance < amount)
        {
            throw new InvalidOperationException($"Nincs elegendő fedezet! A(z) {sourceWallet.Name} tárca egyenlege: {sourceWallet.Balance:N0} Ft.");
        }

        // Egyenlegek átírása (Transaction rekord NÉLKÜL)
        sourceWallet.Balance -= amount;
        targetWallet.Balance += amount;

        await db.SaveChangesAsync();
    }
}