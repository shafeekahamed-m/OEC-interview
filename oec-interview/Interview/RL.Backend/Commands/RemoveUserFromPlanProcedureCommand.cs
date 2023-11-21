using MediatR;
using RL.Backend.Models;

namespace RL.Backend.Commands
{
    public class RemoveUserFromPlanProcedureCommand : IRequest<ApiResponse<Unit>>
    {
        public int PlanProcedureId { get; set; }
        public List<int> UserIds { get; set; }
    }
}
