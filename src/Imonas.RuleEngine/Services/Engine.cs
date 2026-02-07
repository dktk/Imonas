using System.Linq.Expressions;
using Imonas.RuleEngine.Models;

namespace Imonas.RuleEngine.Services;

public static class Engine
{
    public static Expression<Func<Transaction, bool>> BuildPredicate(RuleGroup group)
    {
        var param = Expression.Parameter(typeof(Transaction), "t");
        var expr = BuildGroupExpression(group, param);
        return Expression.Lambda<Func<Transaction, bool>>(expr ?? Expression.Constant(true), param);
    }

    private static Expression? BuildGroupExpression(RuleGroup group, ParameterExpression param)
    {
        var childExprs = new List<Expression>();

        foreach (var r in group.Rules.OrderBy(r => r.Order))
        {
            var (member, constant) = BuildMemberAndConstant(r, param);
            var exp = r.Operator switch
            {
                RuleOperator.Equal       => Expression.Equal(member, constant),
                RuleOperator.LessThan    => Expression.LessThan(member, constant),
                RuleOperator.GreaterThan => Expression.GreaterThan(member, constant),
                _ => throw new NotSupportedException($"Unsupported operator: {r.Operator}")
            };
            childExprs.Add(exp);
        }

        foreach (var c in group.Children.OrderBy(c => c.Order))
        {
            var cexpr = BuildGroupExpression(c, param);
            if (cexpr is not null) childExprs.Add(cexpr);
        }

        if (childExprs.Count == 0) return null;

        Expression combined = childExprs[0];
        for (int i = 1; i < childExprs.Count; i++)
        {
            combined = group.GroupOperator == LogicalOperator.And
                ? Expression.AndAlso(combined, childExprs[i])
                : Expression.OrElse(combined, childExprs[i]);
        }
        return combined;
    }

    private static (Expression member, Expression constant) BuildMemberAndConstant(Rule rule, ParameterExpression param)
    {
        return rule.Field switch
        {
            RuleField.Id => (
                member: Expression.Property(param, nameof(Transaction.Id)),
                constant: Expression.Constant(long.Parse(rule.Value))
            ),
            RuleField.Status => (
                member: Expression.Property(param, nameof(Transaction.Status)),
                constant: Expression.Constant(Enum.Parse<Status>(rule.Value))
            ),
            RuleField.Amount => (
                member: Expression.Property(param, nameof(Transaction.Amount)),
                constant: Expression.Constant(decimal.Parse(rule.Value))
            ),
            RuleField.Date => (
                member: Expression.Property(param, nameof(Transaction.Date)),
                constant: Expression.Constant(DateTime.Parse(rule.Value))
            ),
            _ => throw new NotSupportedException($"Unsupported field: {rule.Field}")
        };
    }

    public static IEnumerable<string> ValidateRulesRecursive(RuleGroup group)
    {
        var errors = new List<string>();

        foreach (var r in group.Rules)
        {
            if (r.Field == RuleField.Status && r.Operator != RuleOperator.Equal)
                errors.Add($"Status supports only 'Equal' operator.");
            try
            {
                // todo: fix this
                //_ =  switch 
                //{
                //    RuleField.Id => long.Parse(r.Value),
                //    RuleField.Amount => decimal.Parse(r.Value),
                //    RuleField.Date => DateTime.Parse(r.Value),
                //    RuleField.Status => Enum.Parse<Status>(r.Value),
                //    _ => null
                //};
            }
            catch
            {
                errors.Add($"Invalid value '{r.Value}' for field {r.Field}.");
            }
        }

        foreach (var c in group.Children)
            errors.AddRange(ValidateRulesRecursive(c));

        return errors;
    }
}
