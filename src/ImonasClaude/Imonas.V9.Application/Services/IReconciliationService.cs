using Imonas.V9.Domain.Entities.Reconciliation;

namespace Imonas.V9.Application.Services;

public interface IReconciliationService
{
    Task<ReconciliationRun> StartRunAsync(string runName, string rulePackVersion);
    Task<ReconciliationRun?> GetRunAsync(Guid runId);
    Task<List<ReconciliationRun>> GetRecentRunsAsync(int count = 10);
    Task<bool> CompleteRunAsync(Guid runId);
}
