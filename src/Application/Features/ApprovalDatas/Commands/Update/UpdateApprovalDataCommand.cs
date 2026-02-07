using Application.Features.ApprovalDatas.DTOs;

using SG.Common;

namespace Application.Features.ApprovalDatas.Commands.Update
{
    public class UpdateApprovalDataCommand: ApprovalDataDto,IRequest<Result<bool>>
    {
      
    }

    public class UpdateApprovalDataCommandHandler : IRequestHandler<UpdateApprovalDataCommand,Result<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<UpdateApprovalDataCommandHandler> _localizer;
        public UpdateApprovalDataCommandHandler(
            IApplicationDbContext context,
            IStringLocalizer<UpdateApprovalDataCommandHandler> localizer,
             IMapper mapper
            )
        {
            _context = context;
            _localizer = localizer;
            _mapper = mapper;
        }
        public  Task<Result<bool>> Handle(UpdateApprovalDataCommand request, CancellationToken cancellationToken)
        {
           //TODO:Implementing UpdateApprovalDataCommandHandler method 
           throw new System.NotImplementedException();
        }
    }
}
