using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Domain
{
    public enum Status
    {
        [Display(Description ="Successful")]
        Successful = 0,
        [Display(Description = "Pending")]
        Pending = 1,
        [Display(Description = "Failed")]
        Failed = 2,
        [Display(Description = "Expired")]
        Expired = 4,
        [Display(Description = "Error")]
        Error = 8,
        [Display(Description = "Deleted")]
        Deleted = 16,
        [Display(Description = "Cancelled")]
        Cancelled = 32,
        [Display(Description = "Filtered")]
        Filtered = 64,
        [Display(Description = "Declined")]
        Declined = 128,
    }

    [DebuggerDisplay("{TxId}")]
    public class TargetData
    {
        public required string Id { get; set; }
        public required string TxId { get; set; }
        public required DateTime Date { get; set; }
        public required decimal Amount { get; set; }
        public required string Currency { get; set; }
        public required string TxStatus { get; set; }
        public required string Psp { get; set; }
        public required string Email { get; set; }
        public required int ClientId { get; set; }
        public required string Description { get; set; }
        public required string ReferenceCode { get; set; }

        public PaymentAction Action { get; set; }
        public string PaymentMethod { get; set; }
        public string Merchant { get; set; }
    }
}
