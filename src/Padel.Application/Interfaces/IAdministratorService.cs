using Padel.Application.DTOs;

namespace Padel.Application.Interfaces;

public interface IAdministratorService
{
    Task<List<AdministratorDto>> GetAllAsync();
    Task<AdministratorDto?> GetByIdAsync(int id);
    Task<AdministratorDto?> GetByUsernameAsync(string username);
    Task<List<AdministratorDto>> GetBySiteIdAsync(int siteId);
    Task<AdministratorDto> CreateAsync(CreateAdministratorDto dto);
    Task DeleteAsync(int id);
}
