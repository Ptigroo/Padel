using Microsoft.EntityFrameworkCore;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;
using Padel.Infrastructure.Data;
using MatchType = Padel.Domain.Entities.MatchType;

namespace Padel.Infrastructure.Repositories;

public class PaymentRepository(PadelDbContext context) : IPaymentRepository
{
    private IQueryable<Payment> FullQuery => context.Payments
        .Include(p => p.Member)
        .Include(p => p.Match)
            .ThenInclude(m => m!.Court)
                .ThenInclude(c => c!.Site)
        .Include(p => p.MatchPlayer);

    public async Task<IEnumerable<Payment>> GetByMemberAsync(int memberId)
    {
        return await FullQuery
            .Where(p => p.MemberId == memberId)
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByMatchAsync(int matchId)
    {
        return await FullQuery
            .Where(p => p.MatchId == matchId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Payment?> GetByIdAsync(int id)
    {
        return await FullQuery.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<decimal> GetUnpaidBalanceAsync(int memberId)
    {
        return await context.Payments
            .Where(p => p.MemberId == memberId && p.Status == PaymentStatus.Pending)
            .SumAsync(p => p.Amount);
    }

    public async Task<decimal> GetUnpaidPublicMatchDebtAsync(int memberId)
    {
        // CF-RC-006: Calculate debt for public matches where the organizer
        // has unfilled spots (< 4 players) and the match date has passed
        var now = DateTime.Now;
        var matches = await context.Matches
            .Include(m => m.Players)
            .Where(m => m.OrganizerId == memberId
                && m.MatchType == MatchType.Public
                && m.ScheduledAt <= now
                && m.Players.Count < 4)
            .ToListAsync();

        return matches.Sum(m => (4 - m.Players.Count) * 15m);
    }

    public async Task<Payment> CreateAsync(Payment payment)
    {
        context.Payments.Add(payment);
        await context.SaveChangesAsync();
        return payment;
    }

    public async Task<Payment> UpdateAsync(Payment payment)
    {
        context.Payments.Update(payment);
        await context.SaveChangesAsync();
        return payment;
    }
}
