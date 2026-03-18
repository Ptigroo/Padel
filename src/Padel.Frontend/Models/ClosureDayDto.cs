namespace Padel.Frontend.Models;

public class ClosureDayDto
{
    public int Id { get; set; }
    public string Date { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public int? SiteId { get; set; }
    public string? SiteName { get; set; }
    public bool IsGlobal => SiteId is null;
}
