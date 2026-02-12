using Domain.Entities.Audit;
using Domain.Entities.Cases;
using Domain.Entities.Configuration;
using Domain.Entities.Identity;
using Domain.Entities.Log;
using Domain.Entities.MedalionData.Bronze;
using Domain.Entities.MedalionData.Gold;
using Domain.Entities.MedalionData.Silver;
using Domain.Entities.Rules;
using Domain.Entities.Worflow;

namespace Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<User> AspNetUsers { get; }
        DbSet<Serilog> Serilogs { get; set; }
        DbSet<AuditTrail> AuditTrails { get; set; }
        DbSet<Customer> Customers { get; set; }
        DbSet<DocumentType> DocumentTypes { get; set; }
        DbSet<Document> Documents { get; set; }
        DbSet<KeyValue> KeyValues { get; set; }
        DbSet<ApprovalData> ApprovalDatas { get; set; }
        DbSet<Invoice> Invoices {  get; set; }
        DbSet<InvoiceRawData> InvoiceRawDatas { get; set; }
        DbSet<Psp> Psps { get; set; }
        DbSet<ReconciliationComment> ReconciliationComments { get; set; }

        DbSet<RawPayment> RawPayments { get; set; }
        DbSet<ExternalPayment> ExternalPayments { get; set; }
        DbSet<InternalPayment> InternalPayments { get; set; }
        DbSet<PspSettlement> PspSettlements { get; set; }

        DbSet<InternalSystem> InternalSystems { get; set; }

        DbSet<ReconciliationRun> ReconciliationRuns { get; set; }
        DbSet<RunMetric> RunMetrics { get; set; }

        // Cases
        DbSet<ExceptionCase> ExceptionCases { get; set; }
        DbSet<CaseComment> CaseComments { get; set; }
        DbSet<CaseAttachment> CaseAttachments { get; set; }
        DbSet<CaseLabel> CaseLabels { get; set; }

        // User Notifications
        DbSet<UserNotification> UserNotifications { get; set; }

        // Configuration
        DbSet<StatusMapping> StatusMappings { get; set; }
        DbSet<FieldMapping> FieldMappings { get; set; }
        DbSet<ReconciliationSchedule> ReconciliationSchedules { get; set; }
        DbSet<FeeContract> FeeContracts { get; set; }
        DbSet<PspConfiguration> PspConfigurations { get; set; }

        // Rules
        DbSet<MatchingRule> MatchingRules { get; set; }
        DbSet<Notification> Notifications { get; set; }
        DbSet<Currency> Currencies { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
