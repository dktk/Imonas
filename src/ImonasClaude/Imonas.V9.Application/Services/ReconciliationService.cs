using Imonas.V9.Domain.Entities.Reconciliation;
using Imonas.V9.Domain.Enums;
using Imonas.V9.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Imonas.V9.Application.Services;

public class ReconciliationService : IReconciliationService
{
    private readonly ImonasDbContext _context;

    public ReconciliationService(ImonasDbContext context)
    {
        _context = context;
    }

    public async Task<ReconciliationRun> StartRunAsync(string runName, string rulePackVersion)
    {
        var run = new ReconciliationRun
        {
            Id = Guid.NewGuid(),
            RunName = runName,
            RulePackVersion = rulePackVersion,
            Status = RunStatus.Running,
            StartedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            IsReplayable = true
        };

        _context.ReconciliationRuns.Add(run);
        await _context.SaveChangesAsync();

        return run;
    }

    public async Task<ReconciliationRun?> GetRunAsync(Guid runId)
    {
        return await _context.ReconciliationRuns
            .Include(r => r.Metrics)
            .FirstOrDefaultAsync(r => r.Id == runId);
    }

    public async Task<List<ReconciliationRun>> GetRecentRunsAsync(int count = 10)
    {
        return await _context.ReconciliationRuns
            .OrderByDescending(r => r.StartedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<bool> CompleteRunAsync(Guid runId)
    {
        var run = await _context.ReconciliationRuns.FindAsync(runId);
        if (run == null) return false;

        run.Status = RunStatus.Completed;
        run.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
