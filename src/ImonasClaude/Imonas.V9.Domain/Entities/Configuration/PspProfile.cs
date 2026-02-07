using Imonas.V9.Domain.Common;

namespace Imonas.V9.Domain.Entities.Configuration;

public class PspProfile : BaseEntity
{
    public string PspName { get; set; } = string.Empty;
    public string PspCode { get; set; } = string.Empty;
    public string FileFormat { get; set; } = string.Empty;
    public string TimeZone { get; set; } = "UTC";
    public string SupportedCurrencies { get; set; } = string.Empty;
    public string? MerchantIds { get; set; }
    public string? SettlementSchedule { get; set; }
    public bool IsActive { get; set; }
    public string? ConfigurationJson { get; set; }
}
