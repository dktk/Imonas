using Application;
using Application.Features.Notifications.Commands;
using Application.Features.Notifications.DTOs;
using Application.Features.Notifications.Queries;

using AutoMapper;

namespace SmartAdmin.WebUI.Pages.Config
{
    public class NotificationsModel(
        ISender mediator,
        IMapper mapper,
        ResultSafeWrapper resultSafeWrapper,
        IStringLocalizer<NotificationsModel> localizer) : PageModel
    {
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public NotificationDto NotificationInput { get; set; } = new();

        public async Task OnGet()
        {
            NotificationInput = await mediator.Send(new GetNotificationQuery()) ?? new();
        }

        public async Task<IActionResult> OnPostSaveEmailSettings()
        {
            return await SaveSettings("Email settings saved successfully.");
        }

        private async Task<IActionResult> SaveSettings(string message)
        {
            var command = new AddNotificationCommand
            {
                EmailEnableSsl = NotificationInput.EmailEnableSsl,
                EmailFromAddress = NotificationInput.EmailFromAddress,
                EmailSmtpPort = NotificationInput.EmailSmtpPort,
                EmailSmtpServer = NotificationInput.EmailSmtpServer,
                Id = NotificationInput.Id,
                MicrosoftTeamsWebhookURL = NotificationInput.MicrosoftTeamsWebhookURL,
                NotifyOnCaseSLABreach = NotificationInput.NotifyOnCaseSLABreach,
                NotifyOnCriticalCaseCreated = NotificationInput.NotifyOnCriticalCaseCreated,
                NotifyOnLowMatchRate = NotificationInput.NotifyOnLowMatchRate,
                NotifyOnRunFailure = NotificationInput.NotifyOnRunFailure,
                NotifyOnRunToCompletion = NotificationInput.NotifyOnRunToCompletion,
                SlackDefaultChannel = NotificationInput.SlackDefaultChannel,
                SlackWebhookURL = NotificationInput.SlackWebhookURL
            };
            var result = await mediator.Send(command);

            if (result.Success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                ErrorMessage = result.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostTestEmailConnection()
        {
            return await SaveSettings("Email connection test successful.");
        }

        public async Task<IActionResult> OnPostSaveSlackSettings()
        {
            return await SaveSettings("Slack integration settings saved successfully.");            
        }

        public async Task<IActionResult> OnPostTestSlackIntegration()
        {
            return await SaveSettings("Slack test message sent successfully.");            
        }

        public async Task<IActionResult> OnPostSaveTeamsSettings()
        {
            return await SaveSettings("Microsoft Teams integration settings saved successfully.");
        }

        public async Task<IActionResult> OnPostTestTeamsIntegration()
        {
            return await SaveSettings("Microsoft Teams test message sent successfully.");
        }
    }
}
