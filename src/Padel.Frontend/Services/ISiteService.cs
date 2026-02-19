using Padel.Frontend.Models;

namespace Padel.Frontend.Services;

public interface ISiteService
{
    Task<List<SiteDto>> GetAllAsync();
    Task<SiteDto?> GetByIdAsync(int id);
    Task<SiteDto?> CreateAsync(CreateSiteDto dto);
    Task<SiteDto?> UpdateAsync(int id, UpdateSiteDto dto);
    Task<bool> DeleteAsync(int id);
}
