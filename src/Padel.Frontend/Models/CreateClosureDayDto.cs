using System.ComponentModel.DataAnnotations;

namespace Padel.Frontend.Models;

public class CreateClosureDayDto
{
    [Required(ErrorMessage = "La date est requise.")]
    public string Date { get; set; } = DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd");

    public string? Reason { get; set; }

    public int? SiteId { get; set; }
}
