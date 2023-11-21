using MediatR;
using Microsoft.EntityFrameworkCore;
using RL.Backend.Exceptions;
using RL.Backend.Models;
using RL.Data;

namespace RL.Backend.Commands.Handlers.Plans;

public class RemoveUserFromPlanProcedureCommandHandler : IRequestHandler<RemoveUserFromPlanProcedureCommand, ApiResponse<Unit>>
{
    private readonly ILogger<RemoveUserFromPlanProcedureCommandHandler> _logger;
    private readonly RLContext _context;

    public RemoveUserFromPlanProcedureCommandHandler(ILogger<RemoveUserFromPlanProcedureCommandHandler> logger, RLContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<ApiResponse<Unit>> Handle(RemoveUserFromPlanProcedureCommand request, CancellationToken cancellationToken)
    {
        bool userAssignmentUpdated = false;
        try
        {
            List<int> userIds = request.UserIds;
            int planProcedureId = request.PlanProcedureId;
            UserPlanProcedureCommandValidation userPlanProcedureCommandValidation = new(_context);

            // Validate PlanProcedureId
            var planProcedureValidationResult = await userPlanProcedureCommandValidation.ValidatePlanProcedureIdAsync(planProcedureId, cancellationToken);
            if (!planProcedureValidationResult.Succeeded)
            {
                return planProcedureValidationResult;
            }

            // Validate UserIds
            var userIdsValidationResult = await userPlanProcedureCommandValidation.ValidateUserIdsAsync(userIds, cancellationToken);
            if (!userIdsValidationResult.Succeeded)
            {
                return userIdsValidationResult;
            }

            //if there are valid userIds present in the request
            if (userIds is not null && userIds?.Count > 0)
            {
                //get user assignments which needs to be deleted
                var deleteUserAssignments = await _context.UserPlanProcedure
                        .Where(x => x.PlanProcedureId == planProcedureId && !userIds.Any(y => y == x.UserId))
                        .ToListAsync(cancellationToken: cancellationToken);

                //mark delete for userIds which are not part of the request
                deleteUserAssignments?.ForEach(item => { item.IsDelete = true; item.UpdateDate = DateTime.Now; });

                //set userAssignmentUpdated to true to update the context
                if (deleteUserAssignments is not null && deleteUserAssignments.Count > 0) userAssignmentUpdated = true;
            }
            else
            {
                //update all userIds to Delete true which are matching the planProcedureId

                //get user assignments which needs to be deleted
                var allUserAssignments = await _context.UserPlanProcedure
                            .Where(x => x.PlanProcedureId == planProcedureId)
                            .ToListAsync(cancellationToken: cancellationToken);

                //update delete flag for items matching the planProcedureId
                allUserAssignments?.ForEach(item => { item.IsDelete = true; item.UpdateDate = DateTime.Now; });

                //set userAssignmentUpdated to true to update the context
                userAssignmentUpdated = true;
            }

            //update the context
            if (userAssignmentUpdated)
                await _context.SaveChangesAsync(cancellationToken);

            //after context updated return success
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
        catch (NullReferenceException ex)
        {
            _logger.LogError(ex, "Null Reference exception: {Message}", ex.Message);
            return ApiResponse<Unit>.Fail(new NullReferenceException(ex.Message));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An unexpected error occurred: {Message}", e.Message);
            return ApiResponse<Unit>.Fail(e);
        }
    }
}
