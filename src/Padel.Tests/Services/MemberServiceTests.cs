using Moq;
using Padel.Application.DTOs;
using Padel.Application.Interfaces;
using Padel.Application.Services;
using Padel.Domain.Entities;

namespace Padel.Tests.Services;

public class MemberServiceTests
{
    private readonly Mock<IMemberRepository> _memberRepo = new();
    private readonly Mock<ISiteRepository> _siteRepo = new();

    private MemberService CreateService() => new(_memberRepo.Object, _siteRepo.Object);

    // ═══════════════════════════════════════
    // Génération matricule (CF-RC-001)
    // ═══════════════════════════════════════

    [Fact]
    public async Task CreateAsync_GlobalMember_GeneratesGMatricule()
    {
        _memberRepo.Setup(r => r.GetLastMatriculeByTypeAsync(MemberType.Global)).ReturnsAsync((string?)null);
        _memberRepo.Setup(r => r.CreateAsync(It.IsAny<Member>()))
            .ReturnsAsync((Member m) => { m.Id = 1; return m; });

        var service = CreateService();
        var dto = new CreateMemberDto
        {
            FirstName = "Jean", LastName = "Dupont",
            Email = "j@t.com", MemberType = "Global"
        };

        var result = await service.CreateAsync(dto);

        Assert.StartsWith("G", result.Matricule);
        Assert.Equal("G0001", result.Matricule);
    }

    [Fact]
    public async Task CreateAsync_SiteMember_GeneratesSMatricule()
    {
        _memberRepo.Setup(r => r.GetLastMatriculeByTypeAsync(MemberType.Site)).ReturnsAsync("S00003");
        _siteRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Site { Id = 1, Name = "S1", Address = "A1" });
        _memberRepo.Setup(r => r.CreateAsync(It.IsAny<Member>()))
            .ReturnsAsync((Member m) => { m.Id = 1; return m; });

        var service = CreateService();
        var dto = new CreateMemberDto
        {
            FirstName = "Marie", LastName = "Martin",
            Email = "m@t.com", MemberType = "Site", SiteId = 1
        };

        var result = await service.CreateAsync(dto);

        Assert.Equal("S00004", result.Matricule);
    }

    [Fact]
    public async Task CreateAsync_LibreMember_GeneratesLMatricule()
    {
        _memberRepo.Setup(r => r.GetLastMatriculeByTypeAsync(MemberType.Libre)).ReturnsAsync("L00010");
        _memberRepo.Setup(r => r.CreateAsync(It.IsAny<Member>()))
            .ReturnsAsync((Member m) => { m.Id = 1; return m; });

        var service = CreateService();
        var dto = new CreateMemberDto
        {
            FirstName = "Luc", LastName = "Libre",
            Email = "l@t.com", MemberType = "Libre"
        };

        var result = await service.CreateAsync(dto);

        Assert.Equal("L00011", result.Matricule);
    }

    // ═══════════════════════════════════════
    // Validation — Site sans SiteId (CF-RV-010)
    // ═══════════════════════════════════════

    [Fact]
    public async Task CreateAsync_SiteMemberWithoutSiteId_ThrowsException()
    {
        var service = CreateService();
        var dto = new CreateMemberDto
        {
            FirstName = "X", LastName = "Y",
            Email = "x@t.com", MemberType = "Site"
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
        Assert.Contains("SiteId", ex.Message);
    }

    // ═══════════════════════════════════════
    // Validation — Global avec SiteId (CF-RV-011)
    // ═══════════════════════════════════════

    [Fact]
    public async Task CreateAsync_GlobalMemberWithSiteId_ThrowsException()
    {
        var service = CreateService();
        var dto = new CreateMemberDto
        {
            FirstName = "X", LastName = "Y",
            Email = "x@t.com", MemberType = "Global", SiteId = 1
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
        Assert.Contains("SiteId", ex.Message);
    }

    // ═══════════════════════════════════════
    // Validation — Site inexistant (CF-RV-012)
    // ═══════════════════════════════════════

    [Fact]
    public async Task CreateAsync_SiteMemberWithInvalidSite_ThrowsException()
    {
        _siteRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Site?)null);

        var service = CreateService();
        var dto = new CreateMemberDto
        {
            FirstName = "X", LastName = "Y",
            Email = "x@t.com", MemberType = "Site", SiteId = 999
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
