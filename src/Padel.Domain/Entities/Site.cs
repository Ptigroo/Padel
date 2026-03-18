namespace Padel.Domain.Entities;

public class Site
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public ICollection<Court> Courts { get; set; } = [];
    public ICollection<SiteSchedule> Schedules { get; set; } = [];
    public ICollection<ClosureDay> ClosureDays { get; set; } = [];
    public ICollection<Member> Members { get; set; } = [];
}
