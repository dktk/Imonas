using Application.Features.UserNotifications.Commands;
using Application.Features.UserNotifications.Queries;

namespace SmartAdmin.WebUI.EndPoints
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsEndpoint(ISender mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int take = 20)
        {
            var notifications = await mediator.Send(new GetMyNotificationsQuery { Take = take });
            return Ok(notifications);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await mediator.Send(new GetUnreadNotificationCountQuery());
            return Ok(new { count });
        }

        [HttpPost("{id}/mark-read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            await mediator.Send(new MarkNotificationReadCommand { Id = id });
            return Ok();
        }

        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllRead()
        {
            await mediator.Send(new MarkAllNotificationsReadCommand());
            return Ok();
        }
    }
}
