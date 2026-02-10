using System.Reflection;

using Application.Common.Interfaces;

using Domain.Entities.Audit;
using Domain.Entities.Cases;
using Domain.Entities.Configuration;
using Domain.Entities.Identity;
using Domain.Entities.MedalionData.Bronze;
using Domain.Entities.MedalionData.Gold;
using Domain.Entities.MedalionData.Silver;
using Domain.Entities.Rules;
using Domain.Entities.Worflow;

using Infrastructure.Persistence.Extensions;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<
        ApplicationUser, ApplicationRole, string,
        ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin,
        ApplicationRoleClaim, ApplicationUserToken>, IApplicationDbContext
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTime;
        private readonly IConfiguration _configuration;
        private readonly IDomainEventService _domainEventService;

        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(
            IConfiguration configuration,
            DbContextOptions<ApplicationDbContext> options,
            ICurrentUserService currentUserService,
            IDomainEventService domainEventService,
            IDateTime dateTime) : base(options)
        {
            _currentUserService = currentUserService;
            _domainEventService = domainEventService;
            _dateTime = dateTime;
            _configuration = configuration;
        }

        public DbSet<Domain.Entities.Log.Serilog> Serilogs { get; set; }
        public DbSet<AuditTrail> AuditTrails { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<Document> Documents { get; set; }

        public DbSet<KeyValue> KeyValues { get; set; }
        public DbSet<ApprovalData> ApprovalDatas { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceRawData> InvoiceRawDatas { get; set; }
        public DbSet<Psp> Psps { get; set; }
        public DbSet<ReconciliationComment> ReconciliationComments { get; set; }

        public DbSet<RawPayment> RawPayments { get; set; }
        public DbSet<ExternalPayment> ExternalPayments { get; set; }
        public DbSet<InternalPayment> InternalPayments { get; set; }

        public DbSet<PspSettlement> PspSettlements { get; set; }
        public DbSet<InternalSystem> InternalSystems { get; set; }

        public DbSet<ReconciliationRun> ReconciliationRuns { get; set; }
        public DbSet<RunMetric> RunMetrics { get; set; }

        // Cases
        public DbSet<ExceptionCase> ExceptionCases { get; set; }
        public DbSet<CaseComment> CaseComments { get; set; }
        public DbSet<CaseAttachment> CaseAttachments { get; set; }
        public DbSet<CaseLabel> CaseLabels { get; set; }

        // Configuration
        public DbSet<StatusMapping> StatusMappings { get; set; }
        public DbSet<FieldMapping> FieldMappings { get; set; }
        public DbSet<ReconciliationSchedule> ReconciliationSchedules { get; set; }
        public DbSet<FeeContract> FeeContracts { get; set; }

        // Rules
        public DbSet<MatchingRule> MatchingRules { get; set; }

        // Notifications
        public DbSet<Notification> Notifications { get; set; }

        // User Notifications (in-app)
        public DbSet<UserNotification> UserNotifications { get; set; }

        public DbSet<Currency> Currencies { get; set; }

        // Identity Users (Domain entity mapped to AspNetUsers table)
        public DbSet<User> AspNetUsers { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var auditEntries = OnBeforeSaveChanges(_currentUserService.UserId);

            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.UserId = _currentUserService.UserId;
                        entry.Entity.Created = _dateTime.Now;
                        break;

                    case EntityState.Modified:
                        entry.Entity.LastModifiedBy = _currentUserService.UserId;
                        entry.Entity.LastModified = _dateTime.Now;
                        break;
                    case EntityState.Deleted:
                        if (entry.Entity is ISoftDelete softDelete)
                        {
                            softDelete.DeletedBy = _currentUserService.UserId;
                            softDelete.Deleted = _dateTime.Now;
                            entry.State = EntityState.Modified;
                        }
                        break;
                }
            }

            var events = ChangeTracker.Entries<IHasDomainEvent>()
                    .Select(x => x.Entity.DomainEvents)
                    .SelectMany(x => x)
                    .Where(domainEvent => !domainEvent.IsPublished)
                    .ToArray();

            var result = await base.SaveChangesAsync(cancellationToken);
            await DispatchEvents(events);
            await OnAfterSaveChanges(auditEntries, cancellationToken);
            return result;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseNpgsql(_configuration.GetConnectionString("DefaultConnection"))
                    .UseSnakeCaseNamingConvention();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Psp>()
                                .HasKey(c => c.Id);

            ConfigureBronze(builder);
            ConfigureSilver(builder);
            ConfigureGold(builder);
            ConfigureIdentity(builder);

            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            builder.ApplyGlobalFilters<ISoftDelete>(s => s.Deleted == null);
        }

        private static void ConfigureIdentity(ModelBuilder modelBuilder)
        {
            // Map Domain User entity to AspNetUsers table as a read-only query type
            // Using ToView() allows reading from the table without conflicting with ApplicationUser
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToView("AspNetUsers");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserName).HasColumnName("user_name");
                entity.Property(e => e.NormalizedUserName).HasColumnName("normalized_user_name");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.NormalizedEmail).HasColumnName("normalized_email");
                entity.Property(e => e.EmailConfirmed).HasColumnName("email_confirmed");
                entity.Property(e => e.PhoneNumber).HasColumnName("phone_number");
                entity.Property(e => e.PhoneNumberConfirmed).HasColumnName("phone_number_confirmed");
                entity.Property(e => e.TwoFactorEnabled).HasColumnName("two_factor_enabled");
                entity.Property(e => e.LockoutEnd).HasColumnName("lockout_end");
                entity.Property(e => e.LockoutEnabled).HasColumnName("lockout_enabled");
                entity.Property(e => e.AccessFailedCount).HasColumnName("access_failed_count");
                entity.Property(e => e.DisplayName).HasColumnName("display_name");
                entity.Property(e => e.Site).HasColumnName("site");
                entity.Property(e => e.ProfilePictureDataUrl).HasColumnName("profile_picture_data_url");
                entity.Property(e => e.IsActive).HasColumnName("is_active");
                entity.Property(e => e.IsLive).HasColumnName("is_live");
            });
        }

        private async Task DispatchEvents(DomainEvent[] events)
        {
            foreach (var @event in events)
            {
                @event.IsPublished = true;
                await _domainEventService.Publish(@event);
            }
        }

        private static void ConfigureBronze(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RawPayment>(entity =>
            {
                entity.ToTable("raw_payments", schema: "bronze");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.PspId)
                        .HasDatabaseName("idx_raw_payments_psp_id");

                entity.HasIndex(e => new { e.PspId, e.FileHash })
                        .HasDatabaseName("idx_raw_payments_psp_id_file_hash");
            });
        }

        private static void ConfigureSilver(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InternalPayment>(entity =>
            {
                entity.ToTable("internal_payments", schema: "silver");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .HasColumnName("id");

                entity.Property(e => e.TxId)
                    .IsRequired()
                    .HasColumnName("tx_id");

                entity.Property(e => e.TxDate)
                    .IsRequired()
                    .HasColumnName("tx_date");

                entity
                    .Property(e => e.Amount)
                    .IsRequired()
                    .HasColumnName("amount");

                entity
                    .Property(e => e.CurrencyCode)
                    .IsRequired()
                    .HasColumnName("currency_code");

                entity
                    .Property(e => e.UserId)
                    .IsRequired()
                    .HasColumnName("user_id");

                // todo:  this should be indexed
                entity
                    .Property(e => e.ProviderTxId)
                    .IsRequired()
                    .HasColumnName("provider_tx_id");

                entity
                    .Property(e => e.UserEmail)
                    .IsRequired()
                    .HasColumnName("user_email");

                entity
                    .Property(e => e.ClientId)
                    .IsRequired()
                    .HasColumnName("client_id");

                entity
                    .Property(e => e.Description)
                    .HasColumnName("description");

                entity
                    .Property(e => e.Status)
                    .IsRequired()
                    .HasColumnName("status");

                entity
                    .Property(e => e.System)
                    .IsRequired()
                    .HasColumnName("system");

                entity
                    .Property(e => e.Hash)
                    .IsRequired()
                    .HasColumnName("hash");

                entity.HasIndex(e => e.TxId)
                    .HasDatabaseName("idx_internal_payments_provider_tx_id");

                entity.HasIndex(e => e.TxDate)
                    .HasDatabaseName("idx_internal_payments_date");
            });

            modelBuilder.Entity<ExternalPayment>(entity =>
            {
                entity.ToTable("external_payments", schema: "silver");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .HasColumnName("id");

                entity.Property(e => e.ExternalSystem)
                    .IsRequired()
                    .HasColumnName("external_system");

                entity.Property(e => e.ExternalPaymentId)
                    .IsRequired()
                    .HasColumnName("external_payment_id");

                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasColumnName("player_id");

                entity.Property(e => e.BrandId)
                    .IsRequired()
                    .HasColumnName("brand_id");

                entity.Property(e => e.PspId)
                    .HasColumnType("integer")
                    .IsRequired()
                    .HasColumnName("psp_id");

                entity.Property(e => e.Amount)
                    .HasColumnName("amount")
                    .HasPrecision(18, 2);

                entity.Property(e => e.Hash)
                    .IsRequired()
                    .HasColumnName("hash");

                entity.Property(e => e.TxId)
                    .IsRequired()
                    .HasColumnName("tx_id");

                entity.Property(e => e.CurrencyCode)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasColumnName("currency_code");

                entity.Property(e => e.TxDate)
                    .IsRequired()
                    .HasColumnName("tx_date");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasColumnName("status");

                entity.Property(e => e.RawPaymentId)
                    .IsRequired()
                    .HasColumnName("raw_payment_id");

                entity.HasOne(e => e.RawPayment)
                    .WithMany()
                    .HasForeignKey(e => e.RawPaymentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Action)
                    .IsRequired()
                    .HasColumnName("action");

                entity.HasIndex(e => e.ClientId)
                    .HasDatabaseName("idx_external_payments_player");

                entity.HasIndex(e => e.BrandId)
                    .HasDatabaseName("idx_external_payments_brand");

                entity.HasIndex(e => new { e.PspId, e.TxDate })
                    .HasDatabaseName("idx_external_payments_psp_ts");
            });
        }

        private static void ConfigureGold(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReconciliationComment>(entity =>
            {
                entity.ToTable("reconciliation_comments", schema: "gold");

                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Reconciliation)
                    .WithMany()
                    .HasForeignKey(e => e.ReconciliationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.ReconciliationId })
                    .HasDatabaseName("idx_reconciliation_comments_reconciliation_id");
            });

            modelBuilder.Entity<PspSettlement>(entity =>
            {
                entity.ToTable("psp_settlements", schema: "gold");

                entity
                    .Property(e => e.Id)
                    .HasColumnType("integer")
                    .HasColumnName("id");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.PspId)
                    .IsRequired()
                    .HasColumnName("psp_id");

                entity.Property(e => e.CurrencyCode)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasColumnName("currency_code");

                entity.Property(e => e.Amount)
                    .IsRequired()
                    .HasColumnName("amount");

                entity.Property(e => e.TotalFees)
                    .HasColumnName("total_fees")
                    .HasPrecision(18, 2);

                entity.Property(e => e.NetSettlement)
                    .HasColumnName("net_settlement")
                    .HasPrecision(18, 2);

                entity.Property(e => e.ExternalPaymentId)
                    .IsRequired()
                    .HasColumnName("external_payment_id");

                entity.Property(e => e.InternalPaymentId)
                    .IsRequired()
                    .HasColumnName("internal_payment_id");

                entity.HasOne(e => e.InternalPayment)
                    .WithMany()
                    .HasForeignKey(e => e.InternalPaymentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ExternalPayment)
                    .WithMany()
                    .HasForeignKey(e => e.ExternalPaymentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.TxDate)
                    .IsRequired()
                    .HasColumnName("tx_date");

                entity.HasIndex(e => new { e.ReconciliationRunId })
                    .HasDatabaseName("idx_psp_reconciliation_run_id");

                entity.HasIndex(e => new { e.PspId, e.TxDate })
                    .HasDatabaseName("idx_psp_settlement_psp_txdate");
            });
        }

        private List<AuditTrail> OnBeforeSaveChanges(string userId)
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditTrail>();
            foreach (var entry in ChangeTracker.Entries<IAuditTrial>())
            {
                if (entry.Entity is AuditTrail ||
                    entry.State == EntityState.Detached ||
                    entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditTrail()
                {
                    DateTime = _dateTime.Now,
                    TableName = entry.Entity.GetType().Name,
                    UserId = userId,
                    AffectedColumns = new List<string>()
                };
                auditEntries.Add(auditEntry);
                foreach (var property in entry.Properties)
                {

                    if (property.IsTemporary)
                    {
                        auditEntry.TemporaryProperties.Add(property);
                        continue;
                    }
                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.PrimaryKey[propertyName] = property.CurrentValue;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = AuditType.Create;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;

                        case EntityState.Deleted:
                            auditEntry.AuditType = AuditType.Delete;
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified && property.OriginalValue?.Equals(property.CurrentValue) == false)
                            {
                                auditEntry.AffectedColumns.Add(propertyName);
                                auditEntry.AuditType = AuditType.Update;
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
            }

            foreach (var auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
            {
                AuditTrails.Add(auditEntry);
            }
            return auditEntries.Where(_ => _.HasTemporaryProperties).ToList();
        }

        private Task OnAfterSaveChanges(List<AuditTrail> auditEntries, CancellationToken cancellationToken = new())
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return Task.CompletedTask;

            foreach (var auditEntry in auditEntries)
            {
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.PrimaryKey[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }
                AuditTrails.Add(auditEntry);
            }
            return SaveChangesAsync(cancellationToken);
        }
    }
}
