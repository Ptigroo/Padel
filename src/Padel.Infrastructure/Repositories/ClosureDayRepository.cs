using Microsoft.EntityFrameworkCore;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;
using Padel.Infrastructure.Data;

namespace Padel.Infrastructure.Repositories;

public class ClosureDayRepository(PadelDbContext context) : IClosureDayRepository
{
    public async Task<IEnumerable<ClosureDay>> GetBySiteIdAsync(int? siteId)
    {
        var query = context.ClosureDays
            .Include(c => c.Site)
            .AsNoTracking();

        if (siteId.HasValue)
        {
            // Return global closures + site-specific closures
            query = query.Where(c => c.SiteId == null || c.SiteId == siteId.Value);
        }

        return await query
            .OrderByDescending(c => c.Date)
            .ToListAsync();
    }

    public async Task<ClosureDay?> GetByIdAsync(int id)
    {
        return await context.ClosureDays
            .Include(c => c.Site)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> ExistsForDateAndSiteAsync(DateOnly date, int? siteId)
    {
        return await context.ClosureDays
            .AnyAsync(c => c.Date == date && (c.SiteId == null || c.SiteId == siteId));
    }

    public async Task<ClosureDay> CreateAsync(ClosureDay closureDay)
    {
        context.ClosureDays.Add(closureDay);
        await context.SaveChangesAsync();
        return closureDay;
    }

    public async Task DeleteAsync(ClosureDay closureDay)
    {
        context.ClosureDays.Remove(closureDay);
        await context.SaveChangesAsync();
    }
}
