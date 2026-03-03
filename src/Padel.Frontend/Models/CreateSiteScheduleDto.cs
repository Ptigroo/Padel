using System.ComponentModel.DataAnnotations;

namespace Padel.Frontend.Models;

public class CreateSiteScheduleDto
{
    public int SiteId { get; set; }

    [Required(ErrorMessage = "L'année est requise.")]
    [Range(2020, 2100, ErrorMessage = "L'année doit être entre 2020 et 2100.")]
    public int Year { get; set; } = DateTime.Now.Year;

    [Required(ErrorMessage = "L'heure de début est requise.")]
    public string StartTime { get; set; } = "08:00";

    [Required(ErrorMessage = "L'heure de fin est requise.")]
    public string EndTime { get; set; } = "22:00";
}
