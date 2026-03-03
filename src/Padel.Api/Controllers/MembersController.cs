using Microsoft.AspNetCore.Mvc;
using Padel.Application.DTOs;
using Padel.Application.Interfaces;

namespace Padel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController(IMemberService memberService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetAll()
    {
        var members = await memberService.GetAllAsync();
        return Ok(members);
    }

    [HttpGet("site/{siteId}")]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetBySiteId(int siteId)
    {
        var members = await memberService.GetBySiteIdAsync(siteId);
        return Ok(members);
    }

    [HttpGet("{matricule}")]
    public async Task<ActionResult<MemberDto>> GetByMatricule(string matricule)
    {
        var member = await memberService.GetByMatriculeAsync(matricule);
        if (member is null)
            return NotFound();

        return Ok(member);
    }

    [HttpPost]
    public async Task<ActionResult<MemberDto>> Create(CreateMemberDto dto)
    {
        try
        {
            var member = await memberService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetByMatricule), new { matricule = member.Matricule }, member);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{matricule}")]
    public async Task<ActionResult<MemberDto>> Update(string matricule, UpdateMemberDto dto)
    {
        var member = await memberService.UpdateAsync(matricule, dto);
        if (member is null)
            return NotFound();

        return Ok(member);
    }

    [HttpDelete("{matricule}")]
    public async Task<IActionResult> Delete(string matricule)
    {
        var result = await memberService.DeleteAsync(matricule);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
