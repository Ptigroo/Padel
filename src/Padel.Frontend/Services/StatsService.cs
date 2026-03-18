using System.Net.Http.Json;
using Padel.Frontend.Models;

namespace Padel.Frontend.Services;

public class StatsService(HttpClient httpClient) : IStatsService
{
    public async Task<GlobalStatsDto?> GetGlobalStatsAsync()
    {
        return await httpClient.GetFromJsonAsync<GlobalStatsDto>("api/stats");
    }

    public async Task<SiteStatsDto?> GetSiteStatsAsync(int siteId)
    {
        return await httpClient.GetFromJsonAsync<SiteStatsDto>($"api/stats/site/{siteId}");
    }

    public async Task<List<SiteStatsDto>> GetAllSiteStatsAsync()
    {
        return await httpClient.GetFromJsonAsync<List<SiteStatsDto>>("api/stats/sites") ?? [];
    }
}
