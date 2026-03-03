using System.Net.Http.Json;
using Padel.Frontend.Models;

namespace Padel.Frontend.Services;

public class ScheduleService(HttpClient httpClient) : IScheduleService
{
    public async Task<List<SiteScheduleDto>> GetBySiteIdAsync(int siteId)
    {
        return await httpClient.GetFromJsonAsync<List<SiteScheduleDto>>($"api/schedules/site/{siteId}") ?? [];
    }

    public async Task<SiteScheduleDto?> CreateAsync(CreateSiteScheduleDto dto)
    {
        var response = await httpClient.PostAsJsonAsync("api/schedules", dto);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<SiteScheduleDto>();
        return null;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await httpClient.DeleteAsync($"api/schedules/{id}");
        return response.IsSuccessStatusCode;
    }
}
