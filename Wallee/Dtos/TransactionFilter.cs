using Wallee.Models;

namespace Wallee.Dtos;

public class TransactionFilter
{
    public int? WalletID { get; set; }    // null = összes tárca
    public int? CategoryID { get; set; }  // null = összes kategória
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? MaxAmount { get; set; }
    public TransactionType? Type { get; set; }
}