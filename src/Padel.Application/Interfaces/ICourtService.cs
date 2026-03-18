using Padel.Application.DTOs;

namespace Padel.Application.Interfaces;

public interface ICourtService
{
    Task<IEnumerable<CourtDto>> GetBySiteIdAsync(int siteId);
    Task<CourtDto?> GetByIdAsync(int id);
    Task<CourtDto> CreateAsync(CreateCourtDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<SlotDto>> GetAvailableSlotsAsync(int courtId, DateOnly date);
}
