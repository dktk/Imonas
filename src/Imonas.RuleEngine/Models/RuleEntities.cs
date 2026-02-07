using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imonas.RuleEngine.Models;

[Table("RuleGroup", Schema = "rules")]
public class RuleGroup
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public LogicalOperator GroupOperator { get; set; } = LogicalOperator.And;

    public int? ParentGroupId { get; set; }
    public RuleGroup? ParentGroup { get; set; }
    public List<RuleGroup> Children { get; set; } = new();
    public List<Rule> Rules { get; set; } = new();

    public int Order { get; set; }

    public bool IsDeleted { get; set; }
    public int Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}

[Table("Rule", Schema = "rules")]
public class Rule
{
    public int Id { get; set; }

    [Required]
    public int RuleGroupId { get; set; }

    [ForeignKey(nameof(RuleGroupId))]
    public RuleGroup? RuleGroup { get; set; }

    [Required]
    public RuleField Field { get; set; }

    [Required]
    public RuleOperator Operator { get; set; }

    [Required, MaxLength(200)]
    public string Value { get; set; } = string.Empty;

    public int Order { get; set; }
}

[Table("AuditEntry", Schema = "rules")]
public class AuditEntry
{
    public long Id { get; set; }
    [MaxLength(100)] public string EntityName { get; set; } = string.Empty;
    [MaxLength(100)] public string Action { get; set; } = string.Empty;
    public string ChangesJson { get; set; } = "{}";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    [MaxLength(100)] public string User { get; set; } = "system";
}
