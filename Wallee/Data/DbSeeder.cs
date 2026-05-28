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
                new Wallet { Name = "Napi kiadások", Description = "Mindennapi költések", Balance = 0 },
                new Wallet { Name = "Utazás", Description = "Nyaralás", Balance = 0 },
                new Wallet { Name = "Megtakarítás", Description = "Hosszútávú cél", Balance = 0 }
            );
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