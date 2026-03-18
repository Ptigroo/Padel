using Padel.Frontend.Models;

namespace Padel.Frontend.Services;

public interface IClosureDayService
{
    Task<List<ClosureDayDto>> GetBySiteIdAsync(int? siteId);
    Task<ClosureDayDto?> CreateAsync(CreateClosureDayDto dto);
    Task<bool> DeleteAsync(int id);
}
