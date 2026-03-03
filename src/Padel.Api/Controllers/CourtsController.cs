using Microsoft.AspNetCore.Mvc;
using Padel.Application.DTOs;
using Padel.Application.Interfaces;

namespace Padel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CourtsController(ICourtService courtService) : ControllerBase
{
    [HttpGet("site/{siteId}")]
    public async Task<ActionResult<IEnumerable<CourtDto>>> GetBySiteId(int siteId)
    {
        var courts = await courtService.GetBySiteIdAsync(siteId);
        return Ok(courts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CourtDto>> GetById(int id)
    {
        var court = await courtService.GetByIdAsync(id);
        if (court is null)
            return NotFound();

        return Ok(court);
    }

    [HttpPost]
    public async Task<ActionResult<CourtDto>> Create(CreateCourtDto dto)
    {
        try
        {
            var court = await courtService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = court.Id }, court);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await courtService.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
