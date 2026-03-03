namespace Padel.Domain.Entities;

public class SiteSchedule
{
    public int Id { get; set; }
    public int SiteId { get; set; }
    public int Year { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public Site? Site { get; set; }
}
