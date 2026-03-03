using Padel.Domain.Entities;

namespace Padel.Application.Interfaces;

public interface ICourtRepository
{
    Task<IEnumerable<Court>> GetBySiteIdAsync(int siteId);
    Task<Court?> GetByIdAsync(int id);
    Task<Court> CreateAsync(Court court);
    Task DeleteAsync(Court court);
}
