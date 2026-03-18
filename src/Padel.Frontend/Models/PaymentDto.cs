namespace Padel.Frontend.Models;

public class PaymentDto
{
    public int Id { get; set; }
    public int MatchPlayerId { get; set; }
    public int MatchId { get; set; }
    public int MemberId { get; set; }
    public string Matricule { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string CourtName { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
}
