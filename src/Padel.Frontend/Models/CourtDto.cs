namespace Padel.Frontend.Models;

public class CourtDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SiteId { get; set; }
    public string? SiteName { get; set; }
}
