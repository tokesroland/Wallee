using Microsoft.EntityFrameworkCore;
using Wallee.Data;
using Wallee.Dtos;
using Wallee.Models;

namespace Wallee.Services;

public class ExpectedTransactionService(IDbContextFactory<WalleeDbContext> factory) : IExpectedTransactionService
{
    public async Task<List<ExpectedTransaction>> GetAllAsync(ExpectedTransactionFilter filter)
    {
        await using var db = await factory.CreateDbContextAsync();

        var q = db.ExpectedTransactions
            .Include(e => e.Wallet)
            .Include(e => e.Category)
            .Include(e => e.LinkedTransaction)
            .AsQueryable();

        // státusz nézet: kártyasorokhoz csak pending, táblázathoz minden
        if (filter.StatusMode == StatusViewMode.PendingOnly)
            q = q.Where(e => e.Status == ExpectedStatus.pending);

        if (filter.TypeFilter is ExpectedTransactionType type) q = q.Where(e => e.Type == type);
        if (filter.CategoryID is int cid) q = q.Where(e => e.CategoryID == cid);
        if (filter.DateFrom is DateTime from) q = q.Where(e => e.ExpectedDate >= from.Date);
        if (filter.DateTo is DateTime to) q = q.Where(e => e.ExpectedDate < to.Date.AddDays(1));
        if (filter.MaxAmount is int max) q = q.Where(e => e.Amount <= max);
        if (!string.IsNullOrWhiteSpace(filter.TitleSearch))
        {
            var s = filter.TitleSearch;
            q = q.Where(e => e.Title.Contains(s) || (e.Description != null && e.Description.Contains(s)));
        }

        // a táblázat dátum szerint növekvő, a kártyák összeg szerint a hívó oldalon rendeződnek
        return await q.OrderBy(e => e.ExpectedDate).ToListAsync();
    }

    public async Task<ExpectedTransaction?> GetByIdAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.ExpectedTransactions
            .Include(e => e.Wallet)
            .Include(e => e.Category)
            .Include(e => e.LinkedTransaction)
            .FirstOrDefaultAsync(e => e.ID == id);
    }

    public async Task<ExpectedTransaction> CreateAsync(ExpectedTransactionCreateDto dto)
    {
        await using var db = await factory.CreateDbContextAsync();

        var entity = new ExpectedTransaction
        {
            Title = dto.Title,
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description,
            Amount = dto.Amount,
            Type = dto.Type,
            WalletID = 1,                                   // mindig a fő számla
            CategoryID = dto.CategoryID == 0 ? null : dto.CategoryID,
            Status = ExpectedStatus.pending,
            Recurrence = dto.Recurrence,
            RecurrenceDay = dto.RecurrenceDay,
            ExpectedDate = dto.ExpectedDate,
            CreatedAt = DateTime.Now
        };

        db.ExpectedTransactions.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    public async Task<ExpectedTransaction> UpdateAsync(int id, ExpectedTransactionUpdateDto dto)
    {
        await using var db = await factory.CreateDbContextAsync();

        var entity = await db.ExpectedTransactions.FindAsync(id)
                     ?? throw new KeyNotFoundException("A tétel nem található.");

        if (entity.Status != ExpectedStatus.pending)
            throw new InvalidOperationException("Csak függőben lévő tétel módosítható.");

        entity.Title = dto.Title;
        entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description;
        entity.Amount = dto.Amount;
        entity.CategoryID = dto.CategoryID == 0 ? null : dto.CategoryID;
        entity.Recurrence = dto.Recurrence;
        entity.RecurrenceDay = dto.RecurrenceDay;
        entity.ExpectedDate = dto.ExpectedDate;

        await db.SaveChangesAsync();
        return entity;
    }

    public async Task ApproveAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();

        var expected = await db.ExpectedTransactions.FindAsync(id)
                       ?? throw new KeyNotFoundException("A tétel nem található.");

        if (expected.Status != ExpectedStatus.pending)
            throw new InvalidOperationException("Csak függőben lévő tétel hagyható jóvá.");

        // 1. Valós Transaction létrehozása
        var transaction = new Transaction
        {
            Amount = expected.Amount,
            Type = expected.Type == ExpectedTransactionType.income
                       ? TransactionType.income
                       : TransactionType.expense,
            WalletID = expected.WalletID,
            CategoryID = expected.CategoryID,
            Description = expected.Title + (expected.Description != null ? $" — {expected.Description}" : ""),
            Date = DateTime.Now,
            Currency = "HUF"
        };
        db.Transactions.Add(transaction);

        // 2. Tárca egyenleg frissítése — KIZÁRÓLAG itt, hogy ne legyen dupla könyvelés
        var wallet = await db.Wallets.FindAsync(expected.WalletID);
        if (wallet is not null)
        {
            wallet.Balance += transaction.Type == TransactionType.income
                              ? expected.Amount
                              : -expected.Amount;
        }

        // 3. Eredeti rekord lezárása (NEM törlés — approved marad audit trail-ként)
        expected.Status = ExpectedStatus.approved;
        await db.SaveChangesAsync();   // itt kap ID-t a transaction

        expected.LinkedTransactionID = transaction.ID;

        // 4. Ha ismétlődő: következő pending generálása
        if (expected.Recurrence != RecurrenceType.once)
        {
            db.ExpectedTransactions.Add(new ExpectedTransaction
            {
                Title = expected.Title,
                Description = expected.Description,
                Amount = expected.Amount,
                Type = expected.Type,
                WalletID = expected.WalletID,
                CategoryID = expected.CategoryID,
                Recurrence = expected.Recurrence,
                RecurrenceDay = expected.RecurrenceDay,
                Status = ExpectedStatus.pending,
                ExpectedDate = CalculateNextDate(expected.ExpectedDate, expected.Recurrence, expected.RecurrenceDay)
            });
        }

        await db.SaveChangesAsync();
    }

    public async Task SkipAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();

        var expected = await db.ExpectedTransactions.FindAsync(id)
                       ?? throw new KeyNotFoundException("A tétel nem található.");

        if (expected.Status != ExpectedStatus.pending)
            throw new InvalidOperationException("Csak függőben lévő tétel hagyható ki.");

        // Nincs Transaction, nincs egyenleg-változás — csak lezárjuk
        expected.Status = ExpectedStatus.skipped;

        // Ismétlődő esetén szintén generálódik a következő pending
        if (expected.Recurrence != RecurrenceType.once)
        {
            db.ExpectedTransactions.Add(new ExpectedTransaction
            {
                Title = expected.Title,
                Description = expected.Description,
                Amount = expected.Amount,
                Type = expected.Type,
                WalletID = expected.WalletID,
                CategoryID = expected.CategoryID,
                Recurrence = expected.Recurrence,
                RecurrenceDay = expected.RecurrenceDay,
                Status = ExpectedStatus.pending,
                ExpectedDate = CalculateNextDate(expected.ExpectedDate, expected.Recurrence, expected.RecurrenceDay)
            });
        }

        await db.SaveChangesAsync();
    }

    public async Task<MonthlySummaryDto> GetMonthlySummaryAsync()
    {
        await using var db = await factory.CreateDbContextAsync();

        var today = DateTime.Today;
        var monthEnd = new DateTime(today.Year, today.Month,
                        DateTime.DaysInMonth(today.Year, today.Month), 23, 59, 59);

        var items = await db.ExpectedTransactions
            .Where(e => e.Status == ExpectedStatus.pending && e.ExpectedDate <= monthEnd)
            .ToListAsync();

        var nextEvent = await db.ExpectedTransactions
            .Include(e => e.Wallet)
            .Where(e => e.Status == ExpectedStatus.pending && e.ExpectedDate >= today)
            .OrderBy(e => e.ExpectedDate)
            .FirstOrDefaultAsync();

        return new MonthlySummaryDto
        {
            TotalIncome = items.Where(e => e.Type == ExpectedTransactionType.income).Sum(e => e.Amount),
            TotalExpense = items.Where(e => e.Type == ExpectedTransactionType.expense).Sum(e => e.Amount),
            IncomeCount = items.Count(e => e.Type == ExpectedTransactionType.income),
            ExpenseCount = items.Count(e => e.Type == ExpectedTransactionType.expense),
            Count = items.Count,
            NextEvent = nextEvent
        };
    }

    private static DateTime CalculateNextDate(DateTime from, RecurrenceType recurrence, int? day)
    {
        return recurrence switch
        {
            RecurrenceType.daily => from.AddDays(1),
            RecurrenceType.weekly => from.AddDays(7),
            RecurrenceType.monthly => day.HasValue
                ? new DateTime(
                    from.AddMonths(1).Year,
                    from.AddMonths(1).Month,
                    Math.Min(day.Value, DateTime.DaysInMonth(from.AddMonths(1).Year, from.AddMonths(1).Month)))
                : from.AddMonths(1),
            RecurrenceType.yearly => from.AddYears(1),
            _ => from
        };
    }
}
