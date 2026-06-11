using Padel.Frontend.Models;

namespace Padel.Frontend.Services;

public interface IAdministratorService
{
    Task<List<AdministratorDto>> GetAllAsync();
    Task<AdministratorDto?> GetByUsernameAsync(string username);
    Task<List<AdministratorDto>> GetBySiteIdAsync(int siteId);
    Task<AdministratorDto> CreateAsync(CreateAdministratorDto dto);
}
