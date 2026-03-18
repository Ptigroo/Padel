namespace Padel.Application.DTOs;

public class GlobalStatsDto
{
    public int TotalSites { get; set; }
    public int TotalMembers { get; set; }
    public int TotalMatches { get; set; }
    public decimal TotalRevenue { get; set; }
    public int MatchesScheduled { get; set; }
    public int MatchesFull { get; set; }
    public int MatchesCompleted { get; set; }
    public int MatchesCancelled { get; set; }
}
