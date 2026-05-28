namespace Wallee.Models;

public class Category
{
    public int ID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool Active { get; set; } = true;

    // navigációs property — ezen keresztül érhető el egy tárca tranzakciói anélkül, hogy JOIN-t kellene írni
    public ICollection<Transaction> Transactions { get; set; } = [];
}