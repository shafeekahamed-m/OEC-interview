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
                //get all the userassignments to update/insert into the context
                var userAssignments = _context.UserPlanProcedure;

                //get list of all userIds which are not matching the planProcedureId(to insert) using the request
                var nonExistingUserAssignments = userIds.Except(_context.UserPlanProcedure
                                                    .Where(x => x.PlanProcedureId == planProcedureId)
                                                    .Select(x => x.UserId)
                                                    .ToList())
                                                .ToList();

                //insert the data to the context based on nonExistingUserAssignments
                foreach (var item in nonExistingUserAssignments)
                {
                    userAssignments.Add(new UserPlanProcedure
                    {
                        PlanProcedureId = planProcedureId,
                        UserId = item
                    });
                }

                //get list of all userIds which are matching the planProcedureId(to update) using the request
                var updateExistingUserAssignments = userIds.Except(_context.UserPlanProcedure
                                                    .Where(x => x.PlanProcedureId == planProcedureId && !x.IsDelete)
                                                    .Select(x => x.UserId)
                                                    .ToList())
                                                .ToList();

                //update the soft delete flag to false and update the lastupdated date
                foreach (var item in updateExistingUserAssignments)
                {
                    //get the object matching the userId and planProcedureId
                    var matchingUserAssignment = userAssignments.FirstOrDefault(item1 => item1.PlanProcedureId == planProcedureId && item == item1.UserId);

                    if (matchingUserAssignment != null)
                    {
                        matchingUserAssignment.IsDelete = false;
                        matchingUserAssignment.UpdateDate = DateTime.Now;
                    }
                }

                //use this flag to check if the context need to updated
                if (nonExistingUserAssignments is not null && nonExistingUserAssignments.Count > 0
                    || updateExistingUserAssignments is not null && updateExistingUserAssignments.Count > 0) userAssignmentUpdated = true;
            }
            
            //update context based on the flag
            if (userAssignmentUpdated)
                await _context.SaveChangesAsync(cancellationToken);

            //return success
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
