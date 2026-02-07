namespace Imonas.RuleEngine.Models;

public enum Status
{
    Successful = 0,
    Pending = 1,
    Failed = 2,
    Expired = 4,
    Error = 8,
    Deleted = 16,
    Cancelled = 32,
    Filtered = 64,
    Declined = 128
}
