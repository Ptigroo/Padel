namespace Padel.Application.DTOs;

public class MatchPlayerDto
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public required string Matricule { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateTime JoinedAt { get; set; }
    public string? PaymentStatus { get; set; }
}
