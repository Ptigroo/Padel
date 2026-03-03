namespace Padel.Application.DTOs;

public class CourtDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int SiteId { get; set; }
    public string? SiteName { get; set; }
}
