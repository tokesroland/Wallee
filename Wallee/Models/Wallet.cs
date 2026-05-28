namespace Wallee.Models;

public class Wallet
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Balance { get; set; }
    public bool Active { get; set; } = true;

    public ICollection<Transaction> Transactions { get; set; } = [];
}