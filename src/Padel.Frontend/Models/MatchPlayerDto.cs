namespace Padel.Frontend.Models;

public class MatchPlayerDto
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string Matricule { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public string? PaymentStatus { get; set; }
}
