namespace Padel.Application.DTOs;

public class MemberDto
{
    public int Id { get; set; }
    public required string Matricule { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string MemberType { get; set; }
    public int? SiteId { get; set; }
    public string? SiteName { get; set; }
    public bool ReservationBlocked { get; set; }
    public DateTime? BlockedUntil { get; set; }
}
