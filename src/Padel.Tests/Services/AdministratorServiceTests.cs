using Moq;
using Padel.Application.DTOs;
using Padel.Application.Interfaces;
using Padel.Application.Services;
using Padel.Domain.Entities;

namespace Padel.Tests.Services;

public class AdministratorServiceTests
{
    private readonly Mock<IAdministratorRepository> _adminRepositoryMock;
    private readonly Mock<ISiteRepository> _siteRepositoryMock;
    private readonly AdministratorService _service;

    public AdministratorServiceTests()
    {
        _adminRepositoryMock = new Mock<IAdministratorRepository>();
        _siteRepositoryMock = new Mock<ISiteRepository>();
        _service = new AdministratorService(_adminRepositoryMock.Object, _siteRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_GlobalAdmin_CreatesWithoutSite()
    {
        // Arrange
        var dto = new CreateAdministratorDto
        {
            FirstName = "Admin",
            LastName = "Global",
            Email = "admin@test.com",
            Type = AdministratorType.Global,
            SiteId = null
        };

        _adminRepositoryMock.Setup(r => r.GetNextUsernameAsync(AdministratorType.Global))
            .ReturnsAsync("AG0001");

        _adminRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Administrator>()))
            .ReturnsAsync((Administrator a) =>
            {
                a.Id = 1;
                return a;
            });

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("AG0001", result.Username);
        Assert.Equal("Global", result.Type);
        Assert.Null(result.SiteId);
    }

    [Fact]
    public async Task CreateAsync_SiteAdmin_RequiresSiteId()
    {
        // Arrange
        var dto = new CreateAdministratorDto
        {
            FirstName = "Admin",
            LastName = "Site",
            Email = "admin@test.com",
            Type = AdministratorType.Site,
            SiteId = null // Missing!
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.CreateAsync(dto)
        );
        Assert.Contains("SiteId is required", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_SiteAdmin_ValidatesSiteExists()
    {
        // Arrange
        var dto = new CreateAdministratorDto
        {
            FirstName = "Admin",
            LastName = "Site",
            Email = "admin@test.com",
            Type = AdministratorType.Site,
            SiteId = 999
        };

        _siteRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Site?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.CreateAsync(dto)
        );
        Assert.Contains("Site with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_SiteAdmin_CreatesSuccessfully()
    {
        // Arrange
        var siteId = 1;
        var site = new Site { Id = siteId, Name = "Test Site", Address = "123 Test" };

        var dto = new CreateAdministratorDto
        {
            FirstName = "Admin",
            LastName = "Site",
            Email = "admin@test.com",
            Type = AdministratorType.Site,
            SiteId = siteId
        };

        _siteRepositoryMock.Setup(r => r.GetByIdAsync(siteId)).ReturnsAsync(site);
        _adminRepositoryMock.Setup(r => r.GetNextUsernameAsync(AdministratorType.Site))
            .ReturnsAsync("AS00001");

        _adminRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Administrator>()))
            .ReturnsAsync((Administrator a) =>
            {
                a.Id = 1;
                a.Site = site;
                return a;
            });

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("AS00001", result.Username);
        Assert.Equal("Site", result.Type);
        Assert.Equal(siteId, result.SiteId);
        Assert.Equal("Test Site", result.SiteName);
    }

    [Fact]
    public async Task GetByUsernameAsync_ReturnsAdmin()
    {
        // Arrange
        var admin = new Administrator
        {
            Id = 1,
            Username = "AG0001",
            FirstName = "Test",
            LastName = "Admin",
            Email = "test@test.com",
            Type = AdministratorType.Global
        };

        _adminRepositoryMock.Setup(r => r.GetByUsernameAsync("AG0001")).ReturnsAsync(admin);

        // Act
        var result = await _service.GetByUsernameAsync("AG0001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("AG0001", result.Username);
    }
}
