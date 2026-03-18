using Padel.Domain.Entities;

namespace Padel.Application.Interfaces;

public interface IMatchRepository
{
    Task<IEnumerable<Match>> GetAllAsync();
    Task<Match?> GetByIdAsync(int id);
    Task<IEnumerable<Match>> GetPublicAsync(int? siteId);
    Task<IEnumerable<Match>> GetByOrganizerAsync(int organizerId);
    Task<IEnumerable<Match>> GetByPlayerAsync(int memberId);
    Task<IEnumerable<Match>> GetBySiteAsync(int siteId);
    Task<bool> HasConflictAsync(int courtId, DateTime scheduledAt, DateTime endsAt);
    Task<IEnumerable<Match>> GetMatchesBecomingPublicAsync(DateTime tomorrowDate);
    Task<IEnumerable<Match>> GetMatchesWithUnpaidPlayersAsync(DateTime tomorrowDate);
    Task<Match> CreateAsync(Match match);
    Task<Match> UpdateAsync(Match match);
}
