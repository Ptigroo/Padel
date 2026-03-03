using Padel.Application.DTOs;
using Padel.Application.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Application.Services;

public class ScheduleService(ISiteScheduleRepository scheduleRepository, ISiteRepository siteRepository) : IScheduleService
{
    public async Task<IEnumerable<SiteScheduleDto>> GetBySiteIdAsync(int siteId)
    {
        var schedules = await scheduleRepository.GetBySiteIdAsync(siteId);
        return schedules.Select(MapToDto);
    }

    public async Task<SiteScheduleDto> CreateAsync(CreateSiteScheduleDto dto)
    {
        _ = await siteRepository.GetByIdAsync(dto.SiteId)
            ?? throw new InvalidOperationException($"Site with id {dto.SiteId} not found.");

        if (dto.StartTime >= dto.EndTime)
            throw new InvalidOperationException("Start time must be before end time.");

        if (await scheduleRepository.ExistsAsync(dto.SiteId, dto.Year))
            throw new InvalidOperationException($"A schedule already exists for site {dto.SiteId} and year {dto.Year}.");

        var schedule = new SiteSchedule
        {
            SiteId = dto.SiteId,
            Year = dto.Year,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime
        };

        var created = await scheduleRepository.CreateAsync(schedule);
        return MapToDto(created);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var schedule = await scheduleRepository.GetByIdAsync(id);
        if (schedule is null)
            return false;

        await scheduleRepository.DeleteAsync(schedule);
        return true;
    }

    private static SiteScheduleDto MapToDto(SiteSchedule schedule) => new()
    {
        Id = schedule.Id,
        SiteId = schedule.SiteId,
        Year = schedule.Year,
        StartTime = schedule.StartTime,
        EndTime = schedule.EndTime
    };
}
