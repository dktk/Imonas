using Application.Features.Schedules.Commands;
using Application.Features.Schedules.Queries;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace SmartAdmin.WebUI.Pages.Schedules
{
    public class EditModel(
        IStringLocalizer<EditModel> localizer,
        ISender mediator) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public List<SelectListItem> RecurrenceTypeOptions { get; set; } = new();
        public List<SelectListItem> DayOfWeekOptions { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public string PspName { get; set; } = string.Empty;

        public class InputModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Name is required")]
            [StringLength(255)]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Recurrence type is required")]
            public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.Daily;

            public int RecurrenceInterval { get; set; } = 1;

            public DayOfWeek? DayOfWeek { get; set; }

            [Range(1, 31, ErrorMessage = "Day of month must be between 1 and 31")]
            public int? DayOfMonth { get; set; }

            [Required(ErrorMessage = "Scheduled time is required")]
            public TimeSpan ScheduledTime { get; set; } = new TimeSpan(9, 0, 0);

            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            public bool IsActive { get; set; } = true;

            [StringLength(1000)]
            public string? Description { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var schedule = await mediator.Send(new GetScheduleByIdQuery { Id = id });

            if (schedule == null)
            {
                return NotFound();
            }

            ScheduleName = schedule.Name;
            PspName = schedule.PspName;
            Input = new InputModel
            {
                Id = schedule.Id,
                Name = schedule.Name,
                RecurrenceType = schedule.RecurrenceType,
                RecurrenceInterval = schedule.RecurrenceInterval,
                DayOfWeek = schedule.DayOfWeek,
                DayOfMonth = schedule.DayOfMonth,
                ScheduledTime = schedule.ScheduledTime,
                StartDate = schedule.StartDate,
                EndDate = schedule.EndDate,
                IsActive = schedule.IsActive,
                Description = schedule.Description
            };

            await LoadOptionsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadOptionsAsync();
                return Page();
            }

            // Validate recurrence-specific fields
            if ((Input.RecurrenceType == RecurrenceType.Weekly || Input.RecurrenceType == RecurrenceType.BiWeekly)
                && !Input.DayOfWeek.HasValue)
            {
                ModelState.AddModelError("Input.DayOfWeek", "Day of week is required for weekly schedules.");
                await LoadOptionsAsync();
                return Page();
            }

            if ((Input.RecurrenceType == RecurrenceType.Monthly || Input.RecurrenceType == RecurrenceType.Quarterly)
                && !Input.DayOfMonth.HasValue)
            {
                ModelState.AddModelError("Input.DayOfMonth", "Day of month is required for monthly/quarterly schedules.");
                await LoadOptionsAsync();
                return Page();
            }

            try
            {
                var command = new UpdateScheduleCommand
                {
                    Id = Input.Id,
                    Name = Input.Name,
                    RecurrenceType = Input.RecurrenceType,
                    RecurrenceInterval = Input.RecurrenceInterval,
                    DayOfWeek = Input.DayOfWeek,
                    DayOfMonth = Input.DayOfMonth,
                    ScheduledTime = Input.ScheduledTime,
                    StartDate = Input.StartDate,
                    EndDate = Input.EndDate,
                    IsActive = Input.IsActive,
                    Description = Input.Description
                };

                var result = await mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = $"Schedule '{Input.Name}' updated successfully.";
                    return RedirectToPage("Index");
                }

                ErrorMessage = result.Message ?? "Failed to update schedule.";
                await LoadOptionsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to update schedule: {ex.Message}";
                await LoadOptionsAsync();
                return Page();
            }
        }

        private Task LoadOptionsAsync()
        {
            RecurrenceTypeOptions = Enum.GetValues<RecurrenceType>()
                .Select(r => new SelectListItem
                {
                    Value = ((int)r).ToString(),
                    Text = r.ToString()
                }).ToList();

            DayOfWeekOptions = Enum.GetValues<DayOfWeek>()
                .Select(d => new SelectListItem
                {
                    Value = ((int)d).ToString(),
                    Text = d.ToString()
                }).ToList();

            return Task.CompletedTask;
        }
    }
}
