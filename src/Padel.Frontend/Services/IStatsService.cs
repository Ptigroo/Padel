using Padel.Frontend.Models;

namespace Padel.Frontend.Services;

public interface IStatsService
{
    Task<GlobalStatsDto?> GetGlobalStatsAsync();
    Task<SiteStatsDto?> GetSiteStatsAsync(int siteId);
    Task<List<SiteStatsDto>> GetAllSiteStatsAsync();
}
