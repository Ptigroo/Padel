using System.Net.Http.Json;
using Padel.Frontend.Models;

namespace Padel.Frontend.Services;

public class AdministratorService(HttpClient httpClient) : IAdministratorService
{
    private const string BaseUrl = "api/administrators";

    public async Task<List<AdministratorDto>> GetAllAsync()
    {
        return await httpClient.GetFromJsonAsync<List<AdministratorDto>>(BaseUrl)
            ?? [];
    }

    public async Task<AdministratorDto?> GetByUsernameAsync(string username)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<AdministratorDto>($"{BaseUrl}/username/{username}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<List<AdministratorDto>> GetBySiteIdAsync(int siteId)
    {
        return await httpClient.GetFromJsonAsync<List<AdministratorDto>>($"{BaseUrl}/site/{siteId}")
            ?? [];
    }

    public async Task<AdministratorDto> CreateAsync(CreateAdministratorDto dto)
    {
        var response = await httpClient.PostAsJsonAsync(BaseUrl, dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AdministratorDto>()
            ?? throw new InvalidOperationException("Failed to create administrator");
    }
}
