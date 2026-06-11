using Microsoft.AspNetCore.Mvc;
using Padel.Application.DTOs;
using Padel.Application.Interfaces;

namespace Padel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdministratorsController(IAdministratorService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AdministratorDto>>> GetAll()
    {
        var administrators = await service.GetAllAsync();
        return Ok(administrators);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdministratorDto>> GetById(int id)
    {
        var administrator = await service.GetByIdAsync(id);
        if (administrator == null)
        {
            return NotFound($"Administrator with ID {id} not found.");
        }
        return Ok(administrator);
    }

    [HttpGet("username/{username}")]
    public async Task<ActionResult<AdministratorDto>> GetByUsername(string username)
    {
        var administrator = await service.GetByUsernameAsync(username);
        if (administrator == null)
        {
            return NotFound($"Administrator with username {username} not found.");
        }
        return Ok(administrator);
    }

    [HttpGet("site/{siteId:int}")]
    public async Task<ActionResult<List<AdministratorDto>>> GetBySiteId(int siteId)
    {
        var administrators = await service.GetBySiteIdAsync(siteId);
        return Ok(administrators);
    }

    [HttpPost]
    public async Task<ActionResult<AdministratorDto>> Create([FromBody] CreateAdministratorDto dto)
    {
        try
        {
            var administrator = await service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = administrator.Id }, administrator);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await service.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound($"Administrator with ID {id} not found.");
        }

        await service.DeleteAsync(id);
        return NoContent();
    }
}
