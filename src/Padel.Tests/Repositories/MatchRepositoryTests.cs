using Microsoft.EntityFrameworkCore;
using Padel.Domain.Entities;
using Padel.Infrastructure.Data;
using Padel.Infrastructure.Repositories;

namespace Padel.Tests.Repositories;

public class MatchRepositoryTests : IDisposable
{
    private readonly PadelDbContext _context;
    private readonly MatchRepository _repository;

    public MatchRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PadelDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new PadelDbContext(options);
        _repository = new MatchRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllMatches()
    {
        // Arrange
        var site = new Site { Name = "Test Site", Address = "123 Test St" };
        await _context.Sites.AddAsync(site);
        await _context.SaveChangesAsync();

        var court = new Court { Name = "Court 1", SiteId = site.Id };
        await _context.Courts.AddAsync(court);
        await _context.SaveChangesAsync();

        var member = new Member 
        {
            Matricule = "G0001",
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            MemberType = MemberType.Global
        };
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        var match1 = new Match
        {
            CourtId = court.Id,
            OrganizerId = member.Id,
            ScheduledAt = DateTime.Now.AddDays(1),
            EndsAt = DateTime.Now.AddDays(1).AddHours(1.5),
            MatchType = MatchType.Public,
            Status = MatchStatus.Scheduled
        };
        var match2 = new Match
        {
            CourtId = court.Id,
            OrganizerId = member.Id,
            ScheduledAt = DateTime.Now.AddDays(2),
            EndsAt = DateTime.Now.AddDays(2).AddHours(1.5),
            MatchType = MatchType.Private,
            Status = MatchStatus.Full
        };
        await _context.Matches.AddRangeAsync(match1, match2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectMatch()
    {
        // Arrange
        var site = new Site { Name = "Test Site", Address = "123 Test St" };
        await _context.Sites.AddAsync(site);
        await _context.SaveChangesAsync();

        var court = new Court { Name = "Court 1", SiteId = site.Id };
        await _context.Courts.AddAsync(court);
        await _context.SaveChangesAsync();

        var member = new Member
        {
            Matricule = "G0001",
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            MemberType = MemberType.Global
        };
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        var match = new Match
        {
            CourtId = court.Id,
            OrganizerId = member.Id,
            ScheduledAt = DateTime.Now.AddDays(1),
            EndsAt = DateTime.Now.AddDays(1).AddHours(1.5),
            MatchType = MatchType.Public,
            Status = MatchStatus.Scheduled
        };
        await _context.Matches.AddAsync(match);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(match.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(match.Id, result.Id);
        Assert.NotNull(result.Court);
        Assert.NotNull(result.Organizer);
    }

    [Fact]
    public async Task GetBySiteIdAsync_ReturnsMatchesForSite()
    {
        // Arrange
        var site1 = new Site { Name = "Site 1", Address = "111 Test" };
        var site2 = new Site { Name = "Site 2", Address = "222 Test" };
        await _context.Sites.AddRangeAsync(site1, site2);
        await _context.SaveChangesAsync();

        var court1 = new Court { Name = "Court 1", SiteId = site1.Id };
        var court2 = new Court { Name = "Court 2", SiteId = site2.Id };
        await _context.Courts.AddRangeAsync(court1, court2);
        await _context.SaveChangesAsync();

        var member = new Member
        {
            Matricule = "G0001",
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            MemberType = MemberType.Global
        };
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        var match1 = new Match
        {
            CourtId = court1.Id,
            OrganizerId = member.Id,
            ScheduledAt = DateTime.Now.AddDays(1),
            EndsAt = DateTime.Now.AddDays(1).AddHours(1.5),
            MatchType = MatchType.Public
        };
        var match2 = new Match
        {
            CourtId = court2.Id,
            OrganizerId = member.Id,
            ScheduledAt = DateTime.Now.AddDays(1),
            EndsAt = DateTime.Now.AddDays(1).AddHours(1.5),
            MatchType = MatchType.Public
        };
        await _context.Matches.AddRangeAsync(match1, match2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySiteIdAsync(site1.Id);

        // Assert
        Assert.Single(result);
        Assert.Equal(court1.Id, result[0].CourtId);
    }

    [Fact]
    public async Task CreateAsync_AddsMatch()
    {
        // Arrange
        var site = new Site { Name = "Test Site", Address = "123 Test St" };
        await _context.Sites.AddAsync(site);
        await _context.SaveChangesAsync();

        var court = new Court { Name = "Court 1", SiteId = site.Id };
        await _context.Courts.AddAsync(court);
        await _context.SaveChangesAsync();

        var member = new Member
        {
            Matricule = "G0001",
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            MemberType = MemberType.Global
        };
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        var newMatch = new Match
        {
            CourtId = court.Id,
            OrganizerId = member.Id,
            ScheduledAt = DateTime.Now.AddDays(1),
            EndsAt = DateTime.Now.AddDays(1).AddHours(1.5),
            MatchType = MatchType.Public,
            Status = MatchStatus.Scheduled
        };

        // Act
        var result = await _repository.CreateAsync(newMatch);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(1, await _context.Matches.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_RemovesMatch()
    {
        // Arrange
        var site = new Site { Name = "Test Site", Address = "123 Test St" };
        await _context.Sites.AddAsync(site);
        await _context.SaveChangesAsync();

        var court = new Court { Name = "Court 1", SiteId = site.Id };
        await _context.Courts.AddAsync(court);
        await _context.SaveChangesAsync();

        var member = new Member
        {
            Matricule = "G0001",
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            MemberType = MemberType.Global
        };
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        var match = new Match
        {
            CourtId = court.Id,
            OrganizerId = member.Id,
            ScheduledAt = DateTime.Now.AddDays(1),
            EndsAt = DateTime.Now.AddDays(1).AddHours(1.5),
            MatchType = MatchType.Public
        };
        await _context.Matches.AddAsync(match);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(match.Id);

        // Assert
        Assert.Equal(0, await _context.Matches.CountAsync());
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
