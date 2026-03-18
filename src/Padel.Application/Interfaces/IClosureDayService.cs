using Padel.Application.DTOs;

namespace Padel.Application.Interfaces;

public interface IClosureDayService
{
    Task<IEnumerable<ClosureDayDto>> GetBySiteIdAsync(int? siteId);
    Task<ClosureDayDto> CreateAsync(CreateClosureDayDto dto);
    Task<bool> DeleteAsync(int id);
}
