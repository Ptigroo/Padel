using Padel.Domain.Entities;

namespace Padel.Application.Interfaces;

public interface ISiteRepository
{
    Task<IEnumerable<Site>> GetAllAsync();
    Task<Site?> GetByIdAsync(int id);
    Task<Site> CreateAsync(Site site);
    Task<Site> UpdateAsync(Site site);
    Task DeleteAsync(Site site);
}
