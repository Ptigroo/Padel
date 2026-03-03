using Microsoft.AspNetCore.Mvc;
using Padel.Application.DTOs;
using Padel.Application.Interfaces;

namespace Padel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchedulesController(IScheduleService scheduleService) : ControllerBase
{
    [HttpGet("site/{siteId}")]
    public async Task<ActionResult<IEnumerable<SiteScheduleDto>>> GetBySiteId(int siteId)
    {
        var schedules = await scheduleService.GetBySiteIdAsync(siteId);
        return Ok(schedules);
    }

    [HttpPost]
    public async Task<ActionResult<SiteScheduleDto>> Create(CreateSiteScheduleDto dto)
    {
        try
        {
            var schedule = await scheduleService.CreateAsync(dto);
            return Created($"api/schedules/{schedule.Id}", schedule);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await scheduleService.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
