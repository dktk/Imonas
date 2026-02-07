namespace Application.Features.Schedules.DTOs
{
    public class ScheduleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PspId { get; set; }
        public string PspName { get; set; } = string.Empty;
        public RecurrenceType RecurrenceType { get; set; }
        public string RecurrenceTypeDisplay => RecurrenceType.ToString();
        public int RecurrenceInterval { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
        public string? DayOfWeekDisplay => DayOfWeek?.ToString();
        public int? DayOfMonth { get; set; }
        public TimeSpan ScheduledTime { get; set; }
        public string ScheduledTimeDisplay => ScheduledTime.ToString(@"hh\:mm");
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? NextRunDate { get; set; }
        public DateTime? LastRunDate { get; set; }
        public bool IsActive { get; set; }
        public string? AssignedTo { get; set; }
        public string? AssignedToName { get; set; }
        public string? Description { get; set; }
        public DateTime Created { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class ScheduleDetailsDto : ScheduleDto
    {
        public string? PspCode { get; set; }
        public int TotalRunsExecuted { get; set; }
        public int SuccessfulRuns { get; set; }
        public int FailedRuns { get; set; }
    }

    public class ScheduleStatsDto
    {
        public int TotalSchedules { get; set; }
        public int ActiveSchedules { get; set; }
        public int InactiveSchedules { get; set; }
        public int SchedulesDueToday { get; set; }
        public int SchedulesDueThisWeek { get; set; }
    }
}
