using Microsoft.AspNetCore.Mvc;
using Moq;
using Padel.Api.Controllers;
using Padel.Application.DTOs;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Tests.Controllers;

public class AdministratorsControllerTests
{
    private readonly Mock<IAdministratorService> _serviceMock;
    private readonly AdministratorsController _controller;

    public AdministratorsControllerTests()
    {
        _serviceMock = new Mock<IAdministratorService>();
        _controller = new AdministratorsController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithAdministrators()
    {
        // Arrange
        var administrators = new List<AdministratorDto>
        {
            new() { Id = 1, Username = "AG0001", FirstName = "Admin", LastName = "One", Email = "a1@test.com", Type = "Global" },
            new() { Id = 2, Username = "AS00001", FirstName = "Admin", LastName = "Two", Email = "a2@test.com", Type = "Site", SiteId = 1, SiteName = "Test Site" }
        };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(administrators);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedAdmins = Assert.IsType<List<AdministratorDto>>(okResult.Value);
        Assert.Equal(2, returnedAdmins.Count);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenAdministratorExists()
    {
        // Arrange
        var admin = new AdministratorDto
        {
            Id = 1,
            Username = "AG0001",
            FirstName = "Test",
            LastName = "Admin",
            Email = "test@test.com",
            Type = "Global"
        };
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(admin);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedAdmin = Assert.IsType<AdministratorDto>(okResult.Value);
        Assert.Equal("AG0001", returnedAdmin.Username);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenAdministratorDoesNotExist()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((AdministratorDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByUsername_ReturnsOk_WhenAdministratorExists()
    {
        // Arrange
        var admin = new AdministratorDto
        {
            Id = 1,
            Username = "AG0001",
            FirstName = "Test",
            LastName = "Admin",
            Email = "test@test.com",
            Type = "Global"
        };
        _serviceMock.Setup(s => s.GetByUsernameAsync("AG0001")).ReturnsAsync(admin);

        // Act
        var result = await _controller.GetByUsername("AG0001");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedAdmin = Assert.IsType<AdministratorDto>(okResult.Value);
        Assert.Equal("AG0001", returnedAdmin.Username);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenValid()
    {
        // Arrange
        var dto = new CreateAdministratorDto
        {
            FirstName = "New",
            LastName = "Admin",
            Email = "new@test.com",
            Type = AdministratorType.Global
        };

        var created = new AdministratorDto
        {
            Id = 1,
            Username = "AG0001",
            FirstName = "New",
            LastName = "Admin",
            Email = "new@test.com",
            Type = "Global"
        };

        _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedAdmin = Assert.IsType<AdministratorDto>(createdResult.Value);
        Assert.Equal("AG0001", returnedAdmin.Username);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenInvalidOperationException()
    {
        // Arrange
        var dto = new CreateAdministratorDto
        {
            FirstName = "Invalid",
            LastName = "Admin",
            Email = "invalid@test.com",
            Type = AdministratorType.Site // Missing SiteId
        };

        _serviceMock.Setup(s => s.CreateAsync(dto))
            .ThrowsAsync(new InvalidOperationException("SiteId is required for Site administrators."));

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("SiteId is required for Site administrators.", badRequestResult.Value);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenAdministratorExists()
    {
        // Arrange
        var admin = new AdministratorDto { Id = 1, Username = "AG0001", FirstName = "Test", LastName = "Admin", Email = "test@test.com", Type = "Global" };
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(admin);
        _serviceMock.Setup(s => s.DeleteAsync(1)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenAdministratorDoesNotExist()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((AdministratorDto?)null);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
