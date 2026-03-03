namespace Padel.Application.DTOs;

public class SiteScheduleDto
{
    public int Id { get; set; }
    public int SiteId { get; set; }
    public int Year { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
