namespace Domain.Entities
{
    public class Psp : AuditableEntity, IAuditTrial
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string Code { get; set; }
        // Data comes via CSV file uploads
        public bool IsCsvBased { get; set; }
        public required InternalSystem InternalSystem { get; set; }
        public int InternalSystemId { get; set; }
    }
}
