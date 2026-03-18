namespace Padel.Application.DTOs;

public class PaymentDto
{
    public int Id { get; set; }
    public int MatchPlayerId { get; set; }
    public int MatchId { get; set; }
    public int MemberId { get; set; }
    public required string Matricule { get; set; }
    public decimal Amount { get; set; }
    public required string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public required string CourtName { get; set; }
    public required string SiteName { get; set; }
    public DateTime ScheduledAt { get; set; }
}
