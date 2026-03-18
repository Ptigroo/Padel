using Microsoft.AspNetCore.Mvc;
using Moq;
using Padel.Api.Controllers;
using Padel.Application.DTOs;
using Padel.Application.Interfaces;

namespace Padel.Tests.Controllers;

public class SitesControllerTests
{
    private readonly Mock<ISiteService> _siteService = new();

    private SitesController CreateController() => new(_siteService.Object);

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _siteService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<SiteDto> { new() { Id = 1, Name = "S1", Address = "A1" } });

        var controller = CreateController();
        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, ok.StatusCode);
    }

    [Fact]
    public async Task GetById_Existing_ReturnsOk()
    {
        _siteService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(new SiteDto { Id = 1, Name = "S1", Address = "A1" });

        var controller = CreateController();
        var result = await controller.GetById(1);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        _siteService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((SiteDto?)null);

        var controller = CreateController();
        var result = await controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var dto = new CreateSiteDto { Name = "Nouveau", Address = "Adresse" };
        _siteService.Setup(s => s.CreateAsync(dto))
            .ReturnsAsync(new SiteDto { Id = 1, Name = "Nouveau", Address = "Adresse" });

        var controller = CreateController();
        var result = await controller.Create(dto);

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task Update_Existing_ReturnsOk()
    {
        var dto = new UpdateSiteDto { Name = "Modifié", Address = "Nouvelle" };
        _siteService.Setup(s => s.UpdateAsync(1, dto))
            .ReturnsAsync(new SiteDto { Id = 1, Name = "Modifié", Address = "Nouvelle" });

        var controller = CreateController();
        var result = await controller.Update(1, dto);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task Update_NotFound_Returns404()
    {
        _siteService.Setup(s => s.UpdateAsync(999, It.IsAny<UpdateSiteDto>()))
            .ReturnsAsync((SiteDto?)null);

        var controller = CreateController();
        var result = await controller.Update(999, new UpdateSiteDto { Name = "X", Address = "X" });

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Delete_Existing_ReturnsNoContent()
    {
        _siteService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        var controller = CreateController();
        var result = await controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_NotFound_Returns404()
    {
        _siteService.Setup(s => s.DeleteAsync(999)).ReturnsAsync(false);

        var controller = CreateController();
        var result = await controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }
}
