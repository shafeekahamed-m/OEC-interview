using MediatR;
using RL.Backend.Models;

namespace RL.Backend.Commands
{
    public class AddUserToPlanProcedureCommand : IRequest<ApiResponse<Unit>>
    {
        public int PlanProcedureId { get; set; }
        public List<int> UserIds { get; set; }
    }
}
