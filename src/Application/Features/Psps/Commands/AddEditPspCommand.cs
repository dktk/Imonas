// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Application.Features.Psps.DTOs;

using SG.Common;

namespace Application.Features.Psps.Commands
{
    public class AddEditPspCommandValidator : AbstractValidator<AddEditPspCommand>
    {
        public AddEditPspCommandValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty();
        }
    }

    public class AddEditPspCommand : PspDto, IRequest<Result<int>>, IMapFrom<Psp>
    {
        public int InternalSystemId { get; set; }
    }

    public class AddEditPspCommandHandler(
        IApplicationDbContext context,
        ILogger<AddEditPspCommandHandler> logger,
        IMapper mapper) : IRequestHandler<AddEditPspCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(AddEditPspCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Result<int>.CreateFailure(new[] { "Empty name." });
            }

            try
            {
                if (request.Id > 0)
                {
                    var item = await context.Psps.FindAsync(new object[] { request.Id }, cancellationToken);
                    item = mapper.Map(request, item);
                    item.Code = item.Code.ToUpper();
                    await context.SaveChangesAsync(cancellationToken);

                    return Result<int>.CreateSuccess(item.Id);
                }
                else
                {
                    var item = mapper.Map<Psp>(request);
                    item.Code = item.Code.ToUpper();
                    context.Psps.Add(item);
                    await context.SaveChangesAsync(cancellationToken);

                    return Result<int>.CreateSuccess(item.Id);
                }
            }
            catch (Exception ex)
            {
                // todo: get correlation ID
                logger.LogError(ex, "An error occured while trying to save/edit PSP." + request.ToJson());

                return Result<int>.CreateFailure(["An error occured while trying to save/edit PSP."]);
            }
        }
    }
}
