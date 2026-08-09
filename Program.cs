using Microsoft.EntityFrameworkCore;
using Wallee.Components;
using Wallee.Data;
using Wallee.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "Wallee",
    "wallee.db");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? $"Data Source={dbPath}";

Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

builder.Services.AddDbContextFactory<WalleeDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IExpectedTransactionService, ExpectedTransactionService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<WalleeDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.EnsureCreatedAsync();
    await EnsureWalletColumnsAsync(db);
    await DbSeeder.SeedAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static async Task EnsureWalletColumnsAsync(WalleeDbContext db)
{
    await db.Database.OpenConnectionAsync();

    var columns = new[]
    {
        (Name: "Color", Definition: "TEXT NOT NULL DEFAULT '#ee6a59'"),
        (Name: "Icon", Definition: "TEXT NOT NULL DEFAULT '🛒'"),
        (Name: "IsMain", Definition: "INTEGER NOT NULL DEFAULT 0")
    };

    foreach (var column in columns)
    {
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = $"SELECT 1 FROM pragma_table_info('Wallets') WHERE name = '{column.Name}' LIMIT 1";

        if (await command.ExecuteScalarAsync() is null)
        {
            command.CommandText = $"ALTER TABLE Wallets ADD COLUMN {column.Name} {column.Definition}";
            await command.ExecuteNonQueryAsync();
        }
    }
}
