using Microsoft.AspNetCore.Mvc;
using Padel.Application.DTOs;
using Padel.Application.Interfaces;

namespace Padel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SitesController(ISiteService siteService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SiteDto>>> GetAll()
    {
        var sites = await siteService.GetAllAsync();
        return Ok(sites);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SiteDto>> GetById(int id)
    {
        var site = await siteService.GetByIdAsync(id);
        if (site is null)
            return NotFound();

        return Ok(site);
    }

    [HttpPost]
    public async Task<ActionResult<SiteDto>> Create(CreateSiteDto dto)
    {
        var site = await siteService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = site.Id }, site);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SiteDto>> Update(int id, UpdateSiteDto dto)
    {
        var site = await siteService.UpdateAsync(id, dto);
        if (site is null)
            return NotFound();

        return Ok(site);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await siteService.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
