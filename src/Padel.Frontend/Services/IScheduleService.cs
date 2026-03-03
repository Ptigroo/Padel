using Padel.Frontend.Models;

namespace Padel.Frontend.Services;

public interface IScheduleService
{
    Task<List<SiteScheduleDto>> GetBySiteIdAsync(int siteId);
    Task<SiteScheduleDto?> CreateAsync(CreateSiteScheduleDto dto);
    Task<bool> DeleteAsync(int id);
}
