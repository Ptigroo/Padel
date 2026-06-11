using Microsoft.EntityFrameworkCore;
using Padel.Domain.Entities;
using Padel.Infrastructure.Data;
using Padel.Infrastructure.Repositories;

namespace Padel.Tests.Repositories;

public class MemberRepositoryTests : IDisposable
{
    private readonly PadelDbContext _context;
    private readonly MemberRepository _repository;

    public MemberRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PadelDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new PadelDbContext(options);
        _repository = new MemberRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllMembers()
    {
        // Arrange
        var member1 = new Member
        {
            Matricule = "G0001",
            FirstName = "Test",
            LastName = "One",
            Email = "test1@test.com",
            MemberType = MemberType.Global
        };
        var member2 = new Member
        {
            Matricule = "G0002",
            FirstName = "Test",
            LastName = "Two",
            Email = "test2@test.com",
            MemberType = MemberType.Global
        };
        await _context.Members.AddRangeAsync(member1, member2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByMatriculeAsync_ReturnsCorrectMember()
    {
        // Arrange
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

        // Act
        var result = await _repository.GetByMatriculeAsync("G0001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("G0001", result.Matricule);
        Assert.Equal("Test", result.FirstName);
    }

    [Fact]
    public async Task GetBySiteIdAsync_ReturnsSiteMembers()
    {
        // Arrange
        var site = new Site { Name = "Test Site", Address = "123 Test" };
        await _context.Sites.AddAsync(site);
        await _context.SaveChangesAsync();

        var globalMember = new Member
        {
            Matricule = "G0001",
            FirstName = "Global",
            LastName = "Member",
            Email = "global@test.com",
            MemberType = MemberType.Global
        };
        var siteMember = new Member
        {
            Matricule = "S00001",
            FirstName = "Site",
            LastName = "Member",
            Email = "site@test.com",
            MemberType = MemberType.Site,
            SiteId = site.Id
        };
        await _context.Members.AddRangeAsync(globalMember, siteMember);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySiteIdAsync(site.Id);

        // Assert
        Assert.Single(result);
        Assert.Equal("S00001", result[0].Matricule);
    }

    [Fact]
    public async Task CreateAsync_AddsMember()
    {
        // Arrange
        var member = new Member
        {
            Matricule = "G0001",
            FirstName = "New",
            LastName = "Member",
            Email = "new@test.com",
            MemberType = MemberType.Global
        };

        // Act
        var result = await _repository.CreateAsync(member);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("G0001", result.Matricule);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesMember()
    {
        // Arrange
        var member = new Member
        {
            Matricule = "G0001",
            FirstName = "Original",
            LastName = "Name",
            Email = "original@test.com",
            MemberType = MemberType.Global
        };
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();

        // Act
        member.FirstName = "Updated";
        member.Email = "updated@test.com";
        var result = await _repository.UpdateAsync(member);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated", result.FirstName);
        Assert.Equal("updated@test.com", result.Email);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
