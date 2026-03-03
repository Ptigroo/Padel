namespace Padel.Application.DTOs;

public class CreateCourtDto
{
    public required string Name { get; set; }
    public int SiteId { get; set; }
}
