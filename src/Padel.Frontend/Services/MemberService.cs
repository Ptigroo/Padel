using System.Net.Http.Json;
using Padel.Frontend.Models;

namespace Padel.Frontend.Services;

public class MemberService(HttpClient httpClient) : IMemberService
{
    public async Task<List<MemberDto>> GetAllAsync()
    {
        return await httpClient.GetFromJsonAsync<List<MemberDto>>("api/members") ?? [];
    }

    public async Task<List<MemberDto>> GetBySiteIdAsync(int siteId)
    {
        return await httpClient.GetFromJsonAsync<List<MemberDto>>($"api/members/site/{siteId}") ?? [];
    }

    public async Task<MemberDto?> GetByMatriculeAsync(string matricule)
    {
        return await httpClient.GetFromJsonAsync<MemberDto>($"api/members/{matricule}");
    }

    public async Task<MemberDto?> CreateAsync(CreateMemberDto dto)
    {
        var response = await httpClient.PostAsJsonAsync("api/members", dto);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<MemberDto>();
        return null;
    }

    public async Task<MemberDto?> UpdateAsync(string matricule, UpdateMemberDto dto)
    {
        var response = await httpClient.PutAsJsonAsync($"api/members/{matricule}", dto);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<MemberDto>();
        return null;
    }

    public async Task<bool> DeleteAsync(string matricule)
    {
        var response = await httpClient.DeleteAsync($"api/members/{matricule}");
        return response.IsSuccessStatusCode;
    }
}
