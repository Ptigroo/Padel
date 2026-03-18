using Padel.Domain.Entities;

namespace Padel.Application.Interfaces;

public interface IClosureDayRepository
{
    Task<IEnumerable<ClosureDay>> GetBySiteIdAsync(int? siteId);
    Task<ClosureDay?> GetByIdAsync(int id);
    Task<bool> ExistsForDateAndSiteAsync(DateOnly date, int? siteId);
    Task<ClosureDay> CreateAsync(ClosureDay closureDay);
    Task DeleteAsync(ClosureDay closureDay);
}
