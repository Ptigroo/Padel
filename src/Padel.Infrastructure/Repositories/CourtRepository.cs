using Microsoft.EntityFrameworkCore;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;
using Padel.Infrastructure.Data;

namespace Padel.Infrastructure.Repositories;

public class CourtRepository(PadelDbContext context) : ICourtRepository
{
    public async Task<IEnumerable<Court>> GetBySiteIdAsync(int siteId)
    {
        return await context.Courts
            .Include(c => c.Site)
            .Where(c => c.SiteId == siteId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Court?> GetByIdAsync(int id)
    {
        return await context.Courts
            .Include(c => c.Site)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Court> CreateAsync(Court court)
    {
        context.Courts.Add(court);
        await context.SaveChangesAsync();
        return court;
    }

    public async Task DeleteAsync(Court court)
    {
        context.Courts.Remove(court);
        await context.SaveChangesAsync();
    }
}
