using Padel.Application.DTOs;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Application.Services;

public class ClosureDayService(IClosureDayRepository closureDayRepository, ISiteRepository siteRepository) : IClosureDayService
{
    public async Task<IEnumerable<ClosureDayDto>> GetBySiteIdAsync(int? siteId)
    {
        var closureDays = await closureDayRepository.GetBySiteIdAsync(siteId);
        return closureDays.Select(MapToDto);
    }

    public async Task<ClosureDayDto> CreateAsync(CreateClosureDayDto dto)
    {
        if (dto.SiteId.HasValue)
        {
            _ = await siteRepository.GetByIdAsync(dto.SiteId.Value)
                ?? throw new InvalidOperationException($"Site with id {dto.SiteId.Value} not found.");
        }

        var closureDay = new ClosureDay
        {
            Date = dto.Date,
            Reason = dto.Reason,
            SiteId = dto.SiteId
        };

        var created = await closureDayRepository.CreateAsync(closureDay);
        return MapToDto(created);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var closureDay = await closureDayRepository.GetByIdAsync(id);
        if (closureDay is null)
            return false;

        await closureDayRepository.DeleteAsync(closureDay);
        return true;
    }

    private static ClosureDayDto MapToDto(ClosureDay closureDay) => new()
    {
        Id = closureDay.Id,
        Date = closureDay.Date,
        Reason = closureDay.Reason,
        SiteId = closureDay.SiteId,
        SiteName = closureDay.Site?.Name
    };
}
