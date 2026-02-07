using Microsoft.EntityFrameworkCore;
using Imonas.V9.Domain.Entities.Bronze;
using Imonas.V9.Domain.Entities.Silver;
using Imonas.V9.Domain.Entities.Gold;
using Imonas.V9.Domain.Entities.Reconciliation;
using Imonas.V9.Domain.Entities.Rules;
using Imonas.V9.Domain.Entities.Cases;
using Imonas.V9.Domain.Entities.Configuration;
using Imonas.V9.Domain.Entities.Finance;
using Imonas.V9.Domain.Entities.Auth;
using Imonas.V9.Domain.Entities.Audit;

namespace Imonas.V9.Infrastructure.Data;

public class ImonasDbContext : DbContext
{
    public ImonasDbContext(DbContextOptions<ImonasDbContext> options) : base(options)
    {
    }

    // Bronze Layer
    public DbSet<BronzeFile> BronzeFiles { get; set; }
    public DbSet<BronzeRecord> BronzeRecords { get; set; }

    // Silver Layer
    public DbSet<SilverTransaction> SilverTransactions { get; set; }

    // Gold Layer
    public DbSet<GoldReconciliation> GoldReconciliations { get; set; }

    // Reconciliation
    public DbSet<ReconciliationRun> ReconciliationRuns { get; set; }
    public DbSet<RunMetric> RunMetrics { get; set; }

    // Rules
    public DbSet<MatchingRule> MatchingRules { get; set; }

    // Cases
    public DbSet<Case> Cases { get; set; }
    public DbSet<CaseComment> CaseComments { get; set; }
    public DbSet<CaseAttachment> CaseAttachments { get; set; }
    public DbSet<CaseLabel> CaseLabels { get; set; }

    // Configuration
    public DbSet<PspProfile> PspProfiles { get; set; }
    public DbSet<FieldMapping> FieldMappings { get; set; }
    public DbSet<StatusMapping> StatusMappings { get; set; }

    // Finance
    public DbSet<FeeContract> FeeContracts { get; set; }
    public DbSet<Settlement> Settlements { get; set; }

    // Auth
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }

    // Audit
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ImonasDbContext).Assembly);

        // Table naming conventions
        modelBuilder.Entity<BronzeFile>().ToTable("bronze_files");
        modelBuilder.Entity<BronzeRecord>().ToTable("bronze_records");
        modelBuilder.Entity<SilverTransaction>().ToTable("silver_transactions");
        modelBuilder.Entity<GoldReconciliation>().ToTable("gold_reconciliations");
        modelBuilder.Entity<ReconciliationRun>().ToTable("reconciliation_runs");
        modelBuilder.Entity<RunMetric>().ToTable("run_metrics");
        modelBuilder.Entity<MatchingRule>().ToTable("matching_rules");
        modelBuilder.Entity<Case>().ToTable("cases");
        modelBuilder.Entity<CaseComment>().ToTable("case_comments");
        modelBuilder.Entity<CaseAttachment>().ToTable("case_attachments");
        modelBuilder.Entity<CaseLabel>().ToTable("case_labels");
        modelBuilder.Entity<PspProfile>().ToTable("psp_profiles");
        modelBuilder.Entity<FieldMapping>().ToTable("field_mappings");
        modelBuilder.Entity<StatusMapping>().ToTable("status_mappings");
        modelBuilder.Entity<FeeContract>().ToTable("fee_contracts");
        modelBuilder.Entity<Settlement>().ToTable("settlements");
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<UserRole>().ToTable("user_roles");
        modelBuilder.Entity<UserPermission>().ToTable("user_permissions");
        modelBuilder.Entity<AuditLog>().ToTable("audit_logs");

        // Indexes for performance
        modelBuilder.Entity<BronzeFile>()
            .HasIndex(b => b.FileHash);

        modelBuilder.Entity<SilverTransaction>()
            .HasIndex(s => new { s.TransactionId, s.RunId });

        modelBuilder.Entity<GoldReconciliation>()
            .HasIndex(g => new { g.RunId, g.MatchStatus });

        modelBuilder.Entity<Case>()
            .HasIndex(c => new { c.Status, c.AssignedTo });

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Domain.Common.BaseEntity entity)
            {
                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
