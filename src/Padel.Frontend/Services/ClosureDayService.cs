using System.Net.Http.Json;
using Padel.Frontend.Models;

namespace Padel.Frontend.Services;

public class ClosureDayService(HttpClient httpClient) : IClosureDayService
{
    public async Task<List<ClosureDayDto>> GetBySiteIdAsync(int? siteId)
    {
        var url = siteId.HasValue ? $"api/closuredays?siteId={siteId}" : "api/closuredays";
        return await httpClient.GetFromJsonAsync<List<ClosureDayDto>>(url) ?? [];
    }

    public async Task<ClosureDayDto?> CreateAsync(CreateClosureDayDto dto)
    {
        var response = await httpClient.PostAsJsonAsync("api/closuredays", dto);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<ClosureDayDto>();
        return null;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await httpClient.DeleteAsync($"api/closuredays/{id}");
        return response.IsSuccessStatusCode;
    }
}
