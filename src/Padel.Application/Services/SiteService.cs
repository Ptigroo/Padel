using Padel.Application.DTOs;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Application.Services;

public class SiteService(ISiteRepository siteRepository) : ISiteService
{
    public async Task<IEnumerable<SiteDto>> GetAllAsync()
    {
        var sites = await siteRepository.GetAllAsync();
        return sites.Select(MapToDto);
    }

    public async Task<SiteDto?> GetByIdAsync(int id)
    {
        var site = await siteRepository.GetByIdAsync(id);
        return site is null ? null : MapToDto(site);
    }

    public async Task<SiteDto> CreateAsync(CreateSiteDto dto)
    {
        var site = new Site
        {
            Name = dto.Name,
            Address = dto.Address
        };

        var created = await siteRepository.CreateAsync(site);
        return MapToDto(created);
    }

    public async Task<SiteDto?> UpdateAsync(int id, UpdateSiteDto dto)
    {
        var site = await siteRepository.GetByIdAsync(id);
        if (site is null)
            return null;

        site.Name = dto.Name;
        site.Address = dto.Address;

        var updated = await siteRepository.UpdateAsync(site);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var site = await siteRepository.GetByIdAsync(id);
        if (site is null)
            return false;

        await siteRepository.DeleteAsync(site);
        return true;
    }

    private static SiteDto MapToDto(Site site) => new()
    {
        Id = site.Id,
        Name = site.Name,
        Address = site.Address
    };
}
