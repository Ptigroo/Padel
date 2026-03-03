namespace Padel.Frontend.Models;

public class SiteScheduleDto
{
    public int Id { get; set; }
    public int SiteId { get; set; }
    public int Year { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
}
