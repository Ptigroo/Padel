using Microsoft.EntityFrameworkCore;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;
using Padel.Infrastructure.Data;
using MatchType = Padel.Domain.Entities.MatchType;

namespace Padel.Infrastructure.Repositories;

public class MatchRepository(PadelDbContext context) : IMatchRepository
{
    private IQueryable<Match> FullQuery => context.Matches
        .Include(m => m.Court)
            .ThenInclude(c => c!.Site)
        .Include(m => m.Organizer)
        .Include(m => m.Players)
            .ThenInclude(p => p.Member)
        .Include(m => m.Players)
            .ThenInclude(p => p.Payment);

    public async Task<IEnumerable<Match>> GetAllAsync()
    {
        return await FullQuery
            .OrderByDescending(m => m.ScheduledAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Match?> GetByIdAsync(int id)
    {
        return await FullQuery.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Match>> GetPublicAsync(int? siteId)
    {
        var query = FullQuery
            .Where(m => m.MatchType == MatchType.Public && m.Status == MatchStatus.Scheduled);

        if (siteId.HasValue)
            query = query.Where(m => m.Court!.SiteId == siteId.Value);

        return await query
            .OrderBy(m => m.ScheduledAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Match>> GetByOrganizerAsync(int organizerId)
    {
        return await FullQuery
            .Where(m => m.OrganizerId == organizerId)
            .OrderByDescending(m => m.ScheduledAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Match>> GetByPlayerAsync(int memberId)
    {
        return await FullQuery
            .Where(m => m.Players.Any(p => p.MemberId == memberId))
            .OrderByDescending(m => m.ScheduledAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Match>> GetBySiteAsync(int siteId)
    {
        return await FullQuery
            .Where(m => m.Court!.SiteId == siteId)
            .OrderByDescending(m => m.ScheduledAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> HasConflictAsync(int courtId, DateTime scheduledAt, DateTime endsAt)
    {
        return await context.Matches
            .AnyAsync(m => m.CourtId == courtId
                && m.Status != MatchStatus.Cancelled
                && m.ScheduledAt < endsAt
                && m.EndsAt > scheduledAt);
    }

    public async Task<IEnumerable<Match>> GetMatchesBecomingPublicAsync(DateTime tomorrowDate)
    {
        return await FullQuery
            .Where(m => m.MatchType == MatchType.Private
                && m.Status == MatchStatus.Scheduled
                && m.ScheduledAt.Date == tomorrowDate.Date
                && m.Players.Count < 4)
            .ToListAsync();
    }

    public async Task<IEnumerable<Match>> GetMatchesWithUnpaidPlayersAsync(DateTime tomorrowDate)
    {
        return await FullQuery
            .Where(m => m.ScheduledAt.Date == tomorrowDate.Date
                && (m.Status == MatchStatus.Scheduled || m.Status == MatchStatus.Full)
                && m.Players.Any(p => p.Payment != null && p.Payment.Status == PaymentStatus.Pending))
            .ToListAsync();
    }

    public async Task<Match> CreateAsync(Match match)
    {
        context.Matches.Add(match);
        await context.SaveChangesAsync();
        return match;
    }

    public async Task<Match> UpdateAsync(Match match)
    {
        context.Matches.Update(match);
        await context.SaveChangesAsync();
        return match;
    }
}
