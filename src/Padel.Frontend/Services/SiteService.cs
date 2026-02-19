using System.Net.Http.Json;
using Padel.Frontend.Models;

namespace Padel.Frontend.Services;

public class SiteService(HttpClient httpClient) : ISiteService
{
    public async Task<List<SiteDto>> GetAllAsync()
    {
        return await httpClient.GetFromJsonAsync<List<SiteDto>>("api/sites") ?? [];
    }

    public async Task<SiteDto?> GetByIdAsync(int id)
    {
        return await httpClient.GetFromJsonAsync<SiteDto>($"api/sites/{id}");
    }

    public async Task<SiteDto?> CreateAsync(CreateSiteDto dto)
    {
        var response = await httpClient.PostAsJsonAsync("api/sites", dto);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<SiteDto>();
        return null;
    }

    public async Task<SiteDto?> UpdateAsync(int id, UpdateSiteDto dto)
    {
        var response = await httpClient.PutAsJsonAsync($"api/sites/{id}", dto);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<SiteDto>();
        return null;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await httpClient.DeleteAsync($"api/sites/{id}");
        return response.IsSuccessStatusCode;
    }
}
