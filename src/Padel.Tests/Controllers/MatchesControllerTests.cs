using Microsoft.AspNetCore.Mvc;
using Moq;
using Padel.Api.Controllers;
using Padel.Application.DTOs;
using Padel.Application.Interfaces;

namespace Padel.Tests.Controllers;

public class MatchesControllerTests
{
    private readonly Mock<IMatchService> _matchService = new();

    private MatchesController CreateController() => new(_matchService.Object);

    private static MatchDto CreateMatchDto(int id = 1) => new()
    {
        Id = id, CourtId = 1, CourtName = "T1", SiteId = 1, SiteName = "S1",
        OrganizerId = 1, OrganizerMatricule = "G0001", OrganizerName = "Jean D",
        ScheduledAt = DateTime.Today.AddDays(1).AddHours(10),
        EndsAt = DateTime.Today.AddDays(1).AddHours(11).AddMinutes(30),
        MatchType = "Private", Status = "Scheduled", PlayerCount = 1
    };

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _matchService.Setup(s => s.GetAllAsync()).ReturnsAsync(new[] { CreateMatchDto() });

        var controller = CreateController();
        var result = await controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetById_Existing_ReturnsOk()
    {
        _matchService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(CreateMatchDto());

        var controller = CreateController();
        var result = await controller.GetById(1);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        _matchService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((MatchDto?)null);

        var controller = CreateController();
        var result = await controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_Nominal_Returns201()
    {
        var dto = new CreateMatchDto
        {
            CourtId = 1, OrganizerMatricule = "G0001",
            ScheduledAt = DateTime.Today.AddDays(1).AddHours(10), MatchType = "Private"
        };
        _matchService.Setup(s => s.CreateAsync(dto)).ReturnsAsync(CreateMatchDto());

        var controller = CreateController();
        var result = await controller.Create(dto);

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task Create_InvalidOperation_Returns400()
    {
        var dto = new CreateMatchDto
        {
            CourtId = 1, OrganizerMatricule = "G0001",
            ScheduledAt = DateTime.Today.AddDays(1).AddHours(10), MatchType = "Private"
        };
        _matchService.Setup(s => s.CreateAsync(dto))
            .ThrowsAsync(new InvalidOperationException("Blocked"));

        var controller = CreateController();
        var result = await controller.Create(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task Join_InvalidOperation_Returns400()
    {
        _matchService.Setup(s => s.JoinAsync(1, "G0002"))
            .ThrowsAsync(new InvalidOperationException("Cannot join"));

        var controller = CreateController();
        var result = await controller.Join(1, "G0002");

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task ProcessDayBefore_ReturnsOk()
    {
        _matchService.Setup(s => s.ProcessDayBeforeAsync()).Returns(Task.CompletedTask);

        var controller = CreateController();
        var result = await controller.ProcessDayBefore();

        Assert.IsType<OkObjectResult>(result);
    }
}
