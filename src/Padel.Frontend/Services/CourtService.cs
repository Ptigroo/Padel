using System.Net.Http.Json;
using Padel.Frontend.Models;

namespace Padel.Frontend.Services;

public class CourtService(HttpClient httpClient) : ICourtService
{
    public async Task<List<CourtDto>> GetBySiteIdAsync(int siteId)
    {
        return await httpClient.GetFromJsonAsync<List<CourtDto>>($"api/courts/site/{siteId}") ?? [];
    }

    public async Task<CourtDto?> GetByIdAsync(int id)
    {
        return await httpClient.GetFromJsonAsync<CourtDto>($"api/courts/{id}");
    }

    public async Task<CourtDto?> CreateAsync(CreateCourtDto dto)
    {
        var response = await httpClient.PostAsJsonAsync("api/courts", dto);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<CourtDto>();
        return null;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await httpClient.DeleteAsync($"api/courts/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<SlotDto>> GetAvailableSlotsAsync(int courtId, DateOnly date)
    {
        return await httpClient.GetFromJsonAsync<List<SlotDto>>($"api/courts/{courtId}/slots/{date:yyyy-MM-dd}") ?? [];
    }
}
