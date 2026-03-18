namespace Padel.Domain.Entities;

public class Match
{
    public int Id { get; set; }
    public int CourtId { get; set; }
    public int OrganizerId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime EndsAt { get; set; }
    public MatchType MatchType { get; set; }
    public MatchStatus Status { get; set; }
    public Court? Court { get; set; }
    public Member? Organizer { get; set; }
    public ICollection<MatchPlayer> Players { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}
