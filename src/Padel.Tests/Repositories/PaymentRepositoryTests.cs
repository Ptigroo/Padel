using Microsoft.EntityFrameworkCore;
using Padel.Domain.Entities;
using Padel.Infrastructure.Data;
using Padel.Infrastructure.Repositories;
using DomainMatchType = Padel.Domain.Entities.MatchType;

namespace Padel.Tests.Repositories;

public class PaymentRepositoryTests : IDisposable
{
    private readonly PadelDbContext _context;
    private readonly PaymentRepository _repository;

    public PaymentRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PadelDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new PadelDbContext(options);
        _repository = new PaymentRepository(_context);
    }

    [Fact]
    public async Task GetByMatchAsync_ReturnsPaymentsForMatch()
    {
        // Arrange
        var site = new Site { Name = "Test Site", Address = "123 Test" };
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
            MatchType = DomainMatchType.Public
        };
        await _context.Matches.AddAsync(match);
        await _context.SaveChangesAsync();

        var matchPlayer = new MatchPlayer
        {
            MatchId = match.Id,
            MemberId = member.Id
        };
        await _context.MatchPlayers.AddAsync(matchPlayer);
        await _context.SaveChangesAsync();

        var payment = new Payment
        {
            MatchPlayerId = matchPlayer.Id,
            MatchId = match.Id,
            MemberId = member.Id,
            Amount = 20.00m,
            Status = PaymentStatus.Paid,
            CreatedAt = DateTime.Now,
            PaidAt = DateTime.Now
        };
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByMatchAsync(match.Id);
        var resultList = result.ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(payment.Id, resultList[0].Id);
        Assert.Equal(20.00m, resultList[0].Amount);
    }

    [Fact]
    public async Task GetByMemberAsync_ReturnsPaymentsForMember()
    {
        // Arrange
        var site = new Site { Name = "Test Site", Address = "123 Test" };
        await _context.Sites.AddAsync(site);
        await _context.SaveChangesAsync();

        var court = new Court { Name = "Court 1", SiteId = site.Id };
        await _context.Courts.AddAsync(court);
        await _context.SaveChangesAsync();

        var member1 = new Member
        {
            Matricule = "G0001",
            FirstName = "Member",
            LastName = "One",
            Email = "m1@test.com",
            MemberType = MemberType.Global
        };
        var member2 = new Member
        {
            Matricule = "G0002",
            FirstName = "Member",
            LastName = "Two",
            Email = "m2@test.com",
            MemberType = MemberType.Global
        };
        await _context.Members.AddRangeAsync(member1, member2);
        await _context.SaveChangesAsync();

        var match = new Match
        {
            CourtId = court.Id,
            OrganizerId = member1.Id,
            ScheduledAt = DateTime.Now.AddDays(1),
            EndsAt = DateTime.Now.AddDays(1).AddHours(1.5),
            MatchType = DomainMatchType.Public
        };
        await _context.Matches.AddAsync(match);
        await _context.SaveChangesAsync();

        var mp1 = new MatchPlayer { MatchId = match.Id, MemberId = member1.Id };
        var mp2 = new MatchPlayer { MatchId = match.Id, MemberId = member2.Id };
        await _context.MatchPlayers.AddRangeAsync(mp1, mp2);
        await _context.SaveChangesAsync();

        var payment1 = new Payment
        {
            MatchPlayerId = mp1.Id,
            MatchId = match.Id,
            MemberId = member1.Id,
            Amount = 20.00m,
            Status = PaymentStatus.Paid,
            CreatedAt = DateTime.Now
        };
        var payment2 = new Payment
        {
            MatchPlayerId = mp2.Id,
            MatchId = match.Id,
            MemberId = member2.Id,
            Amount = 20.00m,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.Now
        };
        await _context.Payments.AddRangeAsync(payment1, payment2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByMemberAsync(member1.Id);
        var resultList = result.ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(member1.Id, resultList[0].MemberId);
    }

    [Fact]
    public async Task CreateAsync_AddsPayment()
    {
        // Arrange
        var site = new Site { Name = "Test Site", Address = "123 Test" };
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
            MatchType = DomainMatchType.Public
        };
        await _context.Matches.AddAsync(match);
        await _context.SaveChangesAsync();

        var mp = new MatchPlayer { MatchId = match.Id, MemberId = member.Id };
        await _context.MatchPlayers.AddAsync(mp);
        await _context.SaveChangesAsync();

        var payment = new Payment
        {
            MatchPlayerId = mp.Id,
            MatchId = match.Id,
            MemberId = member.Id,
            Amount = 25.00m,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.Now
        };

        // Act
        var result = await _repository.CreateAsync(payment);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(25.00m, result.Amount);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesPayment()
    {
        // Arrange
        var site = new Site { Name = "Test Site", Address = "123 Test" };
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
            MatchType = DomainMatchType.Public
        };
        await _context.Matches.AddAsync(match);
        await _context.SaveChangesAsync();

        var mp = new MatchPlayer { MatchId = match.Id, MemberId = member.Id };
        await _context.MatchPlayers.AddAsync(mp);
        await _context.SaveChangesAsync();

        var payment = new Payment
        {
            MatchPlayerId = mp.Id,
            MatchId = match.Id,
            MemberId = member.Id,
            Amount = 20.00m,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.Now
        };
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();

        // Act
        payment.Status = PaymentStatus.Paid;
        payment.PaidAt = DateTime.Now;
        var result = await _repository.UpdateAsync(payment);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PaymentStatus.Paid, result.Status);
        Assert.NotNull(result.PaidAt);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
