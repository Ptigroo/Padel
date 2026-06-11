using Microsoft.AspNetCore.Mvc;
using Moq;
using Padel.Api.Controllers;
using Padel.Application.DTOs;
using Padel.Application.Interfaces;

namespace Padel.Tests.Controllers;

public class StatsControllerTests
{
    private readonly Mock<IStatsService> _statsServiceMock;
    private readonly StatsController _controller;

    public StatsControllerTests()
    {
        _statsServiceMock = new Mock<IStatsService>();
        _controller = new StatsController(_statsServiceMock.Object);
    }

    [Fact]
    public async Task GetGlobalStats_ReturnsOkWithStats()
    {
        // Arrange
        var stats = new GlobalStatsDto
        {
            TotalMatches = 10,
            TotalPayments = 25,
            TotalRevenuePaid = 500.00m,
            TotalRevenueExpected = 600.00m
        };
        _statsServiceMock.Setup(s => s.GetGlobalStatsAsync()).ReturnsAsync(stats);

        // Act
        var result = await _controller.GetGlobalStats();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedStats = Assert.IsType<GlobalStatsDto>(okResult.Value);
        Assert.Equal(10, returnedStats.TotalMatches);
        Assert.Equal(500.00m, returnedStats.TotalRevenuePaid);
    }

    [Fact]
    public async Task GetSiteStats_ReturnsOkWithStats_WhenSiteExists()
    {
        // Arrange
        var siteId = 1;
        var stats = new SiteStatsDto
        {
            SiteId = siteId,
            SiteName = "Test Site",
            TotalMatches = 5,
            TotalPayments = 10,
            TotalRevenuePaid = 200.00m,
            TotalRevenueExpected = 250.00m
        };
        _statsServiceMock.Setup(s => s.GetSiteStatsAsync(siteId)).ReturnsAsync(stats);

        // Act
        var result = await _controller.GetSiteStats(siteId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedStats = Assert.IsType<SiteStatsDto>(okResult.Value);
        Assert.Equal("Test Site", returnedStats.SiteName);
        Assert.Equal(5, returnedStats.TotalMatches);
    }

    [Fact]
    public async Task GetSiteStats_ReturnsNotFound_WhenSiteDoesNotExist()
    {
        // Arrange
        var siteId = 999;
        _statsServiceMock.Setup(s => s.GetSiteStatsAsync(siteId)).ReturnsAsync((SiteStatsDto?)null);

        // Act
        var result = await _controller.GetSiteStats(siteId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetAllSiteStats_ReturnsOkWithListOfStats()
    {
        // Arrange
        var stats = new List<SiteStatsDto>
        {
            new() { SiteId = 1, SiteName = "Site 1", TotalMatches = 5, TotalPayments = 10, TotalRevenuePaid = 200.00m, TotalRevenueExpected = 250.00m },
            new() { SiteId = 2, SiteName = "Site 2", TotalMatches = 3, TotalPayments = 6, TotalRevenuePaid = 120.00m, TotalRevenueExpected = 150.00m }
        };
        _statsServiceMock.Setup(s => s.GetAllSiteStatsAsync()).ReturnsAsync(stats);

        // Act
        var result = await _controller.GetAllSiteStats();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedStats = Assert.IsType<List<SiteStatsDto>>(okResult.Value);
        Assert.Equal(2, returnedStats.Count);
        Assert.Equal("Site 1", returnedStats[0].SiteName);
        Assert.Equal("Site 2", returnedStats[1].SiteName);
    }
}
