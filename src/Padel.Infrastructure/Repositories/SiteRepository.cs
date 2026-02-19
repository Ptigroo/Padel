using Microsoft.EntityFrameworkCore;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;
using Padel.Infrastructure.Data;

namespace Padel.Infrastructure.Repositories;

public class SiteRepository(PadelDbContext context) : ISiteRepository
{
    public async Task<IEnumerable<Site>> GetAllAsync()
    {
        return await context.Sites.AsNoTracking().ToListAsync();
    }

    public async Task<Site?> GetByIdAsync(int id)
    {
        return await context.Sites.FindAsync(id);
    }

    public async Task<Site> CreateAsync(Site site)
    {
        context.Sites.Add(site);
        await context.SaveChangesAsync();
        return site;
    }

    public async Task<Site> UpdateAsync(Site site)
    {
        context.Sites.Update(site);
        await context.SaveChangesAsync();
        return site;
    }

    public async Task DeleteAsync(Site site)
    {
        context.Sites.Remove(site);
        await context.SaveChangesAsync();
    }
}
