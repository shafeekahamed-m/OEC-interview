using MediatR;
using Microsoft.EntityFrameworkCore;
using RL.Backend.Exceptions;
using RL.Backend.Models;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.Commands.Handlers.Plans;

public class AddUserToPlanProcedureCommandHandler : IRequestHandler<AddUserToPlanProcedureCommand, ApiResponse<Unit>>
{
    private readonly ILogger<AddUserToPlanProcedureCommandHandler> _logger;
    private readonly RLContext _context;

    public AddUserToPlanProcedureCommandHandler(ILogger<AddUserToPlanProcedureCommandHandler> logger,RLContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<ApiResponse<Unit>> Handle(AddUserToPlanProcedureCommand request, CancellationToken cancellationToken)
    {
        try
        {
            List<int> userIds = request.UserIds;
            int planProcedureId = request.PlanProcedureId;
            bool userAssignmentUpdated = false;
            //Validate request
            if (planProcedureId < 0)
                return ApiResponse<Unit>.Fail(new BadRequestException("Invalid PlanProcedureId"));
            if (userIds?.Count > 0)
            {
                var existingUserIds = await _context.Users
                                        .Where(u => userIds.Contains(u.UserId))
                                        .Select(u => u.UserId)
                                        .ToListAsync(cancellationToken);

                var nonExistingUserIds = userIds.Except(existingUserIds).ToList();

                if (nonExistingUserIds.Count > 0)
                    return ApiResponse<Unit>.Fail(new NotFoundException($"UserId:{string.Join(",", nonExistingUserIds.Select(n => n.ToString()))} not found"));
            }

            var planProcedure = await _context.PlanProcedures.FirstOrDefaultAsync(p => p.PlanProcedureId == planProcedureId, cancellationToken: cancellationToken);

            if (planProcedure is null)
                return ApiResponse<Unit>.Fail(new NotFoundException($"PlanProcedureId: {planProcedureId} not found"));

            var userAssignments = _context.UserPlanProcedure;
            var allUserAssignments = await _context.UserPlanProcedure
                        .Where(x => x.PlanProcedureId == planProcedureId)
                        .ToListAsync(cancellationToken: cancellationToken);

            if (userIds is not null && userIds?.Count > 0)
            {
                var deleteUserAssignments = await _context.UserPlanProcedure
                        .Where(x => x.PlanProcedureId == planProcedureId && !userIds.Any(y => y == x.UserId))
                        .ToListAsync(cancellationToken: cancellationToken);

                //mark delete for userIds which are not part of the request
                deleteUserAssignments?.ForEach(item => { item.IsDelete = true; item.UpdateDate = DateTime.Now; });

                var nonExistingUserAssignments = userIds.Except(_context.UserPlanProcedure
                                                    .Where(x => x.PlanProcedureId == planProcedureId)
                                                    .Select(x => x.UserId)
                                                    .ToList())
                                                .ToList();


                foreach (var item in nonExistingUserAssignments)
                {
                    userAssignments.Add(new UserPlanProcedure
                    {
                        PlanProcedureId = planProcedureId,
                        UserId = item
                    });
                }

                var updateExistingUserAssignments = userIds.Except(_context.UserPlanProcedure
                                                    .Where(x => x.PlanProcedureId == planProcedureId && !x.IsDelete)
                                                    .Select(x => x.UserId)
                                                    .ToList())
                                                .ToList();
                foreach (var item in updateExistingUserAssignments)
                {
                    var matchingUserAssignment = userAssignments.FirstOrDefault(item1 => item1.PlanProcedureId == planProcedureId && item == item1.UserId);

                    if (matchingUserAssignment != null)
                    {
                        matchingUserAssignment.IsDelete = false;
                        matchingUserAssignment.UpdateDate = DateTime.Now;
                    }
                }

                if (deleteUserAssignments is not null && deleteUserAssignments.Count > 0
                    || nonExistingUserAssignments is not null && nonExistingUserAssignments.Count > 0
                    || updateExistingUserAssignments is not null && updateExistingUserAssignments.Count > 0) userAssignmentUpdated = true;
            }
            else
            {
                //mark delete for userIds which are not part of the request
                allUserAssignments?.ForEach(item => { item.IsDelete = true; item.UpdateDate = DateTime.Now; });
                userAssignmentUpdated = true;
            }

            if (userAssignmentUpdated)
                await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<Unit>.Succeed(new Unit());
        }
        catch (BadRequestException ex)
        {
            _logger.LogError(ex, "Bad request exception: {Message}", ex.Message);
            return ApiResponse<Unit>.Fail(new BadRequestException(ex.Message));
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "Not found exception: {Message}", ex.Message);
            return ApiResponse<Unit>.Fail(new NotFoundException(ex.Message));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An unexpected error occurred: {Message}", e.Message);
            return ApiResponse<Unit>.Fail(e);
        }
    }
}
