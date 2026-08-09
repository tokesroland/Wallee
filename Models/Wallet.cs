using System.Drawing;

namespace Wallee.Models;

public class Wallet
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Balance { get; set; }
    public bool Active { get; set; } = true;
    public bool IsMain { get; set; } = false;
    public string Icon { get; set; } = "🛒"; 
    public string Color { get; set; } = "#ee6a59";
    public ICollection<Transaction> Transactions { get; set; } = [];
}