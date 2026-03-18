using Microsoft.AspNetCore.Mvc;
using Padel.Application.DTOs;
using Padel.Application.Interfaces;

namespace Padel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchesController(IMatchService matchService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MatchDto>>> GetAll()
    {
        var matches = await matchService.GetAllAsync();
        return Ok(matches);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MatchDto>> GetById(int id)
    {
        var match = await matchService.GetByIdAsync(id);
        if (match is null)
            return NotFound();
        return Ok(match);
    }

    [HttpGet("public")]
    public async Task<ActionResult<IEnumerable<MatchDto>>> GetPublic([FromQuery] int? siteId)
    {
        var matches = await matchService.GetPublicAsync(siteId);
        return Ok(matches);
    }

    [HttpGet("organizer/{matricule}")]
    public async Task<ActionResult<IEnumerable<MatchDto>>> GetByOrganizer(string matricule)
    {
        try
        {
            var matches = await matchService.GetByOrganizerAsync(matricule);
            return Ok(matches);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("player/{matricule}")]
    public async Task<ActionResult<IEnumerable<MatchDto>>> GetByPlayer(string matricule)
    {
        try
        {
            var matches = await matchService.GetByPlayerAsync(matricule);
            return Ok(matches);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("site/{siteId}")]
    public async Task<ActionResult<IEnumerable<MatchDto>>> GetBySite(int siteId)
    {
        var matches = await matchService.GetBySiteAsync(siteId);
        return Ok(matches);
    }

    [HttpPost]
    public async Task<ActionResult<MatchDto>> Create(CreateMatchDto dto)
    {
        try
        {
            var match = await matchService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = match.Id }, match);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/players")]
    public async Task<ActionResult<MatchDto>> AddPlayer(int id, AddPlayerDto dto)
    {
        try
        {
            var match = await matchService.AddPlayerAsync(id, dto);
            return Ok(match);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/join/{matricule}")]
    public async Task<ActionResult<MatchDto>> Join(int id, string matricule)
    {
        try
        {
            var match = await matchService.JoinAsync(id, matricule);
            return Ok(match);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("process-day-before")]
    public async Task<IActionResult> ProcessDayBefore()
    {
        await matchService.ProcessDayBeforeAsync();
        return Ok(new { message = "Day-before processing completed." });
    }
}
