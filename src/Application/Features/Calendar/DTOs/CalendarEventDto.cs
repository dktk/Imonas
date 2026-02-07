namespace Application.Features.Calendar.DTOs
{
    public class CalendarEventDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public string? Description { get; set; }
        public CalendarEventType EventType { get; set; }
        public string Color { get; set; } = string.Empty;
        public string? Url { get; set; }
        public bool AllDay { get; set; }
        public string? AssignedTo { get; set; }
        public string? AssignedToName { get; set; }

        // For case events
        public int? CaseId { get; set; }
        public string? CaseNumber { get; set; }
        public CaseStatus? CaseStatus { get; set; }
        public CaseSeverity? CaseSeverity { get; set; }

        // For schedule events
        public int? ScheduleId { get; set; }
        public int? PspId { get; set; }
        public string? PspName { get; set; }
        public RecurrenceType? RecurrenceType { get; set; }
    }

    public enum CalendarEventType
    {
        CaseDueDate = 1,
        ReconciliationSchedule = 2,
        ReconciliationRun = 3
    }

    public class CalendarFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IncludeCases { get; set; } = true;
        public bool IncludeSchedules { get; set; } = true;
        public bool IncludeRuns { get; set; } = true;
        public string? AssignedTo { get; set; }
        public int? PspId { get; set; }
    }

    public class ReconciliationScheduleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PspId { get; set; }
        public string PspName { get; set; } = string.Empty;
        public RecurrenceType RecurrenceType { get; set; }
        public int RecurrenceInterval { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
        public int? DayOfMonth { get; set; }
        public TimeSpan ScheduledTime { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? NextRunDate { get; set; }
        public DateTime? LastRunDate { get; set; }
        public bool IsActive { get; set; }
        public string? AssignedTo { get; set; }
        public string? AssignedToName { get; set; }
        public string? Description { get; set; }
    }
}
