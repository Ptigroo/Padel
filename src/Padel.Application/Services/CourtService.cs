using Padel.Application.DTOs;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Application.Services;

public class CourtService(
    ICourtRepository courtRepository,
    ISiteRepository siteRepository,
    ISiteScheduleRepository scheduleRepository,
    IClosureDayRepository closureDayRepository) : ICourtService
{
    public async Task<IEnumerable<CourtDto>> GetBySiteIdAsync(int siteId)
    {
        var courts = await courtRepository.GetBySiteIdAsync(siteId);
        return courts.Select(MapToDto);
    }

    public async Task<CourtDto?> GetByIdAsync(int id)
    {
        var court = await courtRepository.GetByIdAsync(id);
        return court is null ? null : MapToDto(court);
    }

    public async Task<CourtDto> CreateAsync(CreateCourtDto dto)
    {
        var site = await siteRepository.GetByIdAsync(dto.SiteId)
            ?? throw new InvalidOperationException($"Site with id {dto.SiteId} not found.");

        var court = new Court
        {
            Name = dto.Name,
            SiteId = dto.SiteId
        };

        var created = await courtRepository.CreateAsync(court);
        created.Site = site;
        return MapToDto(created);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var court = await courtRepository.GetByIdAsync(id);
        if (court is null)
            return false;

        await courtRepository.DeleteAsync(court);
        return true;
    }

    private static CourtDto MapToDto(Court court) => new()
    {
        Id = court.Id,
        Name = court.Name,
        SiteId = court.SiteId,
        SiteName = court.Site?.Name
    };

    public async Task<IEnumerable<SlotDto>> GetAvailableSlotsAsync(int courtId, DateOnly date)
    {
        var court = await courtRepository.GetByIdAsync(courtId)
            ?? throw new InvalidOperationException($"Court with id {courtId} not found.");

        // CF-RV-017: Check closure days
        var isClosed = await closureDayRepository.ExistsForDateAndSiteAsync(date, court.SiteId);
        if (isClosed)
            return [];

        // Get schedule for the year
        var schedule = await scheduleRepository.GetForYearAsync(court.SiteId, date.Year);
        if (schedule is null)
            return [];

        // Get existing matches on this court for the date
        var existingMatches = (await courtRepository.GetMatchesForCourtOnDateAsync(courtId, date)).ToList();

        // CF-RC-002: Calculate slots (1h30 match + 15min pause = 1h45 per slot)
        var matchDuration = TimeSpan.FromMinutes(90);
        var pauseDuration = TimeSpan.FromMinutes(15);
        var slots = new List<SlotDto>();

        var currentStart = schedule.StartTime;
        while (true)
        {
            var slotEnd = currentStart.Add(matchDuration);
            if (slotEnd > schedule.EndTime)
                break;

            var slotStartDt = date.ToDateTime(currentStart);
            var slotEndDt = date.ToDateTime(slotEnd);

            // Check for conflicts with existing matches
            var hasConflict = existingMatches.Any(m =>
                m.ScheduledAt < slotEndDt && m.EndsAt > slotStartDt);

            if (!hasConflict)
            {
                slots.Add(new SlotDto
                {
                    Start = slotStartDt,
                    End = slotEndDt
                });
            }

            currentStart = currentStart.Add(matchDuration).Add(pauseDuration);
        }

        return slots;
    }
}
