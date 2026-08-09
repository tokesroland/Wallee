using Microsoft.EntityFrameworkCore;
using Wallee.Models;

namespace Wallee.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(WalleeDbContext db)
    {
        if (!await db.Wallets.AnyAsync())
        {
            db.Wallets.AddRange(
                new Wallet {ID = 1, Name = "Fő számla", Description = "Alapértelmezett", Balance = 0, IsMain = true }
            );
        }

        if (!await db.Wallets.AnyAsync(w => w.Active && w.IsMain))
        {
            var firstWallet = await db.Wallets.Where(w => w.Active).OrderBy(w => w.ID).FirstOrDefaultAsync();
            if (firstWallet is not null) firstWallet.IsMain = true;
        }

        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(
                new Category { CategoryName = "Élelmiszer" },
                new Category { CategoryName = "Lakhatás" },
                new Category { CategoryName = "Közlekedés" },
                new Category { CategoryName = "Szórakozás" },
                new Category { CategoryName = "Egészség" },
                new Category { CategoryName = "Fizetés" },
                new Category { CategoryName = "Egyéb" }
            );
        }

        await db.SaveChangesAsync();
    }
}