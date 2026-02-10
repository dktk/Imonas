namespace Application.Features.UserNotifications.DTOs
{
    public class UserNotificationDto : IMapFrom<UserNotification>
    {
        public int Id { get; set; }
        public string SenderDisplayName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? LinkUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime Created { get; set; }
    }
}
