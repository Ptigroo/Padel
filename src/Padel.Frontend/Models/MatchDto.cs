namespace Padel.Frontend.Models;

public class MatchDto
{
    public int Id { get; set; }
    public int CourtId { get; set; }
    public string CourtName { get; set; } = string.Empty;
    public int SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public int OrganizerId { get; set; }
    public string OrganizerMatricule { get; set; } = string.Empty;
    public string OrganizerName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string MatchType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int PlayerCount { get; set; }
    public List<MatchPlayerDto> Players { get; set; } = [];
}
