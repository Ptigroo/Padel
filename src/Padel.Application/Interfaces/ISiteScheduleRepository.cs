using Padel.Domain.Entities;

namespace Padel.Application.Interfaces;

public interface ISiteScheduleRepository
{
    Task<IEnumerable<SiteSchedule>> GetBySiteIdAsync(int siteId);
    Task<SiteSchedule?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int siteId, int year);
    Task<SiteSchedule> CreateAsync(SiteSchedule schedule);
    Task DeleteAsync(SiteSchedule schedule);
}
