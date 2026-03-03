using Padel.Application.DTOs;

namespace Padel.Application.Interfaces;

public interface IMemberService
{
    Task<IEnumerable<MemberDto>> GetAllAsync();
    Task<IEnumerable<MemberDto>> GetBySiteIdAsync(int siteId);
    Task<MemberDto?> GetByMatriculeAsync(string matricule);
    Task<MemberDto> CreateAsync(CreateMemberDto dto);
    Task<MemberDto?> UpdateAsync(string matricule, UpdateMemberDto dto);
    Task<bool> DeleteAsync(string matricule);
}
