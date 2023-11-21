using MediatR;
using Microsoft.EntityFrameworkCore;
using RL.Backend.Exceptions;
using RL.Backend.Models;
using RL.Data;

namespace RL.Backend.Commands;
public class UserPlanProcedureCommandValidation
{
    private readonly RLContext _context;

    public UserPlanProcedureCommandValidation(RLContext context)
    {
        _context = context;
    }
    public async Task<ApiResponse<Unit>> ValidatePlanProcedureIdAsync(int planProcedureId, CancellationToken cancellationToken)
    {
        //Validate request
        if (planProcedureId <= 0)
            return ApiResponse<Unit>.Fail(new BadRequestException($"Invalid PlanProcedureId: {planProcedureId}"));

        //Validating whether PlanProcedureId exists in PlanProcedure table
        var planProcedure = await _context.PlanProcedures.FirstOrDefaultAsync(p => p.PlanProcedureId == planProcedureId, cancellationToken: cancellationToken);
        //If PlanProcedureId does not exist throw NotFoundException
        if (planProcedure is null)
            return ApiResponse<Unit>.Fail(new NotFoundException($"PlanProcedureId: {planProcedureId} not found"));

        return ApiResponse<Unit>.Succeed(new Unit());
    }
    public async Task<ApiResponse<Unit>> ValidateUserIdsAsync(List<int> userIds, CancellationToken cancellationToken)
    {
        //Validating whether UserId exists in Users table
        if (userIds?.Count > 0)
        {
            //get the existingUserIds from User table
            var existingUserIds = await _context.Users
                                    .Where(u => userIds.Contains(u.UserId))
                                    .Select(u => u.UserId)
                                    .ToListAsync(cancellationToken);
            //compare with the input to find if the input contains any invalid userIds
            var nonExistingUserIds = userIds.Except(existingUserIds).ToList();
            //if incase of invalid userIds throw NotFoundException
            if (nonExistingUserIds.Count > 0)
                return ApiResponse<Unit>.Fail(new NotFoundException($"UserId:{string.Join(",", nonExistingUserIds.Select(n => n.ToString()))} not found"));
        }

        return ApiResponse<Unit>.Succeed(new Unit());
    }
}
