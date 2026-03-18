using Padel.Domain.Entities;

namespace Padel.Application.Interfaces;

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetByMemberAsync(int memberId);
    Task<IEnumerable<Payment>> GetByMatchAsync(int matchId);
    Task<Payment?> GetByIdAsync(int id);
    Task<decimal> GetUnpaidBalanceAsync(int memberId);
    Task<decimal> GetUnpaidPublicMatchDebtAsync(int memberId);
    Task<Payment> CreateAsync(Payment payment);
    Task<Payment> UpdateAsync(Payment payment);
}
