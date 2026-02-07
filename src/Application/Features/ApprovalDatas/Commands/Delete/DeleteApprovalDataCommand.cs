using SG.Common;

namespace Application.Features.ApprovalDatas.Commands.Delete
{
    public class DeleteApprovalDataCommand: IRequest<Result<bool>>
    {
      
    }
    public class DeleteCheckedApprovalDatasCommand : IRequest<Result<bool>>
    {
     
    }

    public class DeleteApprovalDataCommandHandler : 
                 IRequestHandler<DeleteApprovalDataCommand,Result<bool>>,
                 IRequestHandler<DeleteCheckedApprovalDatasCommand,Result<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<DeleteApprovalDataCommandHandler> _localizer;
        public DeleteApprovalDataCommandHandler(
            IApplicationDbContext context,
            IStringLocalizer<DeleteApprovalDataCommandHandler> localizer,
             IMapper mapper
            )
        {
            _context = context;
            _localizer = localizer;
            _mapper = mapper;
        }
        public  Task<Result<bool>> Handle(DeleteApprovalDataCommand request, CancellationToken cancellationToken)
        {
           //TODO:Implementing DeleteApprovalDataCommandHandler method 
           throw new System.NotImplementedException();
        }

        public  Task<Result<bool>> Handle(DeleteCheckedApprovalDatasCommand request, CancellationToken cancellationToken)
        {
           //TODO:Implementing DeleteCheckedApprovalDatasCommandHandler method 
           throw new System.NotImplementedException();
        }
    }
}
