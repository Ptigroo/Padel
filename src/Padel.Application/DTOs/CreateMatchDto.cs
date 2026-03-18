namespace Padel.Application.DTOs;

public class CreateMatchDto
{
    public int CourtId { get; set; }
    public required string OrganizerMatricule { get; set; }
    public DateTime ScheduledAt { get; set; }
    public required string MatchType { get; set; }
}
