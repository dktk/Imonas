using Imonas.V9.Domain.Common;

namespace Imonas.V9.Domain.Entities.Reconciliation;

public class RunMetric : BaseEntity
{
    public Guid RunId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public string MetricValue { get; set; } = string.Empty;
    public string? MetricCategory { get; set; }

    public virtual ReconciliationRun Run { get; set; } = null!;
}
