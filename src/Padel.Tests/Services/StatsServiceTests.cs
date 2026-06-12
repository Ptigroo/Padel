using Moq;
using Padel.Application.DTOs;
using Padel.Application.Interfaces;
using Padel.Application.Services;
using DomainMatch = Padel.Domain.Entities.Match;
using Padel.Domain.Entities;
using DomainMatchType = Padel.Domain.Entities.MatchType;

namespace Padel.Tests.Services;

public class StatsServiceTests
{
    private readonly Mock<IMatchRepository> _matchRepositoryMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<ISiteRepository> _siteRepositoryMock;
    private readonly Mock<IMemberRepository> _memberRepositoryMock;
    private readonly StatsService _service;

    public StatsServiceTests()
    {
        _matchRepositoryMock = new Mock<IMatchRepository>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _siteRepositoryMock = new Mock<ISiteRepository>();
        _memberRepositoryMock = new Mock<IMemberRepository>();
        _service = new StatsService(
            _siteRepositoryMock.Object,
            _matchRepositoryMock.Object,
            _paymentRepositoryMock.Object,
            _memberRepositoryMock.Object
        );
    }

    [Fact]
    public async Task GetGlobalStatsAsync_ReturnsCorrectStats()
    {
        // Arrange
        var sites = new List<Site>
        {
            new() { Id = 1, Name = "Site 1", Address = "111" }
        };

        var members = new List<Member>
        {
            new() { Id = 1, Matricule = "G0001", FirstName = "John", LastName = "Doe", Email = "john@test.com", MemberType = MemberType.Global }
        };

        var matches = new List<DomainMatch>
        {
            new() { Id = 1, ScheduledAt = DateTime.Now.AddDays(1), MatchType = DomainMatchType.Public, Status = MatchStatus.Scheduled },
            new() { Id = 2, ScheduledAt = DateTime.Now.AddDays(1), MatchType = DomainMatchType.Private, Status = MatchStatus.Full },
            new() { Id = 3, ScheduledAt = DateTime.Now.AddDays(1), MatchType = DomainMatchType.Public, Status = MatchStatus.Completed }
        };

        _siteRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(sites);
        _memberRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(members);
        _matchRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(matches);
        _paymentRepositoryMock.Setup(r => r.GetTotalRevenueAsync()).ReturnsAsync(45.00m);

        // Act
        var result = await _service.GetGlobalStatsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalSites);
        Assert.Equal(1, result.TotalMembers);
        Assert.Equal(3, result.TotalMatches);
        Assert.Equal(45.00m, result.TotalRevenue);
        Assert.Equal(1, result.MatchesScheduled);
        Assert.Equal(1, result.MatchesFull);
        Assert.Equal(1, result.MatchesCompleted);
        Assert.Equal(0, result.MatchesCancelled);
    }

    [Fact]
    public async Task GetSiteStatsAsync_ReturnsStatsForSite()
    {
        // Arrange
        var siteId = 1;
        var site = new Site 
        { 
            Id = siteId, 
            Name = "Test Site", 
            Address = "123 Test",
            Courts = new List<Court> 
            { 
                new() { Id = 1, Name = "Court 1", SiteId = siteId },
                new() { Id = 2, Name = "Court 2", SiteId = siteId }
            }
        };

        var matches = new List<DomainMatch>
        {
            new() { Id = 1, ScheduledAt = DateTime.Now.AddDays(1), Court = new Court { Name = "Court 1", SiteId = siteId }, Status = MatchStatus.Scheduled },
            new() { Id = 2, ScheduledAt = DateTime.Now.AddDays(1), Court = new Court { Name = "Court 2", SiteId = siteId }, Status = MatchStatus.Full }
        };

        var members = new List<Member>
        {
            new() { Id = 1, Matricule = "S00001", FirstName = "John", LastName = "Doe", Email = "john@test.com", MemberType = MemberType.Site, SiteId = siteId }
        };

        _siteRepositoryMock.Setup(r => r.GetByIdAsync(siteId)).ReturnsAsync(site);
        _matchRepositoryMock.Setup(r => r.GetBySiteAsync(siteId)).ReturnsAsync(matches);
        _memberRepositoryMock.Setup(r => r.GetBySiteIdAsync(siteId)).ReturnsAsync(members);
        _paymentRepositoryMock.Setup(r => r.GetRevenueByEsiteAsync(siteId)).ReturnsAsync(45.00m);

        // Act
        var result = await _service.GetSiteStatsAsync(siteId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(siteId, result.SiteId);
        Assert.Equal("Test Site", result.SiteName);
        Assert.Equal(2, result.TotalCourts);
        Assert.Equal(1, result.TotalMembers);
        Assert.Equal(2, result.TotalMatches);
        Assert.Equal(45.00m, result.Revenue);
        Assert.Equal(1, result.MatchesScheduled);
        Assert.Equal(1, result.MatchesFull);
    }

    [Fact]
    public async Task GetAllSiteStatsAsync_ReturnsStatsForAllSites()
    {
        // Arrange
        var sites = new List<Site>
        {
            new() { Id = 1, Name = "Site 1", Address = "111", Courts = new List<Court> { new() { Id = 1, Name = "Court 1" } } },
            new() { Id = 2, Name = "Site 2", Address = "222", Courts = new List<Court> { new() { Id = 2, Name = "Court 2" } } }
        };

        var matches1 = new List<DomainMatch>
        {
            new() { Id = 1, ScheduledAt = DateTime.Now.AddDays(1), Court = new Court { Name = "C1", SiteId = 1 }, Status = MatchStatus.Scheduled }
        };

        var matches2 = new List<DomainMatch>
        {
            new() { Id = 2, ScheduledAt = DateTime.Now.AddDays(1), Court = new Court { Name = "C2", SiteId = 2 }, Status = MatchStatus.Completed }
        };

        var members1 = new List<Member>
        {
            new() { Id = 1, Matricule = "S00001", FirstName = "John", LastName = "Doe", Email = "john@test.com", MemberType = MemberType.Site, SiteId = 1 }
        };

        var members2 = new List<Member>
        {
            new() { Id = 2, Matricule = "S00002", FirstName = "Jane", LastName = "Smith", Email = "jane@test.com", MemberType = MemberType.Site, SiteId = 2 }
        };

        _siteRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(sites);
        _siteRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(sites[0]);
        _siteRepositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(sites[1]);
        _matchRepositoryMock.Setup(r => r.GetBySiteAsync(1)).ReturnsAsync(matches1);
        _matchRepositoryMock.Setup(r => r.GetBySiteAsync(2)).ReturnsAsync(matches2);
        _memberRepositoryMock.Setup(r => r.GetBySiteIdAsync(1)).ReturnsAsync(members1);
        _memberRepositoryMock.Setup(r => r.GetBySiteIdAsync(2)).ReturnsAsync(members2);
        _paymentRepositoryMock.Setup(r => r.GetRevenueByEsiteAsync(1)).ReturnsAsync(20.00m);
        _paymentRepositoryMock.Setup(r => r.GetRevenueByEsiteAsync(2)).ReturnsAsync(30.00m);

        // Act
        var result = await _service.GetAllSiteStatsAsync();

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal(20.00m, resultList[0].Revenue);
        Assert.Equal(30.00m, resultList[1].Revenue);
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
