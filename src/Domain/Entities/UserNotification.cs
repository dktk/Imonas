namespace Domain.Entities
{
    public class UserNotification : AuditableEntity
    {
        public string RecipientUserId { get; set; } = string.Empty;
        public string SenderUserId { get; set; } = string.Empty;
        public string SenderDisplayName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? LinkUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
