using Microsoft.AspNetCore.Mvc;
using Padel.Application.DTOs;
using Padel.Application.Interfaces;

namespace Padel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController(IStatsService statsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<GlobalStatsDto>> GetGlobalStats()
    {
        var stats = await statsService.GetGlobalStatsAsync();
        return Ok(stats);
    }

    [HttpGet("site/{siteId}")]
    public async Task<ActionResult<SiteStatsDto>> GetSiteStats(int siteId)
    {
        var stats = await statsService.GetSiteStatsAsync(siteId);
        if (stats is null)
            return NotFound();
        return Ok(stats);
    }

    [HttpGet("sites")]
    public async Task<ActionResult<IEnumerable<SiteStatsDto>>> GetAllSiteStats()
    {
        var stats = await statsService.GetAllSiteStatsAsync();
        return Ok(stats);
    }
}
