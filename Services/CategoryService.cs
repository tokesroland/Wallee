using Microsoft.EntityFrameworkCore;
using Wallee.Data;
using Wallee.Models;

namespace Wallee.Services;

public class CategoryService(IDbContextFactory<WalleeDbContext> factory) : ICategoryService
{
    public async Task<List<Category>> GetActiveAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Categories
            .Where(c => c.Active)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();
    }
}