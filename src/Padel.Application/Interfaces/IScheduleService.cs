using Padel.Application.DTOs;

namespace Padel.Application.Interfaces;

public interface IScheduleService
{
    Task<IEnumerable<SiteScheduleDto>> GetBySiteIdAsync(int siteId);
    Task<SiteScheduleDto> CreateAsync(CreateSiteScheduleDto dto);
    Task<bool> DeleteAsync(int id);
}
