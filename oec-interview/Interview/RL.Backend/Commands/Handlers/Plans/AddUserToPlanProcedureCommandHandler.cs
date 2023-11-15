using MediatR;
using Microsoft.EntityFrameworkCore;
using RL.Backend.Exceptions;
using RL.Backend.Models;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.Commands.Handlers.Plans;

public class AddUserToPlanProcedureCommandHandler : IRequestHandler<AddUserToPlanProcedureCommand, ApiResponse<Unit>>
{
    private readonly RLContext _context;

    public AddUserToPlanProcedureCommandHandler(RLContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<Unit>> Handle(AddUserToPlanProcedureCommand request, CancellationToken cancellationToken)
    {
        try
        {
            //Validate request
            if (request.PlanProcedureId < 0)
                return ApiResponse<Unit>.Fail(new BadRequestException("Invalid PlanProcedureId"));

            var planProcedure = await _context.PlanProcedures.FirstOrDefaultAsync(p => p.PlanProcedureId == request.PlanProcedureId, cancellationToken: cancellationToken);

            if (planProcedure is null)
                return ApiResponse<Unit>.Fail(new NotFoundException($"PlanProcedureId: {request.PlanProcedureId} not found"));

            var userAssignments = _context.UserPlanProcedure;
            bool userAssignmentUpdated = false;

            if (request.UserIds?.Count > 0)
            {
                foreach (var userId in request.UserIds)
                {
                    var userExist = await _context.Users.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken: cancellationToken);
                    if (userExist is null)
                        return ApiResponse<Unit>.Fail(new NotFoundException($"UserId:{userId} not found"));
                    //var userAssignments = await _context.UserPlanProcedure.FirstOrDefaultAsync(p => p.PlanProcedureId == request.PlanProcedureId, cancellationToken: cancellationToken);

                    var userAssignmentExist = await _context.UserPlanProcedure
                        .FirstOrDefaultAsync(x => x.PlanProcedureId == request.PlanProcedureId && x.UserId == userId, cancellationToken: cancellationToken);
                    if (userAssignmentExist is null)
                    {
                        userAssignments.Add(new UserPlanProcedure
                        {
                            PlanProcedureId = request.PlanProcedureId,
                            UserId = userId
                        });
                        if(!userAssignmentUpdated) userAssignmentUpdated = true;
                    }
                }
            }
            else
            {

            }


            ////if (request.UserIds?.Count() > 0 && CheckUserExists(_context.Users, request.UserIds))
            ////    return ApiResponse<Unit>.Fail(new BadRequestException("Invalid UserId"));

            ////var users = await request.UserIds.All(id => _context.Users.Any(user => user.UserId == id));
            ////var usersExist = request.UserIds.All(id => _context.Users.Any(user => user.UserId == id));
            //var usersExist = await _context.Users.AllAsync(id => request.UserIds.Any(user => user == id.UserId));


            //if (!usersExist)
            //    return ApiResponse<Unit>.Fail(new NotFoundException($"UserId not found"));

            ////var userPlanProcedures = await _context.UserPlanProcedure.Select(u => u).Where(p => p.PlanProcedureId == request.PlanProcedureId);

            //var check = await _context.UserPlanProcedure.Include(u => u).AllAsync(p => p.PlanProcedureId == request.PlanProcedureId);
            ////Already has the procedure, so just succeed
            ////if (plan.PlanProcedures.Any(p => p.ProcedureId == procedure.ProcedureId))
            ////    return ApiResponse<Unit>.Succeed(new Unit());

            ////plan.PlanProcedures.Add(new PlanProcedure
            ////{
            ////    ProcedureId = procedure.ProcedureId
            ////});

            if(userAssignmentUpdated)
                await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<Unit>.Succeed(new Unit());
        }
        catch (Exception e)
        {
            return ApiResponse<Unit>.Fail(e);
        }
    }
    bool CheckUserExists(DbSet<User> Users, List<int> UserIds)
    {
        return UserIds.All(id => Users.Any(user => user.UserId == id));
    }
}
