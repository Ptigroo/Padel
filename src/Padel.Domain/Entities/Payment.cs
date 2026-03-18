namespace Padel.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int MatchPlayerId { get; set; }
    public int MatchId { get; set; }
    public int MemberId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public MatchPlayer? MatchPlayer { get; set; }
    public Match? Match { get; set; }
    public Member? Member { get; set; }
}
