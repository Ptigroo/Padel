using Padel.Domain.Entities;

namespace Padel.Application.Interfaces;

public interface IMemberRepository
{
    Task<IEnumerable<Member>> GetAllAsync();
    Task<IEnumerable<Member>> GetBySiteIdAsync(int siteId);
    Task<Member?> GetByIdAsync(int id);
    Task<Member?> GetByMatriculeAsync(string matricule);
    Task<string?> GetLastMatriculeByTypeAsync(MemberType memberType);
    Task<Member> CreateAsync(Member member);
    Task<Member> UpdateAsync(Member member);
    Task DeleteAsync(Member member);
}
