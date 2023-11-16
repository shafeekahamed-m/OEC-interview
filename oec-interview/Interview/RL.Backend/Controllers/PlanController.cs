using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using RL.Backend.Commands;
using RL.Backend.Commands.Handlers.Plans;
using RL.Backend.Models;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class PlanController : ControllerBase
{
    private readonly ILogger<PlanController> _logger;
    private readonly RLContext _context;
    private readonly IMediator _mediator;

    public PlanController(ILogger<PlanController> logger, RLContext context, IMediator mediator)
    {
        _logger = logger;
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
    [EnableQuery]
    public IEnumerable<Plan> Get()
    {
        return _context.Plans;
    }

    [HttpPost]
    public async Task<IActionResult> PostPlan(CreatePlanCommand command, CancellationToken token)
    {
        var response = await _mediator.Send(command, token);

        return response.ToActionResult();
    }

    [HttpPost("AddProcedureToPlan")]
    public async Task<IActionResult> AddProcedureToPlan(AddProcedureToPlanCommand command, CancellationToken token)
    {
        var response = await _mediator.Send(command, token);
        if (response.Succeeded)
        {
            var newlyAddedPlanProcedure = await GetLatestPlanProcedureId();
            var newPlanProcedureResponse = new ApiResponse<int> { Value = newlyAddedPlanProcedure };
            return newPlanProcedureResponse.ToActionResult();
        }
        return response.ToActionResult();
    }
    private async Task<int> GetLatestPlanProcedureId()
    {
        var planProcedure = await _context.PlanProcedures.OrderByDescending(x => x.PlanProcedureId).FirstOrDefaultAsync();

        return planProcedure.PlanProcedureId;
    }
}
