namespace Padel.Application.DTOs;

public class MatchDto
{
    public int Id { get; set; }
    public int CourtId { get; set; }
    public required string CourtName { get; set; }
    public int SiteId { get; set; }
    public required string SiteName { get; set; }
    public int OrganizerId { get; set; }
    public required string OrganizerMatricule { get; set; }
    public required string OrganizerName { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime EndsAt { get; set; }
    public required string MatchType { get; set; }
    public required string Status { get; set; }
    public int PlayerCount { get; set; }
    public List<MatchPlayerDto> Players { get; set; } = [];
}
