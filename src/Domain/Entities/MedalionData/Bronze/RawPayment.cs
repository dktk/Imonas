namespace Domain.Entities.MedalionData.Bronze;

/// <summary>
/// Bronze Medallion.
/// </summary>
public class RawPayment : AuditableEntity
{
    public required long FileSizeBytes { get; set; }
    public required string FileName { get; set; }

    public FileStatus Status { get; set; }

    public required byte[] RawContent { get; set; } = default!;
    public required string FileHash { get; set; }

    public string? ErrorMessage { get; set; }
    public string? RejectionReason { get; set; }

    public Psp Psp { get; set; }
    public required int PspId { get; set; }    
}
