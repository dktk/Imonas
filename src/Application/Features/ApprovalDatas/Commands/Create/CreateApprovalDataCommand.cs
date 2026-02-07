using Application.Features.ApprovalDatas.DTOs;

using SG.Common;

namespace Application.Features.ApprovalDatas.Commands.Create
{
    public class CreateApprovalDataCommand: ApprovalDataDto,IRequest<Result<bool>>
    {
      
    }
    

    public class CreateApprovalDataCommandHandler : IRequestHandler<CreateApprovalDataCommand,Result<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<CreateApprovalDataCommand> _localizer;
        public CreateApprovalDataCommandHandler(
            IApplicationDbContext context,
            IStringLocalizer<CreateApprovalDataCommand> localizer,
            IMapper mapper
            )
        {
            _context = context;
            _localizer = localizer;
            _mapper = mapper;
        }
        public  Task<Result<bool>> Handle(CreateApprovalDataCommand request, CancellationToken cancellationToken)
        {
           //TODO:Implementing CreateApprovalDataCommandHandler method 
           throw new System.NotImplementedException();
        }
    }
}
