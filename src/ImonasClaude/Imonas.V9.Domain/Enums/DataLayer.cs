namespace Imonas.V9.Domain.Enums;

public enum DataLayer
{
    Bronze = 1,
    Silver = 2,
    Gold = 3
}

public enum FileStatus
{
    Pending = 1,
    Processing = 2,
    Processed = 3,
    Failed = 4,
    Rejected = 5
}

public enum RunStatus
{
    Pending = 1,
    Running = 2,
    Completed = 3,
    Failed = 4,
    PartialSuccess = 5
}

public enum RuleType
{
    Equality = 1,
    Composite = 2,
    ToleranceWindow = 3,
    Fuzzy = 4
}

public enum MatchStatus
{
    Unmatched = 1,
    Matched = 2,
    PartialMatch = 3,
    ManualMatch = 4
}

public enum CaseSeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum CaseStatus
{
    Open = 1,
    InProgress = 2,
    PendingReview = 3,
    Resolved = 4,
    Closed = 5
}

public enum VarianceType
{
    Amount = 1,
    Date = 2,
    Currency = 3,
    BankFee = 4,
    Other = 5
}

public enum UserRole
{
    Admin = 1,
    Analyst = 2,
    Viewer = 3,
    FinanceAdmin = 4,
    ConfigManager = 5
}
