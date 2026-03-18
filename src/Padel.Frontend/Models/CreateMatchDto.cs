using System.ComponentModel.DataAnnotations;

namespace Padel.Frontend.Models;

public class CreateMatchDto
{
    [Required]
    public int CourtId { get; set; }

    [Required(ErrorMessage = "Le matricule est requis.")]
    public string OrganizerMatricule { get; set; } = string.Empty;

    [Required]
    public DateTime ScheduledAt { get; set; } = DateTime.Today.AddDays(1).AddHours(9);

    [Required(ErrorMessage = "Le type de match est requis.")]
    public string MatchType { get; set; } = "Private";
}
