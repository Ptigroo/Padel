using Padel.Application.DTOs;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Application.Services;

public class PaymentService(
    IPaymentRepository paymentRepository,
    IMemberRepository memberRepository) : IPaymentService
{
    public async Task<IEnumerable<PaymentDto>> GetByMemberAsync(string matricule)
    {
        var member = await memberRepository.GetByMatriculeAsync(matricule)
            ?? throw new InvalidOperationException($"Member with matricule {matricule} not found.");
        var payments = await paymentRepository.GetByMemberAsync(member.Id);
        return payments.Select(MapToDto);
    }

    public async Task<IEnumerable<PaymentDto>> GetByMatchAsync(int matchId)
    {
        var payments = await paymentRepository.GetByMatchAsync(matchId);
        return payments.Select(MapToDto);
    }

    public async Task<decimal> GetBalanceAsync(string matricule)
    {
        var member = await memberRepository.GetByMatriculeAsync(matricule)
            ?? throw new InvalidOperationException($"Member with matricule {matricule} not found.");
        return await paymentRepository.GetUnpaidBalanceAsync(member.Id);
    }

    public async Task<PaymentDto> ProcessPaymentAsync(ProcessPaymentDto dto)
    {
        var payment = await paymentRepository.GetByIdAsync(dto.PaymentId)
            ?? throw new InvalidOperationException($"Payment with id {dto.PaymentId} not found.");

        // CF-RV-023: Cannot pay twice
        if (payment.Status == PaymentStatus.Paid)
            throw new InvalidOperationException("This payment has already been processed.");

        // CF-RC-006: Check for unpaid public match debt
        var publicDebt = await paymentRepository.GetUnpaidPublicMatchDebtAsync(payment.MemberId);
        if (publicDebt > 0)
            payment.Amount += publicDebt;

        payment.Status = PaymentStatus.Paid;
        payment.PaidAt = DateTime.Now;

        var updated = await paymentRepository.UpdateAsync(payment);
        return MapToDto(updated);
    }

    private static PaymentDto MapToDto(Payment payment) => new()
    {
        Id = payment.Id,
        MatchPlayerId = payment.MatchPlayerId,
        MatchId = payment.MatchId,
        MemberId = payment.MemberId,
        Matricule = payment.Member?.Matricule ?? "",
        Amount = payment.Amount,
        Status = payment.Status.ToString(),
        CreatedAt = payment.CreatedAt,
        PaidAt = payment.PaidAt,
        CourtName = payment.Match?.Court?.Name ?? "",
        SiteName = payment.Match?.Court?.Site?.Name ?? "",
        ScheduledAt = payment.Match?.ScheduledAt ?? default
    };
}
