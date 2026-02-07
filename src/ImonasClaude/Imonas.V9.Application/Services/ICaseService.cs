using Imonas.V9.Domain.Entities.Cases;

namespace Imonas.V9.Application.Services;

public interface ICaseService
{
    Task<Case> CreateCaseAsync(Case caseEntity);
    Task<Case?> GetCaseAsync(Guid caseId);
    Task<List<Case>> GetCasesAsync(string? assignedTo = null, int pageSize = 50);
    Task<bool> AddCommentAsync(Guid caseId, string comment, string commentedBy);
    Task<bool> AssignCaseAsync(Guid caseId, string assignedTo);
    Task<bool> CloseCaseAsync(Guid caseId, string closedBy, string? resolutionNotes);
}
