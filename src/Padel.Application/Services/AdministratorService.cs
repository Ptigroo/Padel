using Padel.Application.DTOs;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Application.Services;

public class AdministratorService(IAdministratorRepository repository, ISiteRepository siteRepository) : IAdministratorService
{
    public async Task<List<AdministratorDto>> GetAllAsync()
    {
        var administrators = await repository.GetAllAsync();
        return administrators.Select(MapToDto).ToList();
    }

    public async Task<AdministratorDto?> GetByIdAsync(int id)
    {
        var administrator = await repository.GetByIdAsync(id);
        return administrator == null ? null : MapToDto(administrator);
    }

    public async Task<AdministratorDto?> GetByUsernameAsync(string username)
    {
        var administrator = await repository.GetByUsernameAsync(username);
        return administrator == null ? null : MapToDto(administrator);
    }

    public async Task<List<AdministratorDto>> GetBySiteIdAsync(int siteId)
    {
        var administrators = await repository.GetBySiteIdAsync(siteId);
        return administrators.Select(MapToDto).ToList();
    }

    public async Task<AdministratorDto> CreateAsync(CreateAdministratorDto dto)
    {
        // Validation : si type Site, SiteId obligatoire
        if (dto.Type == AdministratorType.Site && dto.SiteId == null)
        {
            throw new InvalidOperationException("SiteId is required for Site administrators.");
        }

        // Validation : si type Global, SiteId doit être null
        if (dto.Type == AdministratorType.Global && dto.SiteId != null)
        {
            throw new InvalidOperationException("Global administrators cannot have a SiteId.");
        }

        // Validation : vérifier que le site existe
        if (dto.SiteId.HasValue)
        {
            var site = await siteRepository.GetByIdAsync(dto.SiteId.Value);
            if (site == null)
            {
                throw new InvalidOperationException($"Site with ID {dto.SiteId.Value} not found.");
            }
        }

        // Génération du username
        var username = await repository.GetNextUsernameAsync(dto.Type);

        var administrator = new Administrator
        {
            Username = username,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Type = dto.Type,
            SiteId = dto.SiteId
        };

        var created = await repository.CreateAsync(administrator);
        return MapToDto(created);
    }

    public async Task DeleteAsync(int id)
    {
        await repository.DeleteAsync(id);
    }

    private static AdministratorDto MapToDto(Administrator administrator)
    {
        return new AdministratorDto
        {
            Id = administrator.Id,
            Username = administrator.Username,
            FirstName = administrator.FirstName,
            LastName = administrator.LastName,
            Email = administrator.Email,
            Type = administrator.Type.ToString(),
            SiteId = administrator.SiteId,
            SiteName = administrator.Site?.Name
        };
    }
}
