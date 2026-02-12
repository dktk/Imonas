namespace Domain.Entities.Configuration
{
    public class PspConfiguration : AuditableEntity
    {
        public int PspId { get; set; }
        public string ConfigJson { get; set; } = "{}";
        public virtual Psp Psp { get; set; } = null!;
    }
}
