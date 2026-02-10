using Application.Features.Cases.DTOs;
using Domain.Entities.Cases;
using Domain.Enums;
using SG.Common;

namespace Application.Features.Cases.Commands
{
    public class CreateCaseCommand : IRequest<Result<ExceptionCaseDto>>
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public CaseSeverity Severity { get; set; } = CaseSeverity.Medium;
        public VarianceType VarianceType { get; set; } = VarianceType.Amount;
        public decimal? VarianceAmount { get; set; }
        public string? AssignedTo { get; set; }
        public DateTime? DueDate { get; set; }
        public string? LinkedTransactionId { get; set; }
        public string? RootCauseCode { get; set; }
        public string? InitialNotes { get; set; }
    }

    public class CreateCaseCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IDateTime dateTime) :
        IRequestHandler<CreateCaseCommand, Result<ExceptionCaseDto>>
    {
        public async Task<Result<ExceptionCaseDto>> Handle(CreateCaseCommand request, CancellationToken cancellationToken)
        {
            var caseNumber = $"CASE-{dateTime.Now:yyyy}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            var transaction = await context.InternalPayments.FirstOrDefaultAsync(x => x.TxId == request.LinkedTransactionId || x.ProviderTxId == request.LinkedTransactionId);

            if (transaction == null)
            {
                return Result<ExceptionCaseDto>.CreateFailure("There is no transaction with Id: " + request.LinkedTransactionId);
            }

            var caseEntity = new ExceptionCase
            {
                CaseNumber = caseNumber,
                Title = request.Title,
                Description = request.Description,
                Severity = request.Severity,
                VarianceType = request.VarianceType,
                VarianceAmount = request.VarianceAmount,
                AssignedToId = request.AssignedTo,
                InternalTransactionId = transaction.Id,
                DueDate = request.DueDate,
                ExternalTransactionId = string.IsNullOrWhiteSpace(request.LinkedTransactionId) ? null : int.Parse(request.LinkedTransactionId),
                RootCauseCode = request.RootCauseCode,
                Status = CaseStatus.Open,
                Created = dateTime.Now,
                UserId = currentUserService.UserId ?? "System"
            };

            context.ExceptionCases.Add(caseEntity);
            await context.SaveChangesAsync(cancellationToken);

            // Add initial notes as comment if provided
            if (!string.IsNullOrWhiteSpace(request.InitialNotes))
            {
                var comment = new CaseComment
                {
                    CaseId = caseEntity.Id,
                    Comment = request.InitialNotes,
                    CommentedBy = currentUserService.UserId ?? "System",
                    Created = dateTime.Now,
                    UserId = currentUserService.UserId ?? "System"
                };
                context.CaseComments.Add(comment);
                await context.SaveChangesAsync(cancellationToken);
            }

            var dto = mapper.Map<ExceptionCaseDto>(caseEntity);
            return Result<ExceptionCaseDto>.CreateSuccess(dto, $"Case {caseNumber} created successfully.");
        }
    }
}
