using Application.Common.Interfaces;
using Application.Features.Calendar.Queries;

using Domain.Constants;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace SmartAdmin.WebUI.Pages.Calendar
{
    [Authorize]
    public class IndexModel(
        IStringLocalizer<IndexModel> localizer,
        ISender mediator,
        ICurrentUserService currentUserService,
        IUserService userService,
        IApplicationDbContext dbContext) : PageModel
    {
        public bool IsAdmin { get; set; }
        public string CurrentUserId { get; set; } = string.Empty;
        public List<SelectListItem> PspOptions { get; set; } = new();
        public List<SelectListItem> UserOptions { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? PspFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? UserFilter { get; set; }

        public async Task OnGetAsync()
        {
            CurrentUserId = currentUserService.UserId ?? string.Empty;

            // Check if user is admin
            IsAdmin = !string.IsNullOrEmpty(CurrentUserId) &&
                (await userService.IsInRoleAsync(CurrentUserId, Roles.Admin) ||
                 await userService.IsInRoleAsync(CurrentUserId, Roles.SystemAdmin));

            // Load PSP options
            var psps = await dbContext.Psps
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();

            PspOptions = psps.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToList();
            PspOptions.Insert(0, new SelectListItem { Value = "", Text = localizer["All PSPs"] });

            // Load user options for admin
            if (IsAdmin)
            {
                var users = await userService.GetUsersAsync();
                UserOptions = users.Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.DisplayName ?? u.Email
                }).ToList();
                UserOptions.Insert(0, new SelectListItem { Value = "", Text = localizer["All Users"] });
            }
        }

        public async Task<IActionResult> OnGetEventsAsync(DateTime start, DateTime end, int? pspId, string? userId)
        {
            var currentUserId = currentUserService.UserId ?? string.Empty;

            // Check if user is admin
            var isAdmin = !string.IsNullOrEmpty(currentUserId) &&
                (await userService.IsInRoleAsync(currentUserId, Roles.Admin) ||
                 await userService.IsInRoleAsync(currentUserId, Roles.SystemAdmin));

            // Non-admin users can only see their own events
            string? assignedTo = null;
            if (!isAdmin)
            {
                assignedTo = currentUserId;
            }
            else if (!string.IsNullOrEmpty(userId))
            {
                assignedTo = userId;
            }

            var query = new GetCalendarEventsQuery
            {
                StartDate = start,
                EndDate = end,
                PspId = pspId,
                AssignedTo = assignedTo,
                IncludeCases = true,
                IncludeSchedules = true,
                IncludeRuns = true
            };

            var events = await mediator.Send(query);

            // Convert to FullCalendar format
            var fullCalendarEvents = events.Select(e => new
            {
                id = e.Id,
                title = e.Title,
                start = e.Start.ToString("o"),
                end = e.End?.ToString("o"),
                color = e.Color,
                url = e.Url,
                allDay = e.AllDay,
                extendedProps = new
                {
                    eventType = e.EventType.ToString(),
                    description = e.Description,
                    assignedTo = e.AssignedTo,
                    assignedToName = e.AssignedToName,
                    caseId = e.CaseId,
                    caseNumber = e.CaseNumber,
                    caseStatus = e.CaseStatus?.ToString(),
                    caseSeverity = e.CaseSeverity?.ToString(),
                    scheduleId = e.ScheduleId,
                    pspId = e.PspId,
                    pspName = e.PspName,
                    recurrenceType = e.RecurrenceType?.ToString()
                }
            });

            return new CamelCaseJsonResult(fullCalendarEvents);
        }
    }
}
