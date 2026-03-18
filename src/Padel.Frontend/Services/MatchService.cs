using System.Net.Http.Json;
using Padel.Frontend.Models;

namespace Padel.Frontend.Services;

public class MatchService(HttpClient httpClient) : IMatchService
{
    public async Task<List<MatchDto>> GetAllAsync()
    {
        return await httpClient.GetFromJsonAsync<List<MatchDto>>("api/matches") ?? [];
    }

    public async Task<MatchDto?> GetByIdAsync(int id)
    {
        return await httpClient.GetFromJsonAsync<MatchDto>($"api/matches/{id}");
    }

    public async Task<List<MatchDto>> GetPublicAsync(int? siteId)
    {
        var url = siteId.HasValue ? $"api/matches/public?siteId={siteId}" : "api/matches/public";
        return await httpClient.GetFromJsonAsync<List<MatchDto>>(url) ?? [];
    }

    public async Task<List<MatchDto>> GetByOrganizerAsync(string matricule)
    {
        return await httpClient.GetFromJsonAsync<List<MatchDto>>($"api/matches/organizer/{matricule}") ?? [];
    }

    public async Task<List<MatchDto>> GetByPlayerAsync(string matricule)
    {
        return await httpClient.GetFromJsonAsync<List<MatchDto>>($"api/matches/player/{matricule}") ?? [];
    }

    public async Task<List<MatchDto>> GetBySiteAsync(int siteId)
    {
        return await httpClient.GetFromJsonAsync<List<MatchDto>>($"api/matches/site/{siteId}") ?? [];
    }

    public async Task<MatchDto?> CreateAsync(CreateMatchDto dto)
    {
        var response = await httpClient.PostAsJsonAsync("api/matches", dto);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<MatchDto>();
        return null;
    }

    public async Task<MatchDto?> AddPlayerAsync(int matchId, AddPlayerDto dto)
    {
        var response = await httpClient.PostAsJsonAsync($"api/matches/{matchId}/players", dto);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<MatchDto>();
        return null;
    }

    public async Task<MatchDto?> JoinAsync(int matchId, string matricule)
    {
        var response = await httpClient.PostAsync($"api/matches/{matchId}/join/{matricule}", null);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<MatchDto>();
        return null;
    }

    public async Task<bool> ProcessDayBeforeAsync()
    {
        var response = await httpClient.PostAsync("api/matches/process-day-before", null);
        return response.IsSuccessStatusCode;
    }
}
