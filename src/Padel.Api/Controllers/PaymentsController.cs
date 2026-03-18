using Microsoft.AspNetCore.Mvc;
using Padel.Application.DTOs;
using Padel.Application.Interfaces;

namespace Padel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    [HttpGet("member/{matricule}")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByMember(string matricule)
    {
        try
        {
            var payments = await paymentService.GetByMemberAsync(matricule);
            return Ok(payments);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("match/{matchId}")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByMatch(int matchId)
    {
        var payments = await paymentService.GetByMatchAsync(matchId);
        return Ok(payments);
    }

    [HttpGet("balance/{matricule}")]
    public async Task<ActionResult<object>> GetBalance(string matricule)
    {
        try
        {
            var balance = await paymentService.GetBalanceAsync(matricule);
            return Ok(new { matricule, balance });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("pay")]
    public async Task<ActionResult<PaymentDto>> Pay(ProcessPaymentDto dto)
    {
        try
        {
            var payment = await paymentService.ProcessPaymentAsync(dto);
            return Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
