using Padel.Application.DTOs;

namespace Padel.Application.Interfaces;

public interface IMatchService
{
    Task<IEnumerable<MatchDto>> GetAllAsync();
    Task<MatchDto?> GetByIdAsync(int id);
    Task<IEnumerable<MatchDto>> GetPublicAsync(int? siteId);
    Task<IEnumerable<MatchDto>> GetByOrganizerAsync(string matricule);
    Task<IEnumerable<MatchDto>> GetByPlayerAsync(string matricule);
    Task<IEnumerable<MatchDto>> GetBySiteAsync(int siteId);
    Task<MatchDto> CreateAsync(CreateMatchDto dto);
    Task<MatchDto> AddPlayerAsync(int matchId, AddPlayerDto dto);
    Task<MatchDto> JoinAsync(int matchId, string matricule);
    Task ProcessDayBeforeAsync();
}
