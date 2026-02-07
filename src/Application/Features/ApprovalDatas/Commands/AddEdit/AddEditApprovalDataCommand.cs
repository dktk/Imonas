using Application.Features.ApprovalDatas.DTOs;

using SG.Common;

namespace Application.Features.ApprovalDatas.Commands.AddEdit
{
    public class AddEditApprovalDataCommand: ApprovalDataDto,IRequest<Result<bool>>
    {
      
    }

    public class AddEditApprovalDataCommandHandler : IRequestHandler<AddEditApprovalDataCommand,Result<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<AddEditApprovalDataCommandHandler> _localizer;
        public AddEditApprovalDataCommandHandler(
            IApplicationDbContext context,
            IStringLocalizer<AddEditApprovalDataCommandHandler> localizer,
            IMapper mapper
            )
        {
            _context = context;
            _localizer = localizer;
            _mapper = mapper;
        }
        public  Task<Result<bool>> Handle(AddEditApprovalDataCommand request, CancellationToken cancellationToken)
        {
           //TODO:Implementing AddEditApprovalDataCommandHandler method 
           throw new System.NotImplementedException();
        }
    }
}
