using SG.Common;

namespace Application.Features.Psps.Commands
{
    public class CreateInternalSystemCommand : IRequest<Result<int>>
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class CreateInternalSystemCommandHandler(
        IApplicationDbContext context) : IRequestHandler<CreateInternalSystemCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(CreateInternalSystemCommand request, CancellationToken cancellationToken)
        {
            // Check for duplicate name
            var exists = await context.InternalSystems
                .AnyAsync(s => s.Name.ToLower() == request.Name.ToLower(), cancellationToken);

            if (exists)
            {
                return Result<int>.BuildFailure($"An internal system with name '{request.Name}' already exists.");
            }

            var entity = new InternalSystem
            {
                Name = request.Name,
                IsActive = request.IsActive
            };

            context.InternalSystems.Add(entity);
            await context.SaveChangesAsync(cancellationToken);

            return Result<int>.BuildSuccess(entity.Id);
        }
    }
}
