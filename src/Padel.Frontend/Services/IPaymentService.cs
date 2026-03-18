using Padel.Frontend.Models;

namespace Padel.Frontend.Services;

public interface IPaymentService
{
    Task<List<PaymentDto>> GetByMemberAsync(string matricule);
    Task<List<PaymentDto>> GetByMatchAsync(int matchId);
    Task<decimal> GetBalanceAsync(string matricule);
    Task<PaymentDto?> ProcessPaymentAsync(int paymentId);
}
