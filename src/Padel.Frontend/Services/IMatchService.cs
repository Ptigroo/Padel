using Padel.Frontend.Models;

namespace Padel.Frontend.Services;

public interface IMatchService
{
    Task<List<MatchDto>> GetAllAsync();
    Task<MatchDto?> GetByIdAsync(int id);
    Task<List<MatchDto>> GetPublicAsync(int? siteId);
    Task<List<MatchDto>> GetByOrganizerAsync(string matricule);
    Task<List<MatchDto>> GetByPlayerAsync(string matricule);
    Task<MatchDto?> CreateAsync(CreateMatchDto dto);
    Task<MatchDto?> AddPlayerAsync(int matchId, AddPlayerDto dto);
    Task<MatchDto?> JoinAsync(int matchId, string matricule);
    Task<bool> ProcessDayBeforeAsync();
}
