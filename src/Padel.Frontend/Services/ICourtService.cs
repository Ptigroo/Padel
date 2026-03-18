using Padel.Frontend.Models;

namespace Padel.Frontend.Services;

public interface ICourtService
{
    Task<List<CourtDto>> GetBySiteIdAsync(int siteId);
    Task<CourtDto?> GetByIdAsync(int id);
    Task<CourtDto?> CreateAsync(CreateCourtDto dto);
    Task<bool> DeleteAsync(int id);
    Task<List<SlotDto>> GetAvailableSlotsAsync(int courtId, DateOnly date);
}
