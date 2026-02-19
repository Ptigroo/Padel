using Padel.Application.DTOs;

namespace Padel.Application.Interfaces;

public interface ISiteService
{
    Task<IEnumerable<SiteDto>> GetAllAsync();
    Task<SiteDto?> GetByIdAsync(int id);
    Task<SiteDto> CreateAsync(CreateSiteDto dto);
    Task<SiteDto?> UpdateAsync(int id, UpdateSiteDto dto);
    Task<bool> DeleteAsync(int id);
}
