using Padel.Frontend.Models;

namespace Padel.Frontend.Services;

public interface IMemberService
{
    Task<List<MemberDto>> GetAllAsync();
    Task<List<MemberDto>> GetBySiteIdAsync(int siteId);
    Task<MemberDto?> GetByMatriculeAsync(string matricule);
    Task<MemberDto?> CreateAsync(CreateMemberDto dto);
    Task<MemberDto?> UpdateAsync(string matricule, UpdateMemberDto dto);
    Task<bool> DeleteAsync(string matricule);
}
