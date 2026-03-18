namespace Padel.Domain.Entities;

public class Member
{
    public int Id { get; set; }
    public required string Matricule { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public MemberType MemberType { get; set; }
    public int? SiteId { get; set; }
    public Site? Site { get; set; }
    public bool ReservationBlocked { get; set; }
    public DateTime? BlockedUntil { get; set; }
    public ICollection<Match> OrganizedMatches { get; set; } = [];
    public ICollection<MatchPlayer> MatchPlayers { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}
