using Domain.Entities.Identity;

namespace Domain.Common
{
    public interface IEntity
    {
        int Id { get; set; }
    }

    public abstract class AuditableEntity : IEntity
    {
        public int Id { get; set; }

        public DateTime Created { get; set; }

        public string UserId { get; set; }

        public DateTime? LastModified { get; set; }

        public string LastModifiedBy { get; set; }
        public User User { get; set; }
    }

    public interface ISoftDelete
    {
        DateTime? Deleted { get; set; }
        string DeletedBy { get; set; }
    }

    public abstract class AuditableSoftDeleteEntity : AuditableEntity, ISoftDelete
    {
        public DateTime? Deleted { get; set; }
        public string DeletedBy { get; set; }
    }
}
