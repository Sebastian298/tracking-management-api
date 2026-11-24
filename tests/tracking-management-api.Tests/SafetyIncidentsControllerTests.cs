using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using tracking_management_api.Controllers;
using tracking_management_api.Dtos;
using tracking_management_api.Infrastructure.Messaging;
using tracking_management_api.Services.SafetyIncidents;

namespace tracking_management_api.Tests;

public sealed class SafetyIncidentsControllerTests
{
    [Fact]
    public async Task CreateDirectAsync_NotifiesInlineAndReturns201()
    {
        var request = SafetyIncidentFixtures.CreateRequest();
        var dto = SafetyIncidentFixtures.CreateDto("INC00000171");
        var (controller, service, inlineNotifier, rabbitNotifier) = CreateController();
        service.RegisterAsync(request, Arg.Any<CancellationToken>()).Returns(dto);
        inlineNotifier.NotifyAsync(dto, Arg.Any<CancellationToken>()).Returns(Guid.Empty);

        var result = await controller.CreateDirectAsync(request, CancellationToken.None);

        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, statusResult.StatusCode);
        Assert.Same(dto, statusResult.Value);
        Assert.Equal("inline", controller.Response.Headers["X-Processing"].ToString());
        Assert.False(string.IsNullOrWhiteSpace(controller.Response.Headers["X-Elapsed-Ms"].ToString()));
        await inlineNotifier.Received(1).NotifyAsync(dto, Arg.Any<CancellationToken>());
        await rabbitNotifier.DidNotReceive().NotifyAsync(Arg.Any<SafetyIncidentDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateQueuedAsync_PublishesAndReturns202()
    {
        var request = SafetyIncidentFixtures.CreateRequest();
        var dto = SafetyIncidentFixtures.CreateDto("INC00000171");
        var messageId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var (controller, service, inlineNotifier, rabbitNotifier) = CreateController();
        service.RegisterAsync(request, Arg.Any<CancellationToken>()).Returns(dto);
        rabbitNotifier.NotifyAsync(dto, Arg.Any<CancellationToken>()).Returns(messageId);

        var result = await controller.CreateQueuedAsync(request, CancellationToken.None);

        var accepted = Assert.IsType<AcceptedResult>(result.Result);
        Assert.Same(dto, accepted.Value);
        Assert.Equal("queued", controller.Response.Headers["X-Processing"].ToString());
        Assert.Equal(messageId.ToString(), controller.Response.Headers["X-Message-Id"].ToString());
        await rabbitNotifier.Received(1).NotifyAsync(dto, Arg.Any<CancellationToken>());
        await inlineNotifier.DidNotReceive().NotifyAsync(Arg.Any<SafetyIncidentDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateQueuedAsync_WhenPublishFails_Returns503()
    {
        var request = SafetyIncidentFixtures.CreateRequest();
        var dto = SafetyIncidentFixtures.CreateDto("INC00000171");
        var (controller, service, _, rabbitNotifier) = CreateController();
        service.RegisterAsync(request, Arg.Any<CancellationToken>()).Returns(dto);
        rabbitNotifier
            .NotifyAsync(dto, Arg.Any<CancellationToken>())
            .ThrowsAsync(new SafetyIncidentPublishException("broker down", new InvalidOperationException("down")));

        var result = await controller.CreateQueuedAsync(request, CancellationToken.None);

        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusResult.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(statusResult.Value);
        Assert.Equal("Message broker unavailable", problem.Title);
        Assert.Equal("queued", controller.Response.Headers["X-Processing"].ToString());
    }

    private static (
        SafetyIncidentsController Controller,
        ISafetyIncidentService Service,
        ISafetyIncidentNotifier InlineNotifier,
        ISafetyIncidentNotifier RabbitNotifier) CreateController()
    {
        var service = Substitute.For<ISafetyIncidentService>();
        var inlineNotifier = Substitute.For<ISafetyIncidentNotifier>();
        var rabbitNotifier = Substitute.For<ISafetyIncidentNotifier>();
        var controller = new SafetyIncidentsController(
            service,
            inlineNotifier,
            rabbitNotifier,
            NullLogger<SafetyIncidentsController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return (controller, service, inlineNotifier, rabbitNotifier);
    }
}
