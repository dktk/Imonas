namespace Domain.Entities
{
    public class RunMetric : AuditableEntity
    {
        public int RunId { get; set; }
        public string MetricName { get; set; } = string.Empty;
        public string MetricValue { get; set; } = string.Empty;
        public string? MetricCategory { get; set; }
        public virtual ReconciliationRun Run { get; set; } = null!;
    }
}
