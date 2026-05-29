namespace Wallee.Models;

public enum ExpectedTransactionType { income, expense }

public enum RecurrenceType { once, daily, weekly, monthly, yearly }

public enum ExpectedStatus { pending, approved, skipped }

public class ExpectedTransaction
{
    public int ID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Amount { get; set; }                              // mindig pozitív
    public ExpectedTransactionType Type { get; set; }            // income vagy expense
    public int WalletID { get; set; } = 1;                       // default, UI-ban nem választható
    public int? CategoryID { get; set; }
    public ExpectedStatus Status { get; set; } = ExpectedStatus.pending;
    public RecurrenceType Recurrence { get; set; } = RecurrenceType.once;
    public int? RecurrenceDay { get; set; }   // havi esetén: 1-31; heti esetén: 1=hétfő … 7=vasárnap
    public DateTime ExpectedDate { get; set; }
    public int? LinkedTransactionID { get; set; }    // jóváhagyáskor kitöltve
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigációs property-k
    public Wallet Wallet { get; set; } = null!;
    public Category? Category { get; set; }
    public Transaction? LinkedTransaction { get; set; }
}
