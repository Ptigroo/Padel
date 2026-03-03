using Microsoft.EntityFrameworkCore;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;
using Padel.Infrastructure.Data;

namespace Padel.Infrastructure.Repositories;

public class MemberRepository(PadelDbContext context) : IMemberRepository
{
    public async Task<IEnumerable<Member>> GetAllAsync()
    {
        return await context.Members
            .Include(m => m.Site)
            .AsNoTracking()
            .OrderBy(m => m.Matricule)
            .ToListAsync();
    }

    public async Task<IEnumerable<Member>> GetBySiteIdAsync(int siteId)
    {
        return await context.Members
            .Include(m => m.Site)
            .Where(m => m.SiteId == siteId)
            .AsNoTracking()
            .OrderBy(m => m.Matricule)
            .ToListAsync();
    }

    public async Task<Member?> GetByIdAsync(int id)
    {
        return await context.Members
            .Include(m => m.Site)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Member?> GetByMatriculeAsync(string matricule)
    {
        return await context.Members
            .Include(m => m.Site)
            .FirstOrDefaultAsync(m => m.Matricule == matricule);
    }

    public async Task<string?> GetLastMatriculeByTypeAsync(MemberType memberType)
    {
        return await context.Members
            .Where(m => m.MemberType == memberType)
            .OrderByDescending(m => m.Matricule)
            .Select(m => m.Matricule)
            .FirstOrDefaultAsync();
    }

    public async Task<Member> CreateAsync(Member member)
    {
        context.Members.Add(member);
        await context.SaveChangesAsync();
        return member;
    }

    public async Task<Member> UpdateAsync(Member member)
    {
        context.Members.Update(member);
        await context.SaveChangesAsync();
        return member;
    }

    public async Task DeleteAsync(Member member)
    {
        context.Members.Remove(member);
        await context.SaveChangesAsync();
    }
}
