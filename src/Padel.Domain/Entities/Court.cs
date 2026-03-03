namespace Padel.Domain.Entities;

public class Court
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int SiteId { get; set; }
    public Site? Site { get; set; }
}
