using Microsoft.EntityFrameworkCore;
using Imonas.RuleEngine.Models;
using System.Text.Json;

namespace Imonas.RuleEngine.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<RuleGroup> RuleGroups => Set<RuleGroup>();
    public DbSet<Rule> Rules => Set<Rule>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RuleGroup>()
            .HasMany(g => g.Rules)
            .WithOne(r => r.RuleGroup!)
            .HasForeignKey(r => r.RuleGroupId)            
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Rule>()
            .HasIndex(r => new { r.RuleGroupId, r.Field, r.Order });

        modelBuilder.Entity<RuleGroup>()
            .HasMany(g => g.Children)
            .WithOne(c => c.ParentGroup)
            .HasForeignKey(c => c.ParentGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RuleGroup>().HasQueryFilter(g => !g.IsDeleted);
    }

    public override int SaveChanges()
    {
        ApplyVersioningAndTimestamps();
        var audit = BuildAuditEntries();
        var result = base.SaveChanges();
        if (audit.Count > 0)
        {
            AuditEntries.AddRange(audit);
            base.SaveChanges();
        }
        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyVersioningAndTimestamps();
        var audit = BuildAuditEntries();
        var result = await base.SaveChangesAsync(cancellationToken);
        if (audit.Count > 0)
        {
            AuditEntries.AddRange(audit);
            await base.SaveChangesAsync(cancellationToken);
        }
        return result;
    }

    private void ApplyVersioningAndTimestamps()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<RuleGroup>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
                entry.Entity.Version = 1;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.Version += 1;
            }
            else if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = now;
                entry.Entity.Version += 1;
            }
        }
    }

    private List<AuditEntry> BuildAuditEntries()
    {
        var list = new List<AuditEntry>();
        foreach (var e in ChangeTracker.Entries())
        {
            if (e.Entity is AuditEntry) continue;

            string action = e.State switch
            {
                EntityState.Added => "Insert",
                EntityState.Modified => "Update",
                EntityState.Deleted => "SoftDelete",
                _ => ""
            };
            if (string.IsNullOrEmpty(action)) continue;

            var changes = new Dictionary<string, object?>();
            foreach (var prop in e.Properties)
            {
                if (!prop.IsTemporary && (prop.IsModified || action=="Insert"))
                {
                    changes[prop.Metadata.Name] = new {
                        Old = prop.OriginalValue,
                        New = prop.CurrentValue
                    };
                }
            }

            list.Add(new AuditEntry
            {
                EntityName = e.Entity.GetType().Name,
                Action = action,
                ChangesJson = System.Text.Json.JsonSerializer.Serialize(changes),
                Timestamp = DateTime.UtcNow,
                User = "system"
            });
        }
        return list;
    }
}
