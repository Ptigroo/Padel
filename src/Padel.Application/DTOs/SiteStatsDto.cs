namespace Padel.Application.DTOs;

public class SiteStatsDto
{
    public int SiteId { get; set; }
    public required string SiteName { get; set; }
    public int TotalCourts { get; set; }
    public int TotalMembers { get; set; }
    public int TotalMatches { get; set; }
    public decimal Revenue { get; set; }
    public int MatchesScheduled { get; set; }
    public int MatchesFull { get; set; }
    public int MatchesCompleted { get; set; }
    public int MatchesCancelled { get; set; }
}
