namespace Padel.Domain.Entities;

public class ClosureDay
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string? Reason { get; set; }
    public int? SiteId { get; set; }
    public Site? Site { get; set; }
}
