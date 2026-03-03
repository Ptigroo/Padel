using Padel.Application.DTOs;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Application.Services;

public class CourtService(ICourtRepository courtRepository, ISiteRepository siteRepository) : ICourtService
{
    public async Task<IEnumerable<CourtDto>> GetBySiteIdAsync(int siteId)
    {
        var courts = await courtRepository.GetBySiteIdAsync(siteId);
        return courts.Select(MapToDto);
    }

    public async Task<CourtDto?> GetByIdAsync(int id)
    {
        var court = await courtRepository.GetByIdAsync(id);
        return court is null ? null : MapToDto(court);
    }

    public async Task<CourtDto> CreateAsync(CreateCourtDto dto)
    {
        var site = await siteRepository.GetByIdAsync(dto.SiteId)
            ?? throw new InvalidOperationException($"Site with id {dto.SiteId} not found.");

        var court = new Court
        {
            Name = dto.Name,
            SiteId = dto.SiteId
        };

        var created = await courtRepository.CreateAsync(court);
        created.Site = site;
        return MapToDto(created);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var court = await courtRepository.GetByIdAsync(id);
        if (court is null)
            return false;

        await courtRepository.DeleteAsync(court);
        return true;
    }

    private static CourtDto MapToDto(Court court) => new()
    {
        Id = court.Id,
        Name = court.Name,
        SiteId = court.SiteId,
        SiteName = court.Site?.Name
    };
}
