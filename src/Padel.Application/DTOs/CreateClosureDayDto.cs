namespace Padel.Application.DTOs;

public class CreateClosureDayDto
{
    public DateOnly Date { get; set; }
    public string? Reason { get; set; }
    public int? SiteId { get; set; }
}
