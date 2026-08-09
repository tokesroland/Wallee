using Microsoft.EntityFrameworkCore;
using Wallee.Models;

namespace Wallee.Data;

public class WalleeDbContext(DbContextOptions<WalleeDbContext> options) : DbContext(options)
{
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<ExpectedTransaction> ExpectedTransactions => Set<ExpectedTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(e =>
        {
            e.Property(t => t.Type).HasConversion<string>();
            e.Property(t => t.Currency).HasMaxLength(3);

            e.HasOne(t => t.Wallet)
             .WithMany(w => w.Transactions)
             .HasForeignKey(t => t.WalletID)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(t => t.Category)
             .WithMany(c => c.Transactions)
             .HasForeignKey(t => t.CategoryID)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ExpectedTransaction>(e =>
        {
            e.Property(t => t.Type).HasConversion<string>();
            e.Property(t => t.Status).HasConversion<string>();
            e.Property(t => t.Recurrence).HasConversion<string>();
            e.Property(t => t.Title).HasMaxLength(100);

            e.HasOne(t => t.Wallet)
             .WithMany()
             .HasForeignKey(t => t.WalletID)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(t => t.Category)
             .WithMany()
             .HasForeignKey(t => t.CategoryID)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(t => t.LinkedTransaction)
             .WithMany()
             .HasForeignKey(t => t.LinkedTransactionID)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Category>()
            .Property(c => c.CategoryName)
            .HasMaxLength(25);

        modelBuilder.Entity<Wallet>()
            .Property(w => w.Name)
            .HasMaxLength(50);
    }
}
