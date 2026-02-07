using System.ComponentModel.DataAnnotations;

using Application.Common.Interfaces;
using Application.Features.Schedules.Commands;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace SmartAdmin.WebUI.Pages.Schedules
{
    public class CreateModel(
        IStringLocalizer<CreateModel> localizer,
        ISender mediator,
        IApplicationDbContext dbContext) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public List<SelectListItem> PspOptions { get; set; } = new();
        public List<SelectListItem> RecurrenceTypeOptions { get; set; } = new();
        public List<SelectListItem> DayOfWeekOptions { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Name is required")]
            [StringLength(255)]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "PSP is required")]
            public int PspId { get; set; }

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

        public async Task OnGetAsync()
        {
            await LoadOptionsAsync();
            Input.StartDate = DateTime.Today;
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
                var command = new CreateScheduleCommand
                {
                    Name = Input.Name,
                    PspId = Input.PspId,
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
                    TempData["SuccessMessage"] = $"Schedule '{Input.Name}' created successfully.";
                    return RedirectToPage("Index");
                }

                ErrorMessage = result.Message ?? "Failed to create schedule.";
                await LoadOptionsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to create schedule: {ex.Message}";
                await LoadOptionsAsync();
                return Page();
            }
        }

        private async Task LoadOptionsAsync()
        {
            var psps = await dbContext.Psps
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();

            PspOptions = psps.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToList();
            PspOptions.Insert(0, new SelectListItem { Value = "", Text = localizer["-- Select PSP --"] });

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
        }
    }
}
