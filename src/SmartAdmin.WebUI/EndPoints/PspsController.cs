using Application.Features.Psps.Queries;

namespace SmartAdmin.WebUI.EndPoints
{
    public class PspsController(IMediator mediator) : Controller
    {
        [HttpGet("active-psps")]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var psps = await mediator.Send(new GetAllPspsQuery(), cancellationToken);

            return Ok(psps);
        }
    }
}
