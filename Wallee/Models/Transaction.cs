namespace Wallee.Models;

public enum TransactionType { expense, income, transfer }

public class Transaction
{
    public int ID { get; set; }
    public int Amount { get; set; }
    public TransactionType Type { get; set; }
    public string Currency { get; set; } = "HUF";
    public string? Description { get; set; }
    public int WalletID { get; set; }
    public int? CategoryID { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;

    public Wallet Wallet { get; set; } = null!;
    public Category? Category { get; set; }
}