using Moq;
using Padel.Application.DTOs;
using Padel.Application.Interfaces;
using Padel.Application.Services;
using DomainMatch = Padel.Domain.Entities.Match;
using Padel.Domain.Entities;

namespace Padel.Tests.Services;

public class StatsServiceTests
{
    private readonly Mock<IMatchRepository> _matchRepositoryMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<ISiteRepository> _siteRepositoryMock;
    private readonly StatsService _service;

    public StatsServiceTests()
    {
        _matchRepositoryMock = new Mock<IMatchRepository>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _siteRepositoryMock = new Mock<ISiteRepository>();
        _service = new StatsService(
            _matchRepositoryMock.Object,
            _paymentRepositoryMock.Object,
            _siteRepositoryMock.Object
        );
    }

    [Fact]
    public async Task GetGlobalStatsAsync_ReturnsCorrectStats()
    {
        // Arrange
        var matches = new List<DomainMatch>
        {
            new() { Id = 1, ScheduledAt = DateTime.Now.AddDays(1), MatchType = MatchType.Public },
            new() { Id = 2, ScheduledAt = DateTime.Now.AddDays(1), MatchType = MatchType.Private },
            new() { Id = 3, ScheduledAt = DateTime.Now.AddDays(1), MatchType = MatchType.Public }
        };

        var payments = new List<Payment>
        {
            new() { MatchId = 1, Amount = 20.00m, Status = PaymentStatus.Paid },
            new() { MatchId = 2, Amount = 25.00m, Status = PaymentStatus.Paid },
            new() { MatchId = 3, Amount = 15.00m, Status = PaymentStatus.Pending }
        };

        _matchRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(matches);
        _paymentRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(payments);

        // Act
        var result = await _service.GetGlobalStatsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalMatches);
        Assert.Equal(3, result.TotalPayments);
        Assert.Equal(45.00m, result.TotalRevenuePaid);
        Assert.Equal(60.00m, result.TotalRevenueExpected);
    }

    [Fact]
    public async Task GetSiteStatsAsync_ReturnsStatsForSite()
    {
        // Arrange
        var siteId = 1;
        var site = new Site { Id = siteId, Name = "Test Site", Address = "123 Test" };

        var matches = new List<DomainMatch>
        {
            new() { Id = 1, ScheduledAt = DateTime.Now.AddDays(1), Court = new Court { Name = "Court 1", SiteId = siteId } },
            new() { Id = 2, ScheduledAt = DateTime.Now.AddDays(1), Court = new Court { Name = "Court 2", SiteId = siteId } }
        };

        var payments = new List<Payment>
        {
            new() { MatchId = 1, Amount = 20.00m, Status = PaymentStatus.Paid },
            new() { MatchId = 2, Amount = 25.00m, Status = PaymentStatus.Paid }
        };

        _siteRepositoryMock.Setup(r => r.GetByIdAsync(siteId)).ReturnsAsync(site);
        _matchRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(matches);
        _paymentRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(payments);

        // Act
        var result = await _service.GetSiteStatsAsync(siteId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(siteId, result.SiteId);
        Assert.Equal("Test Site", result.SiteName);
        Assert.Equal(2, result.TotalMatches);
        Assert.Equal(2, result.TotalPayments);
        Assert.Equal(45.00m, result.TotalRevenuePaid);
    }

    [Fact]
    public async Task GetAllSiteStatsAsync_ReturnsStatsForAllSites()
    {
        // Arrange
        var sites = new List<Site>
        {
            new() { Id = 1, Name = "Site 1", Address = "111" },
            new() { Id = 2, Name = "Site 2", Address = "222" }
        };

        var matches1 = new List<DomainMatch>
        {
            new() { Id = 1, ScheduledAt = DateTime.Now.AddDays(1), Court = new Court { Name = "C1", SiteId = 1 } }
        };

        var matches2 = new List<DomainMatch>
        {
            new() { Id = 2, ScheduledAt = DateTime.Now.AddDays(1), Court = new Court { Name = "C2", SiteId = 2 } }
        };

        var allMatches = new List<DomainMatch>();
        allMatches.AddRange(matches1);
        allMatches.AddRange(matches2);

        var payments1 = new List<Payment>
        {
            new() { MatchId = 1, Amount = 20.00m, Status = PaymentStatus.Paid }
        };

        var payments2 = new List<Payment>
        {
            new() { MatchId = 2, Amount = 30.00m, Status = PaymentStatus.Paid }
        };

        var allPayments = new List<Payment>();
        allPayments.AddRange(payments1);
        allPayments.AddRange(payments2);

        _siteRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(sites);
        _matchRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(allMatches);
        _paymentRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(allPayments);

        // Act
        var result = await _service.GetAllSiteStatsAsync();

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal(20.00m, resultList[0].TotalRevenuePaid);
        Assert.Equal(30.00m, resultList[1].TotalRevenuePaid);
    }

    [Fact]
    public async Task GetSiteStatsAsync_ReturnsNull_WhenSiteNotFound()
    {
        // Arrange
        _siteRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Site?)null);

        // Act
        var result = await _service.GetSiteStatsAsync(999);

        // Assert
        Assert.Null(result);
    }
}
