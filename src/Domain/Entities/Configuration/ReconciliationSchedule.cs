using Domain.Enums;

namespace Domain.Entities.Configuration
{
    public class ReconciliationSchedule : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public int PspId { get; set; }
        public virtual Psp Psp { get; set; } = null!;

        public RecurrenceType RecurrenceType { get; set; }
        public int RecurrenceInterval { get; set; } = 1;
        public DayOfWeek? DayOfWeek { get; set; }
        public int? DayOfMonth { get; set; }
        public TimeSpan ScheduledTime { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? NextRunDate { get; set; }
        public DateTime? LastRunDate { get; set; }

        public bool IsActive { get; set; } = true;
        public string? AssignedTo { get; set; }
        public string? Description { get; set; }
    }
}
