using Imonas.V9.Domain.Entities.Cases;
using Imonas.V9.Domain.Enums;
using Imonas.V9.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Imonas.V9.Application.Services;

public class CaseService : ICaseService
{
    private readonly ImonasDbContext _context;

    public CaseService(ImonasDbContext context)
    {
        _context = context;
    }

    public async Task<Case> CreateCaseAsync(Case caseEntity)
    {
        caseEntity.Id = Guid.NewGuid();
        caseEntity.CreatedAt = DateTime.UtcNow;
        caseEntity.Status = CaseStatus.Open;

        _context.Cases.Add(caseEntity);
        await _context.SaveChangesAsync();

        return caseEntity;
    }

    public async Task<Case?> GetCaseAsync(Guid caseId)
    {
        return await _context.Cases
            .Include(c => c.Comments)
            .Include(c => c.Attachments)
            .Include(c => c.Labels)
            .FirstOrDefaultAsync(c => c.Id == caseId);
    }

    public async Task<List<Case>> GetCasesAsync(string? assignedTo = null, int pageSize = 50)
    {
        var query = _context.Cases.AsQueryable();

        if (!string.IsNullOrEmpty(assignedTo))
        {
            query = query.Where(c => c.AssignedTo == assignedTo);
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> AddCommentAsync(Guid caseId, string comment, string commentedBy)
    {
        var caseEntity = await _context.Cases.FindAsync(caseId);
        if (caseEntity == null) return false;

        var caseComment = new CaseComment
        {
            Id = Guid.NewGuid(),
            CaseId = caseId,
            Comment = comment,
            CommentedBy = commentedBy,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = commentedBy
        };

        _context.CaseComments.Add(caseComment);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AssignCaseAsync(Guid caseId, string assignedTo)
    {
        var caseEntity = await _context.Cases.FindAsync(caseId);
        if (caseEntity == null) return false;

        caseEntity.AssignedTo = assignedTo;
        caseEntity.Status = CaseStatus.InProgress;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CloseCaseAsync(Guid caseId, string closedBy, string? resolutionNotes)
    {
        var caseEntity = await _context.Cases.FindAsync(caseId);
        if (caseEntity == null) return false;

        caseEntity.Status = CaseStatus.Closed;
        caseEntity.ResolvedAt = DateTime.UtcNow;
        caseEntity.ResolvedBy = closedBy;
        caseEntity.ResolutionNotes = resolutionNotes;

        await _context.SaveChangesAsync();
        return true;
    }
}
