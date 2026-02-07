using Domain.Common;

namespace Domain.Entities.Cases
{
    public class CaseComment : AuditableEntity
    {
        public int CaseId { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string CommentedBy { get; set; } = string.Empty;

        public virtual ExceptionCase Case { get; set; } = null!;
    }
}
