using Padel.Application.DTOs;

namespace Padel.Application.Interfaces;

public interface IPaymentService
{
    Task<IEnumerable<PaymentDto>> GetByMemberAsync(string matricule);
    Task<IEnumerable<PaymentDto>> GetByMatchAsync(int matchId);
    Task<decimal> GetBalanceAsync(string matricule);
    Task<PaymentDto> ProcessPaymentAsync(ProcessPaymentDto dto);
}
