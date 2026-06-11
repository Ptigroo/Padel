using Microsoft.EntityFrameworkCore;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;
using Padel.Infrastructure.Data;

namespace Padel.Infrastructure.Repositories;

public class AdministratorRepository(PadelDbContext context) : IAdministratorRepository
{
    public async Task<List<Administrator>> GetAllAsync()
    {
        return await context.Administrators
            .Include(a => a.Site)
            .OrderBy(a => a.Username)
            .ToListAsync();
    }

    public async Task<Administrator?> GetByIdAsync(int id)
    {
        return await context.Administrators
            .Include(a => a.Site)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Administrator?> GetByUsernameAsync(string username)
    {
        return await context.Administrators
            .Include(a => a.Site)
            .FirstOrDefaultAsync(a => a.Username == username);
    }

    public async Task<List<Administrator>> GetBySiteIdAsync(int siteId)
    {
        return await context.Administrators
            .Include(a => a.Site)
            .Where(a => a.SiteId == siteId)
            .OrderBy(a => a.Username)
            .ToListAsync();
    }

    public async Task<Administrator> CreateAsync(Administrator administrator)
    {
        context.Administrators.Add(administrator);
        await context.SaveChangesAsync();
        return administrator;
    }

    public async Task<Administrator> UpdateAsync(Administrator administrator)
    {
        context.Administrators.Update(administrator);
        await context.SaveChangesAsync();
        return administrator;
    }

    public async Task DeleteAsync(int id)
    {
        var administrator = await GetByIdAsync(id);
        if (administrator != null)
        {
            context.Administrators.Remove(administrator);
            await context.SaveChangesAsync();
        }
    }

    public async Task<string> GetNextUsernameAsync(AdministratorType type)
    {
        var prefix = type == AdministratorType.Global ? "AG" : "AS";
        var length = type == AdministratorType.Global ? 4 : 5;

        var lastUsername = await context.Administrators
            .Where(a => a.Username.StartsWith(prefix))
            .OrderByDescending(a => a.Username)
            .Select(a => a.Username)
            .FirstOrDefaultAsync();

        if (lastUsername == null)
        {
            return type == AdministratorType.Global ? "AG0001" : "AS00001";
        }

        var numericPart = lastUsername[prefix.Length..];
        if (int.TryParse(numericPart, out var number))
        {
            return $"{prefix}{(number + 1).ToString().PadLeft(length, '0')}";
        }

        return type == AdministratorType.Global ? "AG0001" : "AS00001";
    }
}
