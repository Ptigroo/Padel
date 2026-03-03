namespace Padel.Frontend.Models;

public class MemberDto
{
    public int Id { get; set; }
    public string Matricule { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MemberType { get; set; } = string.Empty;
    public int? SiteId { get; set; }
    public string? SiteName { get; set; }
    public bool ReservationBlocked { get; set; }
    public DateTime? BlockedUntil { get; set; }
}
