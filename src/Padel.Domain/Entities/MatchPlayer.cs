namespace Padel.Domain.Entities;

public class MatchPlayer
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public int MemberId { get; set; }
    public DateTime JoinedAt { get; set; }
    public Match? Match { get; set; }
    public Member? Member { get; set; }
    public Payment? Payment { get; set; }
}
