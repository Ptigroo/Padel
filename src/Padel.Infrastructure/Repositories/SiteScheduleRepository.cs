using Microsoft.EntityFrameworkCore;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;
using Padel.Infrastructure.Data;

namespace Padel.Infrastructure.Repositories;

public class SiteScheduleRepository(PadelDbContext context) : ISiteScheduleRepository
{
    public async Task<IEnumerable<SiteSchedule>> GetBySiteIdAsync(int siteId)
    {
        return await context.SiteSchedules
            .Where(s => s.SiteId == siteId)
            .OrderByDescending(s => s.Year)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<SiteSchedule?> GetByIdAsync(int id)
    {
        return await context.SiteSchedules.FindAsync(id);
    }

    public async Task<bool> ExistsAsync(int siteId, int year)
    {
        return await context.SiteSchedules
            .AnyAsync(s => s.SiteId == siteId && s.Year == year);
    }

    public async Task<SiteSchedule> CreateAsync(SiteSchedule schedule)
    {
        context.SiteSchedules.Add(schedule);
        await context.SaveChangesAsync();
        return schedule;
    }

    public async Task DeleteAsync(SiteSchedule schedule)
    {
        context.SiteSchedules.Remove(schedule);
        await context.SaveChangesAsync();
    }
}
