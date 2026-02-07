namespace Imonas.RuleEngine.Models;

public enum RuleField
{
    Id = 0,
    Status = 1,
    Amount = 2,
    Date = 3
}

public enum RuleOperator
{
    Equal = 0,
    LessThan = 1,
    GreaterThan = 2
}

public enum LogicalOperator
{
    And = 0,
    Or = 1
}
