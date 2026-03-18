using Microsoft.AspNetCore.Mvc;
using Padel.Application.DTOs;
using Padel.Application.Interfaces;

namespace Padel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClosureDaysController(IClosureDayService closureDayService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClosureDayDto>>> GetBySiteId([FromQuery] int? siteId)
    {
        var closureDays = await closureDayService.GetBySiteIdAsync(siteId);
        return Ok(closureDays);
    }

    [HttpPost]
    public async Task<ActionResult<ClosureDayDto>> Create(CreateClosureDayDto dto)
    {
        try
        {
            var closureDay = await closureDayService.CreateAsync(dto);
            return Created($"api/closuredays/{closureDay.Id}", closureDay);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await closureDayService.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
