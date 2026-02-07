using Imonas.RuleEngine.Models;

namespace Imonas.RuleEngine.Data;

public static class Seed
{
    public static async Task EnsureSeedAsync(AppDbContext db)
    {
        if (!db.Transactions.Any())
        {
            var now = DateTime.UtcNow;
            db.Transactions.AddRange(new[] {
                new Transaction { Id = 1, Status = Status.Successful, Amount =  50.00m, Date = now.AddDays(-10) },
                new Transaction { Id = 2, Status = Status.Pending,    Amount = 150.25m, Date = now.AddDays(-5)  },
                new Transaction { Id = 3, Status = Status.Successful, Amount = 999.99m, Date = now.AddDays(-1)  },
                new Transaction { Id = 4, Status = Status.Failed,     Amount =  10.00m, Date = now.AddDays(-2)  },
                new Transaction { Id = 5, Status = Status.Successful, Amount =  75.00m, Date = now.AddDays(-30) }
            });
        }

        if (!db.RuleGroups.Any())
        {
            var grp = new RuleGroup
            {
                Name = "High Value Successful",
                GroupOperator = LogicalOperator.And,
                Rules = new List<Rule>
                {
                    new Rule { Field = RuleField.Status, Operator = RuleOperator.Equal, Value = nameof(Status.Successful), Order = 0 },
                    new Rule { Field = RuleField.Amount, Operator = RuleOperator.GreaterThan, Value = "100", Order = 1 }
                }
            };
            db.RuleGroups.Add(grp);
        }

        await db.SaveChangesAsync();
    }
}
