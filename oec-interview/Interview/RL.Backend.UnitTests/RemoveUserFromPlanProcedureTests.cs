using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RL.Backend.Commands;
using RL.Backend.Commands.Handlers.Plans;
using RL.Backend.Exceptions;
using RL.Data;
using RL.Data.DataModels;
using System.Numerics;

namespace RL.Backend.UnitTests;

[TestClass]
public  class RemoveUserFromPlanProcedureTests
{
    private readonly ILogger<AddUserToPlanProcedureCommandHandler> _logger;
    [TestMethod]
    [DataRow(-1)]
    [DataRow(0)]
    [DataRow(int.MinValue)]
    public async Task RemoveUserFromPlanProcedureTests_InvalidPlanProcedureId_ReturnsBadRequest(int procedureId)
    {
        //Given
        var context = new Mock<RLContext>();
        var sut = new AddUserToPlanProcedureCommandHandler(_logger, context.Object);
        var request = new AddUserToPlanProcedureCommand()
        {
            PlanProcedureId = procedureId,
            UserIds = new List<int> { 0 },
        };
        //When
        var result = await sut.Handle(request, new CancellationToken());

        //Then
        result.Exception.Should().BeOfType(typeof(BadRequestException));
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(-1, -1)]
    [DataRow(int.MinValue, int.MinValue)]
    public async Task RemoveUserFromPlanProcedureTests_InvalidUserId_ReturnsBadRequest(int planProcedureId, int userId)
    {
        //Given
        var context = new Mock<RLContext>();
        var sut = new AddUserToPlanProcedureCommandHandler(_logger, context.Object);
        var request = new AddUserToPlanProcedureCommand()
        {
            PlanProcedureId = planProcedureId,
            UserIds = new List<int> { userId },
        };
        //When
        var result = await sut.Handle(request, new CancellationToken());

        //Then
        result.Exception.Should().BeOfType(typeof(BadRequestException));
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(19, 19)]
    [DataRow(35, 35)]
    public async Task RemoveUserFromPlanProcedureTests_PlanProcedureIdNotFound_ReturnsNotFound(int planProcedureId, int userId)
    {
        //Given
        var context = DbContextHelper.CreateContext();
        var sut = new AddUserToPlanProcedureCommandHandler(_logger, context);
        var request = new AddUserToPlanProcedureCommand()
        {
            PlanProcedureId = planProcedureId,
            UserIds = new List<int> { 1 },
        };

        context.UserPlanProcedure.Add(new Data.DataModels.UserPlanProcedure
        {
            PlanProcedureId = planProcedureId + 1,
            UserId = userId + 1
        });
        await context.SaveChangesAsync();

        //When
        var result = await sut.Handle(request, new CancellationToken());

        //Then
        result.Exception.Should().BeOfType(typeof(NotFoundException));
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(19)]
    [DataRow(35)]
    public async Task RemoveUserFromPlanProcedureTests_UserIdNotFound_ReturnsNotFound(int userId)
    {
        //Given
        var context = DbContextHelper.CreateContext();
        var sut = new AddUserToPlanProcedureCommandHandler(_logger, context);
        var request = new AddUserToPlanProcedureCommand()
        {
            PlanProcedureId = 1,
            UserIds = new List<int> { userId },
        };

        context.UserPlanProcedure.Add(new Data.DataModels.UserPlanProcedure
        {
            PlanProcedureId = 1,
            UserId = userId + 1
        });
        await context.SaveChangesAsync();

        //When
        var result = await sut.Handle(request, new CancellationToken());

        //Then
        result.Exception.Should().BeOfType(typeof(NotFoundException));
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(1, 1)]
    public async Task RemoveUserFromPlanProcedureTests_AlreadyContainsUserPlanProcedure_ReturnsSuccess(int planProcedureId, int userId)
    {
        //Given
        var context = DbContextHelper.CreateContext();
        var sut = new AddUserToPlanProcedureCommandHandler(_logger, context);
        var request = new AddUserToPlanProcedureCommand()
        {
            PlanProcedureId = planProcedureId,
            UserIds = new List<int> { userId },
        };

        int planId = 1, procedureId = 1;
        context.Plans.Add(new Data.DataModels.Plan
        {
            PlanId = planId
        });
        context.Procedures.Add(new Data.DataModels.Procedure
        {
            ProcedureId = procedureId,
            ProcedureTitle = "Test Procedure"
        });
        context.PlanProcedures.Add(new Data.DataModels.PlanProcedure
        {
            ProcedureId = procedureId,
            PlanId = planId
        });
        context.Users.Add(new Data.DataModels.User
        {
            UserId = userId,
        });

        context.UserPlanProcedure.Add(new Data.DataModels.UserPlanProcedure
        {
            PlanProcedureId = 1,
            UserId = userId
        });
        await context.SaveChangesAsync();

        //When
        var result = await sut.Handle(request, new CancellationToken());

        //Then
        result.Value.Should().BeOfType(typeof(Unit));
        result.Succeeded.Should().BeTrue();
    }
}
