namespace Padel.Application.DTOs;

public class ClosureDayDto
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string? Reason { get; set; }
    public int? SiteId { get; set; }
    public string? SiteName { get; set; }
    public bool IsGlobal => SiteId is null;
}
