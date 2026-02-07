using System.ComponentModel.DataAnnotations.Schema;

namespace Imonas.RuleEngine.Models;

[Table("Transaction", Schema = "rules")]
public class Transaction
{
    public long Id { get; set; }          
    public Status Status { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}
