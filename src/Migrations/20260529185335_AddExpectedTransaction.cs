using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wallee.Migrations;

public partial class AddExpectedTransaction : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Categories",
            columns: table => new
            {
                ID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                CategoryName = table.Column<string>(type: "TEXT", maxLength: 25, nullable: false),
                Active = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Categories", x => x.ID);
            });

        migrationBuilder.CreateTable(
            name: "Wallets",
            columns: table => new
            {
                ID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                Description = table.Column<string>(type: "TEXT", nullable: true),
                Balance = table.Column<int>(type: "INTEGER", nullable: false),
                Active = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Wallets", x => x.ID);
            });

        migrationBuilder.CreateTable(
            name: "Transactions",
            columns: table => new
            {
                ID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Amount = table.Column<int>(type: "INTEGER", nullable: false),
                Type = table.Column<string>(type: "TEXT", nullable: false),
                Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                Description = table.Column<string>(type: "TEXT", nullable: true),
                WalletID = table.Column<int>(type: "INTEGER", nullable: false),
                CategoryID = table.Column<int>(type: "INTEGER", nullable: true),
                Date = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Transactions", x => x.ID);
                table.ForeignKey(
                    name: "FK_Transactions_Categories_CategoryID",
                    column: x => x.CategoryID,
                    principalTable: "Categories",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_Transactions_Wallets_WalletID",
                    column: x => x.WalletID,
                    principalTable: "Wallets",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ExpectedTransactions",
            columns: table => new
            {
                ID = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "TEXT", nullable: true),
                Amount = table.Column<int>(type: "INTEGER", nullable: false),
                Type = table.Column<string>(type: "TEXT", nullable: false),
                WalletID = table.Column<int>(type: "INTEGER", nullable: false),
                CategoryID = table.Column<int>(type: "INTEGER", nullable: true),
                Status = table.Column<string>(type: "TEXT", nullable: false),
                Recurrence = table.Column<string>(type: "TEXT", nullable: false),
                RecurrenceDay = table.Column<int>(type: "INTEGER", nullable: true),
                ExpectedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                LinkedTransactionID = table.Column<int>(type: "INTEGER", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExpectedTransactions", x => x.ID);
                table.ForeignKey(
                    name: "FK_ExpectedTransactions_Categories_CategoryID",
                    column: x => x.CategoryID,
                    principalTable: "Categories",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_ExpectedTransactions_Transactions_LinkedTransactionID",
                    column: x => x.LinkedTransactionID,
                    principalTable: "Transactions",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_ExpectedTransactions_Wallets_WalletID",
                    column: x => x.WalletID,
                    principalTable: "Wallets",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "IX_ExpectedTransactions_CategoryID", table: "ExpectedTransactions", column: "CategoryID");
        migrationBuilder.CreateIndex(name: "IX_ExpectedTransactions_LinkedTransactionID", table: "ExpectedTransactions", column: "LinkedTransactionID");
        migrationBuilder.CreateIndex(name: "IX_ExpectedTransactions_WalletID", table: "ExpectedTransactions", column: "WalletID");
        migrationBuilder.CreateIndex(name: "IX_Transactions_CategoryID", table: "Transactions", column: "CategoryID");
        migrationBuilder.CreateIndex(name: "IX_Transactions_WalletID", table: "Transactions", column: "WalletID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ExpectedTransactions");
        migrationBuilder.DropTable(name: "Transactions");
        migrationBuilder.DropTable(name: "Categories");
        migrationBuilder.DropTable(name: "Wallets");
    }
}
