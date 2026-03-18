using Padel.Application.DTOs;

namespace Padel.Application.Interfaces;

public interface IStatsService
{
    Task<GlobalStatsDto> GetGlobalStatsAsync();
    Task<SiteStatsDto?> GetSiteStatsAsync(int siteId);
    Task<IEnumerable<SiteStatsDto>> GetAllSiteStatsAsync();
}
