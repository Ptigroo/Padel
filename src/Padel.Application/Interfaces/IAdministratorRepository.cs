using Padel.Domain.Entities;

namespace Padel.Application.Interfaces;

public interface IAdministratorRepository
{
    Task<List<Administrator>> GetAllAsync();
    Task<Administrator?> GetByIdAsync(int id);
    Task<Administrator?> GetByUsernameAsync(string username);
    Task<List<Administrator>> GetBySiteIdAsync(int siteId);
    Task<Administrator> CreateAsync(Administrator administrator);
    Task<Administrator> UpdateAsync(Administrator administrator);
    Task DeleteAsync(int id);
    Task<string> GetNextUsernameAsync(AdministratorType type);
}
